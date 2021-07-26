using Facepunch.RTS;
using Gamelib.Maths;
using Sandbox;
using System;

namespace Facepunch.RTS
{
	[Library]
	partial class Projectile : Entity
	{
		private Particles Trail { get; set; }
		public Vector3 EndPosition { get; private set; }
		public Vector3 StartPosition { get; private set; }
		public float TravelDuration { get; private set; }
		public RealTimeUntil TimeUntilHit { get; private set; }
		public Entity Target { get; private set; }
		public Action<Projectile, Entity> Callback { get; private set; }
		public string ExplosionEffect { get; set; } = "particles/weapons/explosion_ground_large/explosion_ground_large.vpcf";
		public string TrailEffect { get; set; } = "particles/weapons/grenade_trail/grenade_trail.vpcf";
		public float BezierHeight { get; set; } = 0f;
		public bool BezierCurve { get; set; } = true;
		public bool Debug { get; set; } = false;

		public Projectile()
		{
			Transmit = TransmitType.Always;
		}


		public void Initialize( Vector3 start, Vector3 end, float duration, Action<Projectile, Entity> callback = null )
		{
			StartPosition = start;
			EndPosition = end;
			TravelDuration = duration;
			TimeUntilHit = duration;
			Callback = callback;

			if ( !string.IsNullOrEmpty( TrailEffect ) )
			{
				Trail = Particles.Create( TrailEffect );
				Trail.SetEntity( 0, this );
			}
		}

		public void Initialize( Vector3 start, Entity target, float duration, Action<Projectile, Entity> callback = null )
		{
			Initialize( start, target.Position, duration, callback );
			Target = target;
		}

		protected override void OnDestroy()
		{
			RemoveEffects();

			base.OnDestroy();
		}

		private void RemoveEffects()
		{
			Trail?.Destroy();
			Trail = null;
		}

		[Event.Tick.Server]
		private void ServerTick()
		{
			var endPos = Target.IsValid() ? Target.Position : EndPosition;
			var distance = StartPosition.Distance( endPos );
			var fraction = 1f - (TimeUntilHit / TravelDuration);

			if ( BezierCurve )
			{
				var height = BezierHeight > 0f ? BezierHeight : endPos.z + (distance / 2f);
				var middle = (StartPosition + (endPos - StartPosition) / 2f).WithZ( height );
				Position = Bezier.Calculate( StartPosition, middle, endPos, fraction );
			}
			else
			{
				Position = Vector3.Lerp( StartPosition, endPos, fraction );
			}

			if ( Debug )
				DebugOverlay.Sphere( Position, 32f, Color.Red );

			if ( TimeUntilHit )
			{
				if ( !string.IsNullOrEmpty( ExplosionEffect ) )
				{
					var explosion = Particles.Create( ExplosionEffect );
					explosion.SetPosition( 0, endPos );
				}

				Callback?.Invoke( this, Target );
				RemoveEffects();
				Delete();
			}
		}
	}
}
