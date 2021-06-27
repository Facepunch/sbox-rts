using Sandbox;
using Facepunch.RTS.Units;
using System.Collections.Generic;
using Gamelib.Nav;
using Facepunch.RTS.Buildings;
using System.Linq;
using System;
using Gamelib.Extensions;
using Sandbox.UI;

namespace Facepunch.RTS
{
	public partial class Ragdoll : ModelEntity
	{
		public float FadeOutDuration { get; private set; }
		public float FadeOutTime { get; private set; }

		public Ragdoll FadeOut( float duration )
		{
			FadeOutDuration = duration;
			FadeOutTime = Time.Now + duration;

			return this;
		}

		public static Ragdoll From( ModelEntity entity, Vector3 velocity, DamageFlags damageFlags, Vector3 forcePos, Vector3 force, int bone )
		{
			var ragdoll = new Ragdoll();
			var modelName = entity.GetModelName();

			ragdoll.Position = entity.Position;
			ragdoll.Rotation = entity.Rotation;
			ragdoll.Scale = entity.Scale;
			ragdoll.MoveType = MoveType.Physics;
			ragdoll.UsePhysicsCollision = true;
			ragdoll.EnableAllCollisions = true;
			ragdoll.CollisionGroup = CollisionGroup.Debris;
			ragdoll.SetModel( modelName );
			ragdoll.CopyBonesFrom( entity );
			ragdoll.CopyBodyGroups( entity );
			ragdoll.CopyMaterialGroup( entity );
			ragdoll.TakeDecalsFrom( entity );
			ragdoll.EnableHitboxes = true;
			ragdoll.EnableAllCollisions = true;
			ragdoll.SurroundingBoundsMode = SurroundingBoundsType.Physics;
			ragdoll.RenderColorAndAlpha = entity.RenderColorAndAlpha;

			if ( ragdoll.PhysicsGroup == null )
			{
				throw new Exception( $"Tried to make a ragdoll with {modelName} but it has no physics group!" );
			}

			ragdoll.PhysicsGroup.Velocity = velocity;
			ragdoll.SetInteractsAs( CollisionLayer.Debris );
			ragdoll.SetInteractsWith( CollisionLayer.WORLD_GEOMETRY );
			ragdoll.SetInteractsExclude( CollisionLayer.Player | CollisionLayer.Debris );

			foreach ( var child in entity.Children )
			{
				if ( child is not Clothes clothes )
					continue;

				var clothing = new ModelEntity();

				clothing.SetModel( clothes.GetModelName() );
				clothing.SetParent( ragdoll, true );
				clothing.RenderColorAndAlpha = clothes.RenderColorAndAlpha;
			}

			if ( damageFlags.HasFlag( DamageFlags.Bullet ) ||
				 damageFlags.HasFlag( DamageFlags.PhysicsImpact ) )
			{
				var body = bone > 0 ? ragdoll.GetBonePhysicsBody( bone ) : null;

				if ( body != null )
					body.ApplyImpulseAt( forcePos, force * body.Mass );
				else
					ragdoll.PhysicsGroup.ApplyImpulse( force );
			}

			if ( damageFlags.HasFlag( DamageFlags.Blast ) )
			{
				if ( ragdoll.PhysicsGroup != null )
				{
					ragdoll.PhysicsGroup.AddVelocity( (entity.Position - (forcePos + Vector3.Down * 100.0f)).Normal * (force.Length * 0.2f) );
					var angularDir = (Rotation.FromYaw( 90 ) * force.WithZ( 0 ).Normal).Normal;
					ragdoll.PhysicsGroup.AddAngularVelocity( angularDir * (force.Length * 0.02f) );
				}
			}

			return ragdoll;
		}

		[Event.Tick]
		public void ClientTick()
		{
			if ( FadeOutDuration == 0f ) return;

			var fraction = ((FadeOutTime - Time.Now) / FadeOutDuration).Clamp( 0f, 1f );

			if ( fraction <= 0f )
			{
				Delete();
				return;
			}

			RenderAlpha = fraction;

			foreach ( var child in Children )
			{
				if ( child is ModelEntity model )
				{
					model.RenderAlpha = fraction;
				}
			}
		}
	}
}

