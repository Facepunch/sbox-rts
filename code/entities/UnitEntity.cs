using Sandbox;
using RTS.Units;
using System.Collections.Generic;
using Gamelib.Nav;

namespace RTS
{
	public partial class UnitEntity : ItemEntity<BaseUnit>
	{
		public override bool CanMultiSelect => true;
		public List<ModelEntity> Clothing => new();
		public UnitCircle Circle { get; private set; }
		public float Speed { get; private set; }
		public NavSteer Steer;

		private Vector3 _inputVelocity;

		public override void ClientSpawn()
		{
			Circle = new();
			Circle.SetParent( this );
			Circle.LocalPosition = Vector3.Zero;

			base.ClientSpawn();
		}

		public void MoveTo( Vector3 position )
		{
			Steer ??= new();
			Steer.Target = position;
		}

		public ModelEntity AttachClothing( string modelName )
		{
			var entity = new ModelEntity();

			entity.SetModel( modelName );
			entity.SetParent( this, true );
			entity.EnableShadowInFirstPerson = true;
			entity.EnableHideInFirstPerson = true;

			Clothing.Add( entity );

			return entity;
		}

		public void RemoveClothing()
		{
			Clothing.ForEach( ( entity ) => entity.Delete() );
			Clothing.Clear();
		}

		protected override void OnItemChanged( BaseUnit item )
		{
			if ( !string.IsNullOrEmpty( item.Model ) )
			{
				SetModel( item.Model );
			}

			foreach ( var clothes in item.Clothing )
			{
				AttachClothing( clothes );
			}

			EyePos = Position + Vector3.Up * 64;
			CollisionGroup = CollisionGroup.Player;
			SetupPhysicsFromCapsule( PhysicsMotionType.Keyframed, Capsule.FromHeightAndRadius( 72, 8 ) );

			EnableHitboxes = true;
			Speed = item.Speed;

			base.OnItemChanged( item );
		}

		protected virtual void Move( float timeDelta )
		{
			var bbox = BBox.FromHeightAndRadius( 64, 4 );

			MoveHelper move = new( Position, Velocity );
			move.MaxStandableAngle = 50;
			move.Trace = move.Trace.Ignore( this ).Size( bbox );

			if ( !Velocity.IsNearlyZero( 0.001f ) )
			{
				move.TryUnstuck();
				move.TryMoveWithStep( timeDelta, 30 );
			}

			var tr = move.TraceDirection( Vector3.Down * 10.0f );

			if ( move.IsFloor( tr ) )
			{
				GroundEntity = tr.Entity;

				if ( !tr.StartedSolid )
				{
					move.Position = tr.EndPos;
				}

				move.Velocity -= _inputVelocity;
				move.ApplyFriction( tr.Surface.Friction * 200.0f, timeDelta );
				move.Velocity += _inputVelocity;
			}
			else
			{
				GroundEntity = null;
				move.Velocity += Vector3.Down * 900 * timeDelta;
			}

			Position = move.Position;
			Velocity = move.Velocity;
		}

		[Event.Tick.Server]
		private void ServerTick()
		{
			_inputVelocity = 0;

			if ( Steer != null )
			{
				Steer.Tick( Position );

				if ( !Steer.Output.Finished )
				{
					var control = GroundEntity != null ? 200 : 10;

					_inputVelocity = Steer.Output.Direction.Normal * Speed;
					var vel = Steer.Output.Direction.WithZ( 0 ).Normal * Time.Delta * control;
					Velocity = Velocity.AddClamped( vel, Speed );

					SetAnimLookAt( "lookat_pos", EyePos + Steer.Output.Direction.WithZ( 0 ) * 10 );
					SetAnimLookAt( "aimat_pos", EyePos + Steer.Output.Direction.WithZ( 0 ) * 10 );
					SetAnimFloat( "aimat_weight", 0.5f );
				}
			}
			
			Move( Time.Delta );

			var walkVelocity = Velocity.WithZ( 0 );

			if ( walkVelocity.Length > 1 )
			{
				var turnSpeed = walkVelocity.Length.LerpInverse( 0, 100, true );
				var targetRotation = Rotation.LookAt( walkVelocity.Normal, Vector3.Up );
				Rotation = Rotation.Lerp( Rotation, targetRotation, turnSpeed * Time.Delta * 3 );
			}

			SetAnimBool( "b_grounded", true );
			SetAnimBool( "b_noclip", false );
			SetAnimBool( "b_swim", false );
			SetAnimFloat( "forward", Vector3.Dot( Rotation.Forward, _inputVelocity ) );
			SetAnimFloat( "sideward", Vector3.Dot( Rotation.Right, _inputVelocity ) );
			SetAnimFloat( "wishspeed", Speed );
			SetAnimFloat( "walkspeed_scale", 2.0f / 10.0f );
			SetAnimFloat( "runspeed_scale", 2.0f / 320.0f );
			SetAnimFloat( "duckspeed_scale", 2.0f / 80.0f );
		}

		[Event.Tick.Client]
		private void ClientTick()
		{
			if ( Circle.IsValid() && Player.IsValid() )
			{
				if ( Player.IsLocalPawn && IsSelected )
					Circle.Color = Color.White;
				else
					Circle.Color = Player.TeamColor;
			}
		}
	}
}

