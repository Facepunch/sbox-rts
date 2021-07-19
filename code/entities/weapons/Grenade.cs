using Facepunch.RTS;
using Gamelib.FlowFields.Maths;
using Sandbox;
using System;

namespace Facepunch.RTS
{
	[Library]
	partial class Grenade : Entity
	{
		private Particles Trail { get; set; }
		public Vector3 EndPosition { get; private set; }
		public Vector3 StartPosition { get; private set; }
		public float TravelDuration { get; private set; }
		public RealTimeUntil TimeUntilHit { get; private set; }
		public Entity Target { get; private set; }
		public string TrailEffect => "particles/weapons/grenade_trail/grenade_trail.vpcf";
		public string ExplosionEffect => "particles/weapons/explosion_ground_large/explosion_ground_large.vpcf";
		public Action<Entity> Callback { get; private set; }

		public Grenade()
		{
			Transmit = TransmitType.Always;
		}

		public void Initialize( Vector3 start, Vector3 end, float duration, Action<Entity> callback )
		{
			StartPosition = start;
			EndPosition = end;
			TravelDuration = duration;
			TimeUntilHit = duration;
			Callback = callback;

			Trail = Particles.Create( TrailEffect );
			Trail.SetEntity( 0, this );
		}

		public void Initialize( Vector3 start, Entity target, float duration, Action<Entity> callback )
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
			var middle = (StartPosition + (endPos - StartPosition) / 2f).WithZ( StartPosition.z + (distance / 2f) );
			var bezier = Bezier.Calculate( StartPosition, middle, endPos, fraction );

			Position = bezier;
			DebugOverlay.Sphere( bezier, 4f, Color.Red );

			if ( TimeUntilHit )
			{
				var explosion = Particles.Create( ExplosionEffect );
				explosion.SetPosition( 0, endPos );
				RemoveEffects();
				Callback( Target );
				Delete();
			}
		}
	}
}
