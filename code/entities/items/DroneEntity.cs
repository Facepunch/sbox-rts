using Sandbox;

namespace Facepunch.RTS
{
	[Library( "unit_drone" )]
	public partial class DroneEntity : UnitEntity
	{
		private Particles _miningLaser;
		private Sound _miningSound;

		protected override void ServerTick()
		{
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
