using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS
{
	[Library( "ability_nuke" )]
	public class NukeAbility : BaseAbility
	{
		public override string Name => "Nuke";
		public override string Description => "Now I am become Death, the destroyer of worlds.";
		public override AbilityTargetType TargetType => AbilityTargetType.None;
		public override Texture Icon => Texture.Load( "textures/rts/icons/heal.png" );
		public override float Cooldown => 1f;
		public override float Duration => 5f;
		public override float MaxDistance => 3000f;
		public override float AreaOfEffectRadius => 800f;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Plasma] = 1000,
			[ResourceType.Metal] = 1000
		};
		public override HashSet<string> Dependencies => new()
		{
			//"tech.armageddon"
		};
		public virtual float MinDamage => 30f;
		public virtual float MaxDamage => 100f;

		private PointLightEntity Light { get; set; }
		private Particles Effect { get; set; }
		private Sound Siren { get; set; }
		private Nuke Missile { get; set; }

		public override void OnStarted()
		{
			Reset();

			if ( Host.IsServer )
			{
				Launch();
			}

			base.OnStarted();
		}

		private async void Launch()
		{
			Missile = new Nuke
			{
				BezierHeight = 1500f,
				Debug = true
			};

			Missile.Initialize( User.Position, TargetInfo.Origin, Duration, OnNukeHit );

			Light = new PointLightEntity();
			Light.SetParent( Missile, false );
			Light.SetLightColor( Color.Red );
			Light.Flicker = true;
			Light.Range = 1500f;
			Light.BrightnessMultiplier = 2f;

			Audio.Play( $"nuke.launch{Rand.Int( 1, 2 )}", User.Position );

			OpenHatch( true );

			await GameTask.DelaySeconds( Duration * 0.2f );

			Siren = Missile.PlaySound( "nuke.siren" );
		}

		private void OpenHatch( bool shouldOpen )
		{
			if ( User is BuildingEntity building )
				building.SetAnimBool( "open", shouldOpen );
		}

		private void OnNukeHit( Projectile projectile, Entity target )
		{
			var targetInfo = TargetInfo;
			var origin = targetInfo.Origin;

			Sound.FromWorld( "nuke.explode", projectile.Position );

			Effect = Particles.Create( "particles/weapons/explosion_nuke/nuke_base.vpcf" );
			Effect.SetPosition( 0, origin );
			Effect.SetPosition( 1, new Vector3( AreaOfEffectRadius * 0.6f, 0f, 0f ) );
			Effect.SetPosition( 2, origin + new Vector3( 0f, 0f, AreaOfEffectRadius ) );

			var entities = Physics.GetEntitiesInSphere( targetInfo.Origin, AreaOfEffectRadius * 0.8f );

			foreach ( var entity in entities )
			{
				if ( entity is ISelectable selectable )
				{
					var distance = (selectable.Position.Distance( targetInfo.Origin ));
					var fraction = 1f - (distance / AreaOfEffectRadius);
					var damage = MinDamage + ((MaxDamage - MinDamage) * fraction);

					var damageInfo = new DamageInfo
					{
						Damage = damage,
						Weapon = (Entity)User,
						Attacker = (Entity)User,
						Position = selectable.Position
					};

					selectable.TakeDamage( damageInfo );
				}
			}

			Fog.AddTimedViewer( To.Everyone, targetInfo.Origin, AreaOfEffectRadius, 10f );

			Reset();
		}

		private void Reset()
		{
			if ( Host.IsServer )
			{
				OpenHatch( false );
			}

			if ( Missile.IsValid() )
			{
				Missile.Delete();
				Missile = null;
			}

			Light?.Delete();
			Light = null;

			Siren.Stop();
		}
	}
}
