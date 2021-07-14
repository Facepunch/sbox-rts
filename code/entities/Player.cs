using Sandbox;
using Gamelib.Elo;
using System.Collections.Generic;
using Facepunch.RTS.Buildings;
using System.Linq;
using Facepunch.RTS.Units;
using System;
using Gamelib.Extensions;
using Facepunch.RTS;
using Facepunch.RTS.Managers;

namespace Facepunch.RTS
{
	public partial class Player : Entity
	{
		[Net, Local, OnChangedCallback] public List<uint> Dependencies { get; set; }
		[Net, Local, OnChangedCallback] public List<uint> Researching { get; set; }
		[Net, Local, OnChangedCallback] public List<Entity> Selection { get; set; }
		[Net, Local] public uint MaxPopulation { get; set; }
		[Net, Local] public int Population { get; private set; }
		[Net, Local, Predicted] public float ZoomLevel { get; set; }
		[Net] public bool IsSpectator { get; private set;  }
		[Net] public EloScore Elo { get; private set; }
		[Net] public Color TeamColor { get; set; }
		[Net] public List<int> Resources { get; private set; }

		public Player()
		{
			Elo = new();
			Camera = new RTSCamera();
			Transmit = TransmitType.Always;
			Resources = new List<int>();
			ZoomLevel = 1f;
			Selection = new List<Entity>();
			Researching = new List<uint>();
			Dependencies = new List<uint>();
			MaxPopulation = 100;
		}

		public List<UnitEntity> GetUnits( BaseUnit unit)
		{
			return All.OfType<UnitEntity>().Where( i => i.Player == this && i.Item == unit ).ToList();
		}

		public List<BuildingEntity> GetBuildings( BaseBuilding building )
		{
			return All.OfType<BuildingEntity>().Where( i => i.Player == this && i.Item == building ).ToList();
		}

		public List<BuildingEntity> GetBuildings()
		{
			return All.OfType<BuildingEntity>().Where( i => i.Player == this ).ToList();
		}

		public void AddPopulation( int population )
		{
			Population += population;
		}

		public void TakePopulation( int population )
		{
			Population = Math.Max( Population - population, 0 );
		}

		public List<T> GetSelected<T>() where T : ISelectable
		{
			var output = new List<T>();

			foreach ( var entity in Selection )
			{
				if ( entity is T selected )
				{
					output.Add( selected );
				}
			}

			return output;
		}

		public List<T> ForEachSelected<T>( Func<T, bool> callback ) where T : ISelectable
		{
			var output = new List<T>();

			foreach ( var entity in Selection )
			{
				if ( entity is T selected )
				{
					if ( callback( selected ) )
						output.Add( selected );
				}
			}

			return output;
		}

		public bool HasPopulationCapacity( int population )
		{
			return (Population + population <= MaxPopulation);
		}

		public bool CanAfford( Dictionary<ResourceType, int> costs, out ResourceType missingResource )
		{
			foreach ( var kv in costs )
			{
				if ( GetResource( kv.Key ) < kv.Value )
				{
					missingResource = kv.Key;
					return false;
				}
			}

			missingResource = default;

			return true;
		}

		public bool CanAfford( Dictionary<ResourceType,int> costs )
		{
			foreach ( var kv in costs )
			{
				if ( GetResource( kv.Key ) < kv.Value )
					return false;
			}

			return true;
		}
		public bool CanAfford( BaseAbility ability )
		{
			return CanAfford( ability.Costs );
		}

		public bool CanAfford( BaseAbility ability, out ResourceType missingResource )
		{
			return CanAfford( ability.Costs, out missingResource );
		}

		public bool CanAfford( BaseItem item )
		{
			return CanAfford( item.Costs );
		}

		public bool CanAfford( BaseItem item, out ResourceType missingResource )
		{
			return CanAfford( item.Costs, out missingResource );
		}

		public void GiveResources( Dictionary<ResourceType, int> resources, int multiplier = 1 )
		{
			foreach ( var kv in resources )
			{
				GiveResource( kv.Key, kv.Value * multiplier );
			}
		}

		public void TakeResources( Dictionary<ResourceType, int> resources )
		{
			foreach ( var kv in resources )
			{
				TakeResource( kv.Key, kv.Value );
			}
		}

		public void GiveResources( BaseAbility ability )
		{
			GiveResources( ability.Costs );
		}

		public void TakeResources( BaseAbility ability )
		{
			TakeResources( ability.Costs );
		}

		public void GiveResources( BaseItem item )
		{
			GiveResources( item.Costs );
		}

		public void TakeResources( BaseItem item )
		{
			TakeResources( item.Costs );
		}

