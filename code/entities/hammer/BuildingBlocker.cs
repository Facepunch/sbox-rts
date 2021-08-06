using Sandbox;

namespace Facepunch.RTS
{
	[Library( "rts_building_blocker" )]
	[Hammer.AutoApplyMaterial( "materials/rts/hammer/building_blocker.vmat" )]
	[Hammer.Solid]
	public class BuildingBlocker : BaseTrigger
	{
		public override void Spawn()
		{
			base.Spawn();

			Transmit = TransmitType.Always;
		}

		public override void StartTouch( Entity other )
		{
			Log.Info( other + " / 1" );

			base.StartTouch( other );
		}
	}
}
