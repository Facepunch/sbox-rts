using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.RTS
{
	[Library( "ability_airstrike" )]
	public class AirstrikeAbility : BaseAbility
	{
		public override string Name => "Airstrike";
		public override string Description => "Rain down fire upon your enemies, but be careful not to hit your own units!";
		public override AbilityTargetType TargetType => AbilityTargetType.None;
		public override Texture Icon => Texture.Load( "textures/rts/icons/heal.png" );
		public override float Cooldown => 100f;
		public override float Duration => 5f;
		public override float MaxDistance => 3000f;
		public override float AreaOfEffectRadius => 300f;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Plasma] = 100
		};
		public override HashSet<string> Dependencies => new()
		{
			"tech.pyrotechnics"
		};
		public virtual float BlastRadius => 150f;
		public virtual float MinDamage => 10f;
		public virtual float MaxDamage => 30f;

		private List<Grenade> Rockets { get; set; } = new();
		private Particles Dust { get; set; }

		public override async void OnStarted()
		{
			Cleanup();

			if ( Host.IsServer )
			{
				var targetInfo = TargetInfo;
				var rockets = Rand.Int( 8, 12 );
				var origin = TargetInfo.Origin + Vector3.Up * 3000f;

				for ( var i = 0; i < rockets; i++ )
				{
					var rocket = new Grenade();
					var radius = AreaOfEffectRadius * MathF.Sqrt( Rand.Float( 1f ) );
					var theta = Rand.Float( 1f ) * 2f * MathF.PI;

					var startPosition = origin + new Vector3(
						radius * MathF.Cos( theta ),
						radius * MathF.Sin( theta )
					);
					var endPosition = startPosition.WithZ( TargetInfo.Origin.z );

					rocket.BezierCurve = false;
					rocket.TrailEffect = "particles/weapons/rocket_trail/rocket_trail.vpcf";
					rocket.Initialize( startPosition, endPosition, Rand.Float( 0.5f, 1f ), OnRocketHit );

					await GameTask.Delay( Rand.Int( 0, 10 ) );
				}
			}

			base.OnStarted();
		}

		public override void Tick()
		{
			if ( Dust != null )
			{
				var fraction = Math.Clamp( LastUsedTime / ( Duration / 2f ), 0f, 1f );
				Dust.SetPosition( 1, new Vector3( 255f * (1f - fraction), fraction * AreaOfEffectRadius ) );
			}

			base.Tick();
		}

		public override void OnCancelled()
		{
			Cleanup();

			base.OnCancelled();
		}

		private void OnRocketHit( Grenade rocket, Entity target )
		{
			var entities = Physics.GetEntitiesInSphere( rocket.Position, BlastRadius );

			foreach ( var entity in entities )
			{
				if ( entity is not UnitEntity unit )
					continue;

				var damageInfo = new DamageInfo
				{
					Damage = Rand.Float( MinDamage, MaxDamage ),
					Attacker = (Entity)User,
					Flags = DamageFlags.Blast
				};

				unit.TakeDamage( damageInfo );
			}

			if ( Dust == null )
			{
				Dust = Particles.Create( "particles/vehicle/helicopter_dust/helicopter_dust.vpcf" );
				Dust.SetPosition( 0, TargetInfo.Origin );
			}
		}

		private void Cleanup()
		{
			for ( var i = 0; i < Rockets.Count; i++ )
			{
				Rockets[i].Delete();
			}

			Rockets.Clear();

			if ( Dust != null )
			{
				Dust.Destroy();
				Dust = null;
			}
		}
	}
}
