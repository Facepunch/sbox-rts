using Sandbox;

namespace Facepunch.RTS
{
	[Library( "unit_drone" )]
	public partial class DroneEntity : UnitEntity
	{
		private Particles _miningLaser;

		protected override void ServerTick()
		{
			base.ServerTick();

			if ( IsGathering && _gather.Entity.IsValid() )
			{
				if ( _miningLaser == null )
				{
					_miningLaser = Particles.Create( "particles/weapons/mining_lazer/mining_lazer.vpcf" );
					_miningLaser.SetEntityAttachment( 0, this, "muzzle" );
				}

				_miningLaser.SetPosition( 1, _gather.Entity.WorldSpaceBounds.Center );
			}
			else
			{
				_miningLaser?.Destroy( true );
				_miningLaser = null;
			}
		}
	}
}