		public void ClearResources()
		{
			Resources.Clear();
		}

		public int GetResource( ResourceType type )
		{
			if ( Resources == null ) return 0;
			if ( Resources.Count <= (int)type ) return 0;
			return Resources[(int)type];
		}

		public bool SetResource( ResourceType type, int amount )
		{
			if ( !Host.IsServer ) return false;
			if ( Resources == null ) return false;

			while ( Resources.Count <= (int)type )
			{
				Resources.Add( 0 );
			}

			Resources[(int)type] = amount;
			return true;
		}

		public bool GiveResource( ResourceType type, int amount )
		{
			if ( !Host.IsServer ) return false;
			if ( Resources == null ) return false;

			SetResource( type, GetResource( type ) + amount );
			return true;
		}

		public int TakeResource( ResourceType type, int amount )
		{
			if ( Resources == null ) return 0;

			var available = GetResource( type );
			amount = Math.Min( available, amount );

			SetResource( type, available - amount );
			return amount;
		}

		public void MakeSpectator( bool isSpectator )
		{
			IsSpectator = isSpectator;
		}

		public void RemoveDependency( BaseItem item )
		{
			if ( Dependencies.Contains( item.NetworkId ) )
				Dependencies.Remove( item.NetworkId );
		}

		public void AddDependency( BaseItem item )
		{
			if ( !Dependencies.Contains( item.NetworkId ) )
				Dependencies.Add( item.NetworkId );
		}

		public void ClearSelection()
		{
			Host.AssertServer();

			for ( var i = Selection.Count - 1; i >= 0; i-- )
			{
				var entity = Selection[i];

				if ( entity is not ISelectable selectable )
					continue;

				if ( !entity.IsValid() )
					continue;

				if ( selectable.IsSelected )
					selectable.Deselect();
			}

			Selection.Clear();
		}

		public void LookAt( Entity other )
		{
			Position = other.Position.WithZ( 0f );
		}

		public override void Simulate( Client client )
		{
			var cameraConfig = RTS.Game.Config.Camera;
			var velocity = Vector3.Zero;
			var panSpeed = cameraConfig.PanSpeed - (cameraConfig.PanSpeed * ZoomLevel * 0.6f);

			if ( Input.Down( InputButton.Forward ) )
				velocity += EyeRot.Forward.WithZ(0f) * panSpeed * Time.Delta;

			if ( Input.Down( InputButton.Back ) )
				velocity += EyeRot.Backward.WithZ( 0f ) * panSpeed * Time.Delta;

			if ( Input.Down( InputButton.Left ) )
				velocity += EyeRot.Left * panSpeed * Time.Delta;

			if ( Input.Down( InputButton.Right ) )
				velocity += EyeRot.Right * panSpeed * Time.Delta;

			Position = (Position + velocity);

			if ( cameraConfig.Ortho )
			{
				EyePos = Position + Vector3.Backward * cameraConfig.Backward;
				EyePos += Vector3.Left * cameraConfig.Left;
				EyePos += Vector3.Up * cameraConfig.Up;
			}
			else
			{
				EyePos = Position + Vector3.Backward * (cameraConfig.Backward - (cameraConfig.Backward * ZoomLevel * cameraConfig.ZoomScale));
				EyePos += Vector3.Left * (cameraConfig.Left - (cameraConfig.Left * ZoomLevel * cameraConfig.ZoomScale));
				EyePos += Vector3.Up * (cameraConfig.Up - (cameraConfig.Up * ZoomLevel * cameraConfig.ZoomScale));
			}

			var difference = Position - EyePos;
			EyeRot = Rotation.LookAt( difference, Vector3.Up );

			ZoomLevel += Input.MouseWheel * Time.Delta * 10f;
			ZoomLevel = ZoomLevel.Clamp( 0f, 1f );

			if ( IsServer && Input.Released( InputButton.Use ) )
			{
				// TODO: This is just for testing, delete later.
				var trace = TraceExtension.RayDirection( Input.Cursor.Origin, Input.Cursor.Direction ).Run();
				var bot = RTS.Game.Round.Players.Where( player => player.GetClientOwner() != client ).FirstOrDefault();

				var worker = Items.CreateUnit( bot, "unit.worker" );
				worker.Position = trace.EndPos;
			}

			base.Simulate( client );
		}

		private void OnResearchingChanged()
		{
			SelectedItem.Instance.Update( Selection );
		}

		private void OnSelectionChanged()
		{
			SelectedItem.Instance.Update( Selection );
		}

		private void OnDependenciesChanged()
		{
			SelectedItem.Instance.Update( Selection );
		}
	}
}
