using Gamelib.Extensions;
using Facepunch.RTS.Buildings;
using Sandbox;
using System.Linq;
using Gamelib.FlowFields.Entities;
using Facepunch.RTS;

namespace Facepunch.RTS
{
	public partial class GhostBuilding : ModelEntity
	{
		public GhostBounds BoundsEntity { get; private set; }
		public BaseBuilding Building { get; private set; }
		public UnitEntity Worker { get; private set; }
		public bool IsTouchingBlocker { get; private set; }
		public override Vector3 Position
		{
			get => base.Position;
			set
			{
				base.Position = value;
				PhysicsBody.Position = value;
			}
		}

		public GhostBuilding()
		{
			Transmit = TransmitType.Never;
		}

		public void SetWorkerAndBuilding( UnitEntity worker, BaseBuilding building )
		{
			SetModel( building.Model );

			RenderColor = RenderColor.WithAlpha( 0.5f );

			/*
			GlowActive = true;
			GlowColor = Color.Green;
			GlowState = GlowStates.GlowStateOn;
			*/

			EnableTouch = true;
			EnableSolidCollisions = false;

			Tags.Add( "ghostbuilding" );

			SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

			Building = building;
			Worker = worker;

			if ( IsClient )
			{
				var material = Material.Load( "materials/rts/blueprint.vmat" );

				if ( material != null )
					SceneObject.SetMaterialOverride( material );

				if ( IsClient )
				{
					BoundsEntity = new GhostBounds();
					BoundsEntity.RenderBounds = CollisionBounds * 1.25f;
					BoundsEntity.SetParent( this );
					BoundsEntity.Position = Position;
					BoundsEntity.Color = Color.Green;
					BoundsEntity.Alpha = 0.5f;
				}
			}
		}

		public void ShowValid()
		{
			if ( BoundsEntity.IsValid() )
				BoundsEntity.Color = Color.Green;

			RenderColor = Color.White;
			//GlowColor = Color.Green;
		}

		public void ShowInvalid()
		{
			if ( BoundsEntity.IsValid() )
				BoundsEntity.Color = Color.Red;

			RenderColor = Color.Red;
			//GlowColor = Color.Red;
		}

		public TraceResult GetPlacementTrace( Client client, Vector3 cursorOrigin, Vector3 cursorAim )
		{
			return TraceExtension.RayDirection( cursorOrigin, cursorAim )
				.WorldOnly()
				.Run();
		}

		private bool IsBlockingEntity( Entity entity )
		{
			if ( entity == this || entity is FlowFieldGround )
				return false;

			if ( entity is BuildingEntity building )
			{
				if ( building.Player != Worker.Player && building.IsBlueprint )
					return false;
			}

			return true;
		}

		public bool IsPlacementValid( TraceResult trace )
		{
			if ( !Worker.IsValid() ) return false;

			if ( IsTouchingBlocker ) return false;

			var position = trace.EndPosition;
			var bounds = CollisionBounds * 1.5f;
			var entities = Entity.FindInBox( bounds + position )
				.Where( e => IsBlockingEntity( e ) );

			/*
			if ( IsClient && !Fog.IsAreaSeen( position ) )
				return false;
			*/

			if ( entities.Count() > 0 )
				return false;

			if ( Worker.Item.ConstructRadius > 0f )
			{
				if ( position.Distance( Worker.Position ) > Worker.Item.ConstructRadius )
					return false;
			}

			var groundBounds = bounds;
			groundBounds.Mins = groundBounds.Mins.WithZ( 0f );
			groundBounds.Maxs = groundBounds.Maxs.WithZ( 1f );

			var worldTrace = Trace.Ray( position + Vector3.Up, position + Vector3.Up )
				.WorldOnly()
				.Size( groundBounds )
				.Run();

			if ( worldTrace.Hit )
				return false;

			var verticality = trace.Normal.Dot( Vector3.Up );

			return (verticality >= 0.9f);
		}

		public override void StartTouch( Entity other )
		{
			if ( other is BuildingBlocker )
			{
				IsTouchingBlocker = true;
			}

			base.StartTouch( other );
		}

		public override void EndTouch( Entity other )
		{
			if ( other is BuildingBlocker )
			{
				IsTouchingBlocker = false;
			}

			base.EndTouch( other );
		}

		protected override void OnDestroy()
		{
			if ( BoundsEntity.IsValid() )
				BoundsEntity.Delete();

			base.OnDestroy();
		}
	}
}
