using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.RTS
{
	public partial class DomeShieldEntity : ModelEntity
	{
		private readonly List<UnitEntity> Units = new();
		private RealTimeUntil KillTime;
		private float Radius;
		private float MaxHealth;
		private UnitEntity Unit;
		private Particles Effect;

		public void Setup( UnitEntity unit, float health, float radius, float duration )
		{
			SetupPhysicsFromSphere( PhysicsMotionType.Keyframed, Vector3.Zero, radius );

			Unit = unit;
			Health = health;
			MaxHealth = health;
			KillTime = duration;
			Radius = radius;

			Effect = Particles.Create( "particles/weapons/bubble_guard/bubble_guard.vpcf" );
			Effect.SetPosition( 0, Position );
			Effect.SetPosition( 1, new Vector3( radius, 0f, 0f ) );

			CollisionGroup = CollisionGroup.Trigger;
			EnableSolidCollisions = false;
			EnableTouch = true;

			TryAddUnit( unit );
		}

		protected override void OnDestroy()
		{
			if ( IsServer )
			{
				for ( var i = 0; i < Units.Count; i++ )
				{
					var unit = Units[i];
					if ( !unit.IsValid() ) continue;
					unit.RemoveComponent<ShieldAbsorber>();
				}

				Effect?.Destroy();
				Effect = null;
			}

			base.OnDestroy();
		}

		public override void StartTouch( Entity other )
		{
			if ( IsServer && other is UnitEntity unit )
			{
				TryAddUnit( unit );
			}

			base.StartTouch( other );
		}

		public override void EndTouch( Entity other )
		{
			if ( IsServer && other is UnitEntity unit )
			{
				TryRemoveUnit( unit );
			}

			base.EndTouch( other );
		}

		private void TryRemoveUnit( UnitEntity unit )
		{
			var component = unit.GetComponent<ShieldAbsorber>();

			if ( component?.Shield == this )
			{
				unit.RemoveComponent<ShieldAbsorber>();
				Units.Remove( unit );
			}
		}

		private void TryAddUnit( UnitEntity unit )
		{
			if ( !unit.HasComponent<ShieldAbsorber>() )
			{
				var component = unit.AddComponent<ShieldAbsorber>();
				component.Shield = this;
				Units.Add( unit );
			}
		}

		[Event.Tick.Server]
		private void ServerTick()
		{
			if ( KillTime || !Unit.IsValid() )
			{
				Delete();
				return;
			}

			if ( Effect != null )
			{
				Effect.SetPosition( 0, Position );
				Effect.SetPosition( 2, new Vector3( 100f - ((Health / MaxHealth) * 100f), 0, 0f ) );
			}
		}
	}
}
