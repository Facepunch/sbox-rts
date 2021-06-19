using Sandbox;
using Gamelib.Elo;
using System.Collections.Generic;
using RTS.Buildings;
using System.Linq;
using RTS.Units;
using System;
using Gamelib.Extensions;

namespace RTS
{
	public partial class Player : Entity
	{
		[Net, Local] public List<uint> Dependencies { get; set; }
		[Net, Local] public List<Entity> Selection { get; set; }
		[Net, Local] public Vector3 StartPosition { get; set; }
		[Net, Local] public float StartLineOfSight { get; set; }
		[Net, Local] public uint MaxPopulation { get; set; }
		[Net, Local] public uint Population { get; private set; }
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
			Dependencies = new List<uint>();
			MaxPopulation = 8;
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

		public void AddPopulation( uint population )
		{
			Population += population;
		}

		public void TakePopulation( uint population )
		{
			Population = Math.Max( Population - population, 0 );
		}

		public bool HasPopulationCapacity( uint population )
		{
			return (Population + population <= MaxPopulation);
		}

		public bool CanAffordItem( BaseItem item )
		{
			foreach ( var kv in item.Costs )
			{
				if ( GetResource( kv.Key ) < kv.Value )
					return false;
			}

			return true;
		}

		public void GiveResourcesForItem( BaseItem item )
		{
			foreach ( var kv in item.Costs )
			{
				GiveResource( kv.Key, kv.Value );
			}
		}

		public void TakeResourcesForItem( BaseItem item )
		{
			foreach ( var kv in item.Costs )
			{
				TakeResource( kv.Key, kv.Value );
			}
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
			EnableFog( To.Single( this ), !isSpectator );
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
			var zoomOutDistance = 5000f - (ZoomLevel * 3000f);
			var velocity = Vector3.Zero;
			var panSpeed = 2000f + (ZoomLevel * 3000f);

			if ( Input.Down( InputButton.Forward ) )
				velocity += EyeRot.Forward.WithZ(0f) * panSpeed * Time.Delta;

			if ( Input.Down( InputButton.Back ) )
				velocity += EyeRot.Backward.WithZ( 0f ) * panSpeed * Time.Delta;

			if ( Input.Down( InputButton.Left ) )
				velocity += EyeRot.Left * panSpeed * Time.Delta;

			if ( Input.Down( InputButton.Right ) )
				velocity += EyeRot.Right * panSpeed * Time.Delta;

			Position = (Position + velocity);
			EyePos = Position + Vector3.Backward * zoomOutDistance * 0.5f;
			EyePos += Vector3.Left * zoomOutDistance * 0.5f;
			EyePos += Vector3.Up * zoomOutDistance;

			var difference = Position - EyePos;
			EyeRot = Rotation.LookAt( difference, Vector3.Up );

			ZoomLevel += Input.MouseWheel * Time.Delta * -10f;
			ZoomLevel = ZoomLevel.Clamp( 0f, 1f );

			base.Simulate( client );
		}

		[ClientRpc]
		private void EnableFog( bool shouldEnable )
		{
			if ( shouldEnable )
				FogManager.Instance.Show();
			else
				FogManager.Instance.Hide();
		}
	}
}
