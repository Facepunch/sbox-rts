using Sandbox;

namespace Facepunch.RTS
{
	[Library( "unit_drone" )]
	public partial class DroneEntity : UnitEntity
	{
		protected enum SuicideStage
		{
			None,
			Raise,
			Dive,
			Done
		}

		protected SuicideStage CurrentSuicideStage { get; set; }
		protected ISelectable SuicideTarget { get; set; }

		private Particles _miningLaser;
		private Sound _miningSound;

		public void Suicide( ISelectable target )
		{
			if ( CurrentSuicideStage != SuicideStage.None )
				return;

			Audio.PlayAll( "rts.drone.suicide", Position );

			CurrentSuicideStage = SuicideStage.Raise;
			SuicideTarget = target;

			Deselect();
		}

		public override bool CanSelect()
		{
			if ( CurrentSuicideStage != SuicideStage.None )
				return false;

			return base.CanSelect();
		}

		protected virtual void TickSuicide()
		{
			if ( CurrentSuicideStage == SuicideStage.Done )
				return;

			var targetAsEntity = (SuicideTarget as Entity);

			if ( !targetAsEntity.IsValid() )
			{
				CurrentSuicideStage = SuicideStage.Done;
				Kill();
				return;
			}

			if ( CurrentSuicideStage == SuicideStage.Raise )
			{
				var baseHeight = GetVerticalOffset();
				var targetHeight = baseHeight + Item.VerticalOffset;

				Position = Position.LerpTo( Position.WithZ( targetHeight ), Time.Delta * 20f );

				if ( Position.z.AlmostEqual( targetHeight ) )
				{
					CurrentSuicideStage = SuicideStage.Dive;
					Audio.PlayAll( "rocketlauncher.fire", Position );
				}
			}
			else if ( CurrentSuicideStage  == SuicideStage.Dive )
			{
				var distance = Position.Distance( SuicideTarget.Position );

				Position = Position.LerpTo( SuicideTarget.Position, Time.Delta * 20f );

				LookAtPosition( SuicideTarget.Position, null, false );

				if ( distance <= 20f )
				{
					var damage = new DamageInfo()
						.WithFlag( DamageFlags.Blast )
						.WithAttacker( this )
						.WithWeapon( Weapon );

					damage.Damage = Weapon.GetDamage();

					SuicideTarget.TakeDamage( damage );

					CurrentSuicideStage = SuicideStage.Done;

					Audio.PlayAll( "rocket.explode1", Position );

					Kill();
				}
			}
		}

		protected override void ServerTick()
		{
			if ( CurrentSuicideStage != SuicideStage.None )
			{
				TickSuicide();
				return;
			}

			base.ServerTick();

			if ( IsGathering && _gather.Entity.IsValid() )
			{
				if ( _miningLaser == null )
				{
					_miningLaser = Particles.Create( "particles/weapons/mining_lazer/mining_lazer.vpcf" );
					_miningLaser.SetEntityAttachment( 0, this, "muzzle" );
					_miningSound = PlaySound( "rts.drone.mininglaser" );
				}

				_miningLaser.SetPosition( 1, _gather.Entity.WorldSpaceBounds.Center );
			}
			else
			{
				_miningSound.Stop();
				_miningLaser?.Destroy( true );
				_miningLaser = null;
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			if ( IsServer )
			{
				_miningSound.Stop();
				_miningLaser?.Destroy( true );
				_miningLaser = null;
			}
		}
	}
}
