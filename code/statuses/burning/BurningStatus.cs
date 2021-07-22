﻿using Sandbox;
using System;

namespace Facepunch.RTS
{
	[Library]
	public class BurningStatus : BaseStatus<BurningData>
	{
		public override string Name => "Burning";
		public override string Description => "Help, I'm on fire!";
		public override Texture Icon => Texture.Load( "textures/rts/statuses/burning.png" );

		private RealTimeUntil NextTakeDamage { get; set; }
		private Particles Particles { get; set; }

		public override void OnApplied()
		{
			if ( Host.IsClient )
			{
				Particles = Particles.Create( "particles/weapons/flamethrower/flamethrower_fire.vpcf" );
				Particles.SetPosition( 0, Target.WorldSpaceBounds.Center );
				Particles.SetPosition( 1, new Vector3( 1f, 0f, 0f ) );
			}
		}

		public override void OnRemoved()
		{
			if ( Host.IsClient )
			{
				Particles?.Destroy();
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
					Damage = Data.Damage
				};

				Target.TakeDamage( info );
				NextTakeDamage = Data.Interval;
			}
			else if ( Host.IsClient )
			{
				Particles.SetPosition( 0, Target.WorldSpaceBounds.Center );
			}
		}
	}
}
