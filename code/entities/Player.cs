using Sandbox;
using Gamelib.Elo;
using System.Collections.Generic;
using RTS.Buildings;
using System.Linq;
using RTS.Units;

namespace RTS
{
	public partial class Player : Entity
	{
		[Net, Local] public List<uint> Dependencies { get; set; }
		[Net, Local] public List<Entity> Selection { get; set; }
		[Net] public bool IsSpectator { get; private set;  }
		[Net] public EloScore Elo { get; private set; }
		[Net] public Color TeamColor { get; set; }

		public Player()
		{
			Elo = new();
			Camera = new RTSCamera();
			TeamColor = Color.Random;
			Transmit = TransmitType.Always;
			Selection = new List<Entity>();
			Dependencies = new List<uint>();
		}

		public List<UnitEntity> GetUnits( BaseUnit unit)
		{
			return All.OfType<UnitEntity>().Where( i => i.Player == this && i.Item == unit ).ToList();
		}

		public List<BuildingEntity> GetBuildings( BaseBuilding building )
		{
			return All.OfType<BuildingEntity>().Where( i => i.Player == this && i.Item == building ).ToList();
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
				if ( Selection[i] is not ISelectable selectable )
					continue;

				if ( selectable.IsSelected )
					selectable.Deselect();
			}

			Selection.Clear();
		}

		public void LookAt( Entity other )
		{
			Position = Position.WithX( other.Position.x ).WithY( other.Position.y );
		}

		public override void Simulate( Client client )
		{
			var zoomOutDistance = 1500f;
			var velocity = Vector3.Zero;
			var panSpeed = 1000f;

			if ( Input.Down( InputButton.Forward ) )
				velocity.x += panSpeed * Time.Delta;

			if ( Input.Down( InputButton.Back ) )
				velocity.x -= panSpeed * Time.Delta;

			if ( Input.Down( InputButton.Left ) )
				velocity.y += panSpeed * Time.Delta;

			if ( Input.Down( InputButton.Right ) )
				velocity.y -= panSpeed * Time.Delta;

			Position = (Position + velocity).WithZ( zoomOutDistance );
			Rotation = Rotation.LookAt( new Vector3(0.1f, 0f, -1f) );

			base.Simulate( client );
		}
	}
}
