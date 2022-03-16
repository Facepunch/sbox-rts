using Facepunch.RTS;
using Gamelib.Maths;
using Sandbox;
using System;

namespace Facepunch.RTS
{
	[Library]
	public partial class Projectile : ModelEntity
	{
		private Particles Trail { get; set; }
		public Vector3 EndPosition { get; private set; }
		public Vector3 StartPosition { get; private set; }
		public Vector3 LastPosition { get; private set; }
		public float TravelDuration { get; private set; }
		public RealTimeUntil TimeUntilHit { get; private set; }
		public Entity Target { get; private set; }
		public Action<Projectile, Entity> Callback { get; private set; }
		public string ExplosionEffect { get; set; } = "particles/weapons/explosion_ground_large/explosion_ground_large.vpcf";
		public string TrailEffect { get; set; } = "particles/weapons/grenade_trail/grenade_trail.vpcf";
		public string LaunchSound { get; set; } = null;
		public string Attachment { get; set; } = null;
		public string HitSound { get; set; } = "grenade.explode1";
		public float BezierHeight { get; set; } = 0f;
		public bool FaceDirection { get; set; } = false;
		public bool BezierCurve { get; set; } = true;
		public bool Debug { get; set; } = false;

		private Sound _launchSound;

		public void Initialize( Vector3 start, Vector3 end, float duration, Action<Projectile, Entity> callback = null )
		{
			StartPosition = start;
			EndPosition = end;
			LastPosition = start;
			TravelDuration = duration;
			TimeUntilHit = duration;
			Callback = callback;
			Transmit = TransmitType.Always;

			if ( !string.IsNullOrEmpty( TrailEffect ) )
			{
				Trail = Particles.Create( TrailEffect );

				if ( !string.IsNullOrEmpty( Attachment ) )
					Trail.SetEntityAttachment( 0, this, Attachment );
				else
					Trail.SetEntity( 0, this );
			}

			if ( !string.IsNullOrEmpty( LaunchSound ) )
				_launchSound = PlaySound( LaunchSound );
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
			_launchSound.Stop();
			Trail?.Destroy();
			Trail = null;
		}

		[Event.Tick.Server]
		protected virtual void ServerTick()
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

			if ( FaceDirection )
				Rotation = Rotation.LookAt( (Position - LastPosition).Normal );

			if ( Debug )
				DebugOverlay.Sphere( Position, 32f, Color.Red );

			if ( TimeUntilHit )
			{
				if ( !string.IsNullOrEmpty( ExplosionEffect ) )
				{
					var explosion = Particles.Create( ExplosionEffect );
					explosion.SetPosition( 0, endPos );
				}

				if ( !string.IsNullOrEmpty( HitSound ) )
					Audio.Play( HitSound, endPos );

				Callback?.Invoke( this, Target );
				RemoveEffects();
				Delete();
			}

			LastPosition = Position;
		}
	}
}
