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
		public override float Cooldown => 60f;
		public override float Duration => 10f;
		public override float MaxDistance => 10000f;
		public override float AreaOfEffectRadius => 500f;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Plasma] = 100
		};
		public override HashSet<string> Dependencies => new()
		{
			"tech.pyrotechnics"
		};
		public virtual float BlastRadius => 150f;
		public virtual float MinDamage => 40f;
		public virtual float MaxDamage => 90f;

		private List<Projectile> Rockets { get; set; } = new();
		private Particles Dust { get; set; }

		public override void OnStarted()
		{
			Reset();

			if ( Host.IsServer )
			{
				FireMissiles();
			}

			base.OnStarted();
		}

		public override void Tick()
		{
			if ( Dust != null )
			{
				var fraction = Math.Clamp( LastUsedTime / Duration, 0f, 1f );
				Dust.SetPosition( 1, new Vector3( 255f * (1f - fraction), fraction * AreaOfEffectRadius ) );
			}

			base.Tick();
		}

		private async void FireMissiles()
		{
			var targetInfo = TargetInfo;
			var rockets = Rand.Int( 10, 14 );
			var origin = TargetInfo.Origin + Vector3.Up * 3000f;

			Audio.Play( "missile.jetflyby", targetInfo.Origin );

			await GameTask.DelaySeconds( 4f );

			for ( var i = 0; i < rockets; i++ )
			{
				var rocket = new Projectile();
				var radius = AreaOfEffectRadius * MathF.Sqrt( Rand.Float( 1f ) );
				var theta = Rand.Float( 1f ) * 2f * MathF.PI;

				var startPosition = origin + new Vector3(
					radius * MathF.Cos( theta ),
					radius * MathF.Sin( theta )
				);
				var endPosition = startPosition.WithZ( TargetInfo.Origin.z );

				rocket.FaceDirection = true;
				rocket.BezierCurve = false;
				rocket.TrailEffect = "particles/weapons/missile_trail/missile_trail.vpcf";
				rocket.LaunchSound = "missile.falling";
				rocket.Attachment = "muzzle";
				rocket.HitSound = "missile.explode1";

				rocket.SetModel( "models/weapons/missile/missile.vmdl" );
				rocket.Initialize( startPosition, endPosition, Rand.Float( Duration * 0.1f ), OnRocketHit );

				await GameTask.DelaySeconds( Rand.Float( 0f, (Duration * 0.1f) / rockets ) );
			}
		}

		private void OnRocketHit( Projectile rocket, Entity target )
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
				Dust = Particles.Create("particles/dust_cloud/dust_cloud.vpcf");
				Dust.SetPosition( 0, TargetInfo.Origin );
				Dust.SetPosition( 1, new Vector3(AreaOfEffectRadius * 0.75f, 0.75f, 0.75f) );
				Dust.SetPosition( 2, new Vector3 (75f, 75f, 75f) );
			}
		}

		private void Reset()
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
