using Facepunch.RTS;
using Sandbox;
using System;

namespace Facepunch.RTS
{
	[Library( "weapon_medevac" )]
	partial class MedevacBeam : Weapon
	{
		public override float FireRate => 2f;
		public override int BaseDamage => 0;
		public override string SoundName => null;
		public override WeaponTargetType TargetType => WeaponTargetType.Unit;
		public override WeaponTargetTeam TargetTeam => WeaponTargetTeam.Ally;

		private Particles Beam { get; set; }
		private RealTimeUntil KillBeamTime { get; set; }
		private RealTimeUntil NextHealTime { get; set; }
		private Sound SoundLoop { get; set; }

		public override void Spawn()
		{
			base.Spawn();
		}

		public override bool CanTarget( ISelectable selectable )
		{
			return selectable.Health < selectable.MaxHealth;
		}

		public override void Attack()
		{
			if ( Target is UnitEntity target && target.Health == target.MaxHealth )
				return;

			if ( NextHealTime && Target is ISelectable selectable )
			{
				selectable.ApplyStatus<HealingStatus>( new HealingData()
				{
					Interval = 0.1f,
					Duration = 3f,
					Amount = 1f
				} );

				NextHealTime = 3f;
			}

			CreateBeam();

			LastAttack = 0f;
		}

		public override Transform? GetMuzzle()
		{
			return Attacker.GetAttachment( "muzzle", true );
		}

		protected override void OnDestroy()
		{
			RemoveParticles();

			base.OnDestroy();
		}

		[ClientRpc]
		private void CreateBeam()
		{
			var muzzle = GetMuzzle();

			if ( muzzle.HasValue )
			{
				if ( Beam == null )
				{
					Beam = Particles.Create( "particles/weapons/healing_lazer/healing_lazer.vpcf" );
					Beam.SetPosition( 0, muzzle.Value.Position );
					SoundLoop = PlaySound( "flamethrower.loop" );
				}

				Beam.SetPosition( 1, Target.WorldSpaceBounds.Center );

				KillBeamTime = GetFireRate() * 2f;
			}
		}

		private void RemoveParticles()
		{
			SoundLoop.Stop();
			Beam?.Destroy( true );
			Beam = null;
		}

		[Event.Tick.Client]
		private void ClientTick()
		{
			if ( Beam == null ) return;

			if ( !Target.IsValid() )
			{
				RemoveParticles();
				return;
			}

			var muzzle = GetMuzzle();

			if ( muzzle.HasValue )
			{
				Beam.SetPosition( 0, muzzle.Value.Position );
				Beam.SetPosition( 1, Target.WorldSpaceBounds.Center );
			}

			if ( KillBeamTime )
			{
				RemoveParticles();
			}
		}
	}
}
