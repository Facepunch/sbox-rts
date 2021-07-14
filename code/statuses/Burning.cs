using Sandbox;
using System;

namespace Facepunch.RTS
{
	[Library( "status_burning" )]
	public class Burning : BaseStatus
	{
		public override string Name => "Burning";
		public override string Description => "Help, I'm on fire!";
		public override Texture Icon => Texture.Load( "textures/rts/resistances/fire.png" );
		public override float Duration => 3f;

		private RealTimeUntil NextTakeDamage { get; set; }
		private Particles Particles { get; set; }

		public override void OnApplied()
		{
			if ( Host.IsClient )
			{
				Particles = Particles.Create( "particles/weapons/flamethrower_fire.vpcf" );
				Particles.SetPosition( 0, Target.WorldSpaceBounds.Center );
				Particles.SetPosition( 1, new Vector3( 1f, 0f, 0f ) );

				Log.Info( "yes" );
			}
		}

		public override void OnRemoved()
		{
			if ( Host.IsClient )
			{
				Particles.Destroy();
				Particles = null;
			}
		}

		public override void Tick()
		{
			if ( Host.IsServer && NextTakeDamage )
			{
				var info = new DamageInfo
				{
					Flags = DamageFlags.Burn,
					Damage = 1f
				};

				Target.TakeDamage( info );
				NextTakeDamage = 0.3f;
			}
			else if ( Host.IsClient )
			{
				Particles.SetPosition( 0, Target.WorldSpaceBounds.Center );
			}
		}
	}
}
