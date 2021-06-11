using RTS.Buildings;
using Sandbox;
using System.Linq;

namespace RTS
{
	public partial class GhostBuilding : ModelEntity
	{
		public static Material BlueprintMaterial => Material.Load( "materials/rts/blueprint.vmat" );
		public BaseBuilding Building { get; private set; }
		public UnitEntity Worker{ get; private set; }

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
				SceneObject.SetMaterialOverride( BlueprintMaterial );
		}

		public void ShowValid()
		{
			RenderColor = Color.White;
			GlowColor = Color.Green;
		}

		public void ShowInvalid()
		{
			RenderColor = Color.Red;
			GlowColor = Color.Red;
		}

		public TraceResult GetPlacementTrace( Client client, Vector3 cursorAim )
		{
			if ( IsServer )
				return Trace.Ray( client.Pawn.EyePos, client.Pawn.EyePos + cursorAim * 2000f )
					.WorldOnly()
					.Run();
			else
				return Trace.Ray( CurrentView.Position, CurrentView.Position + cursorAim * 2000f )
					.WorldOnly()
					.Run();
		}

		public bool IsPlacementValid( TraceResult trace )
		{
			var bbox = CollisionBounds + trace.EndPos;
			var entities = Physics.GetEntitiesInBox( bbox ).Where( i => i != this );

			if ( entities.Count() > 0 )
				return false;

			var verticality = trace.Normal.Dot( Vector3.Up );

			return (verticality >= 0.9f);
		}
	}
}
