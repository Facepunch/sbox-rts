using Sandbox;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
		public override float Duration => 2f;
		public override float MaxDistance => 3000f;
		public override float AreaOfEffectRadius => 800f;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Plasma] = 1000,
			[ResourceType.Metal] = 1000
		};
		public override HashSet<string> Dependencies => new()
		{
			//"tech.pyrotechnics"
		};
		public virtual float MinDamage => 30f;
		public virtual float MaxDamage => 100f;

		private Particles Effect { get; set; }

		public override void OnStarted()
		{
			Cleanup();

			base.OnStarted();
		}

		public override void OnFinished()
		{
			Cleanup();

			if ( Host.IsServer )
			{
				var targetInfo = TargetInfo;

				Effect = Particles.Create( "particles/weapons/explosion_nuke/nuke_base.vpcf" );
				Effect.SetPosition( 0, targetInfo.Origin );
				Effect.SetPosition( 1, new Vector3( AreaOfEffectRadius, 0f, 0f ) );

				var entities = Physics.GetEntitiesInSphere( targetInfo.Origin, AreaOfEffectRadius );
				
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
			}

			base.OnFinished();
		}

		private void Cleanup()
		{
			if ( Effect != null )
			{
				Effect.Destroy();
				Effect = null;
			}
		}
	}
}
