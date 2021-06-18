using Gamelib.Extensions;
using RTS.Buildings;
using Sandbox;
using System.Linq;

namespace RTS
{
	public partial class GhostBuilding : ModelEntity
	{
		public static Material BlueprintMaterial => Material.Load( "materials/rts/blueprint.vmat" );
		public GhostBounds BoundsEntity { get; private set; }
		public BaseBuilding Building { get; private set; }
		public UnitEntity Worker { get; private set; }

		public GhostBuilding()
		{
			if ( IsServer ) Transmit = TransmitType.Never;
		}

		public void SetWorkerAndBuilding( UnitEntity worker, BaseBuilding building )
		{
			SetModel( building.Model );

			RenderAlpha = 0.5f;

			GlowActive = true;
			GlowColor = Color.Green;
			GlowState = GlowStates.GlowStateOn;

			Building = building;
			Worker = worker;

			if ( IsClient )
			{
				SceneObject.SetMaterialOverride( BlueprintMaterial );

				if ( IsClient )
				{
					BoundsEntity = new GhostBounds();
					BoundsEntity.RenderBounds = CollisionBounds * 1.25f;
					BoundsEntity.SetParent( this );
					BoundsEntity.Position = Position;
					BoundsEntity.Color = Color.Green;
					BoundsEntity.Alpha = 0.05f;
				}
			}
		}

		public void ShowValid()
		{
			if ( BoundsEntity.IsValid() )
				BoundsEntity.Color = Color.Green;

			RenderColor = Color.White;
			GlowColor = Color.Green;
		}

		public void ShowInvalid()
		{
			if ( BoundsEntity.IsValid() )
				BoundsEntity.Color = Color.Red;

			RenderColor = Color.Red;
			GlowColor = Color.Red;
		}

		public TraceResult GetPlacementTrace( Client client, Vector3 cursorOrigin, Vector3 cursorAim )
		{
			if ( IsServer )
				return TraceExtension.RayDirection( cursorOrigin, cursorAim )
					.WorldOnly()
					.Run();
			else
				return TraceExtension.RayDirection( cursorOrigin, cursorAim )
					.WorldOnly()
					.Run();
		}

		public bool IsPlacementValid( TraceResult trace )
		{
			if ( !Worker.IsValid() ) return false;

			var position = trace.EndPos;
			var bounds = CollisionBounds * 1.25f;
			var entities = Physics.GetEntitiesInBox( bounds + position ).Where( i => i != this );

			if ( IsClient && !FogManager.Instance.IsAreaSeen( position ) )
				return false;

			if ( entities.Count() > 0 )
				return false;

			if ( position.Distance( Worker.Position ) > Worker.Item.ConstructRange )
				return false;

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

		protected override void OnDestroy()
		{
			if ( BoundsEntity.IsValid() )
				BoundsEntity.Delete();

			base.OnDestroy();
		}
	}
}
