using Sandbox;
using RTS.Units;
using System.Collections.Generic;
using Gamelib.Nav;
using RTS.Buildings;
using System.Linq;

namespace RTS
{
	public partial class UnitEntity : ItemEntity<BaseUnit>, IFogViewer, IFogCullable
	{
		[Net] public Weapon Weapon { get; private set; }
		[Net] public float Range { get; private set; }
		[Net] public int Kills { get; set; }
		public override bool CanMultiSelect => true;
		public List<ModelEntity> Clothing => new();
		public UnitCircle Circle { get; private set; }
		public TimeSince LastAttackTime { get; set; }
		public bool FollowTarget { get; private set; }
		public float Speed { get; private set; }
		public Entity Target { get; private set; }
		public NavSteer Steer;

		private Vector3 _inputVelocity;
		private float _wishSpeed;

		public UnitEntity() : base()
		{
			Tags.Add( "unit", "selectable" );
		}

		public bool IsTargetInRange
		{
			get
			{
				return (Target.IsValid() && Target.Position.Distance( Position ) < Range);
			}
		}

		public bool CanConstruct => Item.CanConstruct;

		public override void ClientSpawn()
		{
			Circle = new();
			Circle.SetParent( this );
			Circle.LocalPosition = Vector3.Zero;

			if ( Player.IsValid() && Player.IsLocalPawn )
				FogManager.Instance.AddViewer( this );
			else
				FogManager.Instance.AddCullable( this );

			base.ClientSpawn();
		}

		public void Attack( Entity target )
		{
			Target = target;
			FollowTarget = true;
		}

		public void MoveTo( Vector3 position )
		{
			Target = null;
			Steer ??= new();
			Steer.Target = position;
			FollowTarget = false;
		}

		public void Construct( BuildingEntity building )
		{
			Target = building;
			Steer ??= new();
			Steer.Target = building.Position;
			FollowTarget = true;
		}

		public void ClearTarget()
		{
			Steer = null;
			Target = null;
			FollowTarget = false;
		}

		public void MakeVisible( bool isVisible )
		{
			EnableDrawing = isVisible;
			Circle.EnableDrawing = isVisible;
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

		public bool IsEnemy( ISelectable other )
		{
			return (other.Player != Player);
		}

		protected override void OnDestroy()
		{
			if ( IsClient )
			{
				FogManager.Instance.RemoveViewer( this );
				FogManager.Instance.RemoveCullable( this );
			}

			base.OnDestroy();
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

			Speed = item.Speed;
			Range = item.Range;
			Health = item.MaxHealth;
			EyePos = Position + Vector3.Up * 64;
			CollisionGroup = CollisionGroup.Player;
			EnableHitboxes = true;

			SetupPhysicsFromCapsule( PhysicsMotionType.Keyframed, Capsule.FromHeightAndRadius( 72, 8 ) );

			if ( !string.IsNullOrEmpty( item.Weapon ) )
			{
				Weapon = Library.Create<Weapon>( item.Weapon );
				Weapon.SetParent( this, true );
				Weapon.Unit = this;
			}

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

		private void FindTargetUnit()
		{
			var entities = Physics.GetEntitiesInSphere( Position, Range );
			
			foreach ( var entity in entities )
			{
				if ( entity is UnitEntity unit && IsEnemy( unit ) )
				{
					FollowTarget = false;
					Target = unit;
					return;
				}
			}

			FollowTarget = false;
			Target = null;
		}

		[Event.Tick.Server]
		private void ServerTick()
		{
			_inputVelocity = 0;

			var isTargetInRange = IsTargetInRange;

			if ( !Target.IsValid() || !isTargetInRange )
			{
				if ( Target.IsValid() && FollowTarget )
				{
					Steer.Target = Target.Position;
				}
				else if ( !IsSelected )
				{
					FindTargetUnit();
				}

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
					Rotation = Rotation.Lerp( Rotation, targetRotation, turnSpeed * Time.Delta * 5f );
				}
			}
			else
			{
				var targetDirection = Target.Position - Position;
				var targetRotation = Rotation.LookAt( targetDirection.Normal, Vector3.Up );

				Rotation = Rotation.Lerp( Rotation, targetRotation, Time.Delta * 15f );

				if ( Rotation.Distance( targetRotation ).AlmostEqual( 0f, 0.1f ) )
				{
					if ( Target is BuildingEntity building && building.Player == Player )
					{
						if ( building.IsUnderConstruction )
						{
							building.Health += (building.Item.MaxHealth / building.Item.BuildTime * Time.Delta);
							building.Health = building.Health.Clamp( 0f, building.Item.MaxHealth );

							if ( building.Health == building.Item.MaxHealth )
								building.FinishConstruction();
							else
								building.UpdateConstruction();
						}
						else
						{
							ClearTarget();
						}
					}
					else if ( Weapon.IsValid() && Weapon.CanAttack() )
					{
						Weapon.Attack( Target );
					}
				}
			}

			if ( Weapon.IsValid() )
				SetAnimInt( "holdtype", Weapon.HoldType );
			else
				SetAnimInt( "holdtype", 0 );

			_wishSpeed = _wishSpeed.LerpTo( _inputVelocity.Length, 10f * Time.Delta );

			SetAnimBool( "b_grounded", true );
			SetAnimBool( "b_noclip", false );
			SetAnimBool( "b_swim", false );
			SetAnimFloat( "forward", Vector3.Dot( Rotation.Forward, _inputVelocity ) );
			SetAnimFloat( "sideward", Vector3.Dot( Rotation.Right, _inputVelocity ) );
			SetAnimFloat( "wishspeed", _wishSpeed );
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

