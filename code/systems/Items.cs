using Gamelib.Extensions;
using Facepunch.RTS.Buildings;
using Facepunch.RTS.Units;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using Gamelib.FlowFields;
using Facepunch.RTS.Commands;
using Gamelib.FlowFields.Extensions;

namespace Facepunch.RTS
{
	public static partial class Items
	{
		public static Dictionary<string, BaseItem> Table { get; private set; }
		public static List<BaseItem> List { get; private set; }
		public static GhostBuilding Ghost { get; private set; }
		public static Dictionary<uint,Texture> Icons { get; private set; }
		public static Particles Marker { get; private set; }

		public static void Initialize()
		{
			Icons = new();
			BuildTable();
		}

		public static bool IsGhostValid()
		{
			return Ghost.IsValid();
		}

		public static void FocusCameraOn( ISelectable selectable )
		{
			if ( Game.LocalPawn is not RTSPlayer player ) return;

			var cameraPosition = selectable.Position;

			player.LookAt( cameraPosition );
		}

		public static void FocusCameraOn( List<ISelectable> selectables )
		{
			if ( Game.LocalPawn is not RTSPlayer player ) return;

			var cameraPosition = Vector3.Zero;
			var itemCount = selectables.Count;

			for ( var i = 0; i < itemCount; i++ )
			{
				var item = selectables[i];
				cameraPosition += item.Position;
			}

			cameraPosition /= itemCount;

			player.LookAt( cameraPosition );
		}

		[ClientRpc]
		public static void ShowMarker( Vector3 position )
		{
            Marker?.Destroy();
            Marker = Particles.Create( "particles/movement_marker/movement_marker.vpcf" );
            Marker.SetPosition( 0, position + Vector3.Up * 5f );

			if ( Game.LocalPawn is RTSPlayer player )
			{
				Marker.SetPosition( 1, player.TeamColor * 255f );
			}
        }

		public static BuildingEntity Create( RTSPlayer player, BaseBuilding item )
		{
			BuildingEntity building;

			if ( string.IsNullOrEmpty( item.Entity ) )
				building = new BuildingEntity();
			else
				building = TypeLibrary.Create<BuildingEntity>( item.Entity );

			building.Assign( player, item );

			return building;
		}

		public static UnitEntity Create( RTSPlayer player, BaseUnit item )
		{
			UnitEntity unit;

			if ( string.IsNullOrEmpty( item.Entity ) )
				unit = new UnitEntity();
			else
				unit = TypeLibrary.Create<UnitEntity>( item.Entity );

			unit.Assign( player, item );

			return unit;
		}

		public static T Create<T>( RTSPlayer player, string itemId ) where T : ISelectable, new()
		{
			T output;

			var item = Find<BaseItem>( itemId );

			if ( string.IsNullOrEmpty( item.Entity ) )
				output = new T();
			else
				output = TypeLibrary.Create<T>( item.Entity );

			output.Assign( player, itemId );

			return output;
		}

		[ConCmd.Server]
		public static void StartBuilding( int workerId, uint itemId, string cursorOrigin, string cursorAim, bool shouldQueue = false )
		{
			var caller = ConsoleSystem.Caller.Pawn as RTSPlayer;

			if ( !caller.IsValid() || caller.IsSpectator )
				return;

			var item = Find<BaseBuilding>( itemId );

			if ( Entity.FindByIndex( workerId ) is UnitEntity worker && worker.CanConstruct )
			{
				if ( item.CanCreate( caller, worker ) != RequirementError.Success ) return;

				if ( worker.Player == caller && worker.Item.Queueables.Contains( item.UniqueId ) )
				{
					// This is a bit shit isn't it.
					var ghost = new GhostBuilding();
					ghost.SetWorkerAndBuilding( worker, item );

					var trace = ghost.GetPlacementTrace( ConsoleSystem.Caller, cursorOrigin.ToVector3(), cursorAim.ToVector3() );
					var valid = ghost.IsPlacementValid( trace );
					ghost.Delete();

					if ( valid )
					{
						ResourceHint.Send( caller, 2f, trace.EndPosition, item.Costs, Color.Red );

						caller.TakeResources( item );

						var building = Create( caller, item );

						building.Position = trace.EndPosition;
						building.Item.PlayPlaceSound( caller );

						if ( !caller.SkipAllWaiting )
						{
							var command = new ConstructCommand
							{
								Target = building
							};

							StartOrQueue( new List<UnitEntity> { worker }, command, shouldQueue );
						}
						else
						{
							building.FinishConstruction();
						}

						worker.Item.PlayConstructSound( caller );

						ShowMarker( To.Single( caller ), building.Position );
					}
				}
			}
		}

		[ConCmd.Server]
		public static void Queue( int entityId, uint itemId )
		{
			var caller = ConsoleSystem.Caller.Pawn as RTSPlayer;

			if ( !caller.IsValid() || caller.IsSpectator )
				return;

			var item = Find<BaseItem>( itemId );

			if ( Entity.FindByIndex( entityId ) is ISelectable selectable )
			{
				if ( selectable.Player == caller && item.CanCreate( caller, selectable ) == RequirementError.Success )
				{
					ResourceHint.Send( caller, 2f, selectable.Position, item.Costs, Color.Red );
					caller.TakeResources( item );
					selectable.QueueItem( item );
					item.OnQueued( caller, selectable );
				}
			}
		}

		[ConCmd.Server]
		public static void Unqueue( int entityId, uint queueId )
		{
			var caller = ConsoleSystem.Caller.Pawn as RTSPlayer;

			if ( !caller.IsValid() || caller.IsSpectator )
				return;

			if ( Entity.FindByIndex( entityId ) is ISelectable selectable )
			{
				if ( selectable.Player == caller )
				{
					var item = selectable.UnqueueItem( queueId );

					if ( item != null )
					{
						ResourceHint.Send( caller, 5f, selectable.Position, item.Costs, Color.Green );
						caller.GiveResources( item );
						item.OnUnqueued( caller, selectable );
					}
				}
			}
		}

		[ConCmd.Server]
		public static void Attack( int id, bool shouldQueue = false )
		{
			var caller = ConsoleSystem.Caller.Pawn as RTSPlayer;

			if ( !caller.IsValid() || caller.IsSpectator )
				return;

			var target = Entity.FindByIndex( id );

			if ( target.IsValid() && target is IDamageable damageable )
			{
				if ( !damageable.CanBeAttacked() ) return;

				var command = new AttackCommand()
				{
					Target = damageable
				};

				var units = caller.ForEachSelected<UnitEntity>( unit =>
				{
					if ( unit.IsUsingAbility() || !unit.CanAttackTarget( damageable ) )
						return false;

					return true;
				} );

				if ( units.Count > 0 )
				{
					StartOrQueue( units, command, shouldQueue );

					var randomUnit = units[Game.Random.Int( units.Count - 1 )];
					randomUnit.Item.PlayAttackSound( caller );

					ShowMarker( To.Single( caller ), target.Position );
				}
			}
		}

		[ConCmd.Server]
		public static void Evict( int occupiableId, int unitId )
		{
			var caller = ConsoleSystem.Caller.Pawn as RTSPlayer;

			if ( !caller.IsValid() || caller.IsSpectator )
				return;

			var occupiable = Entity.FindByIndex( occupiableId ) as IOccupiableEntity;
			var unit = Entity.FindByIndex( unitId ) as UnitEntity;

			if ( occupiable != null && unit.IsValid() )
			{
				if ( unit.Player != caller || occupiable.Player != caller )
					return;

				if ( unit.Occupiable.IsValid() )
				{
					occupiable.EvictUnit( unit );
				}
			}
		}

		[ConCmd.Server]
		public static void Occupy( int occupiableId, bool shouldQueue = false )
		{
			var caller = ConsoleSystem.Caller.Pawn as RTSPlayer;

			if ( !caller.IsValid() || caller.IsSpectator )
				return;

			if ( Entity.FindByIndex( occupiableId ) is IOccupiableEntity target )
			{
				if ( target.Player != caller ) return;
				if ( !target.CanOccupyUnits ) return;

				var units = caller.ForEachSelected<UnitEntity>( unit =>
				{
					if ( unit.Item.Occupant == null )
						return false;

					if ( unit.IsUsingAbility() )
						return false;

					if ( !unit.CanOccupy( target ) )
						return false;

					return true;
				} );

				if ( units.Count > 0 )
				{
					var command = new OccupyCommand
					{
						Target = target
					};

					StartOrQueue( units, command, shouldQueue );

					ShowMarker( To.Single( caller ), target.Position );
				}
			}
		}

		[ConCmd.Server]
		public static void RepairOrDeposit( int id, bool shouldQueue )
		{
			var caller = ConsoleSystem.Caller.Pawn as RTSPlayer;

			if ( !caller.IsValid() || caller.IsSpectator )
				return;

			var target = Entity.FindByIndex( id );

			if ( target.IsValid() && target is BuildingEntity building )
			{
				if ( building.Player != caller ) return;

				var depositUnits = caller.ForEachSelected<UnitEntity>( unit =>
				{
					if ( unit.IsUsingAbility() )
						return false;

					if ( unit.Carrying.Count > 0 && building.CanDepositResources )
					{
						unit.SetDepositTarget( building );
						return true;
					}

					return false;
				} );

				var repairUnits = caller.ForEachSelected<UnitEntity>( unit =>
				{
					if ( unit.IsUsingAbility() )
						return false;

					if ( unit.Carrying.Count == 0 )
					{
						if ( unit.CanConstruct && !building.IsUnderConstruction && building.IsDamaged() )
						{
							unit.SetRepairTarget( building );
							return true;
						}
					}

					return false;
				} );

				if ( depositUnits.Count > 0 )
				{
					var command = new DepositCommand
					{
						Target = building
					};

					StartOrQueue( depositUnits, command, shouldQueue );

					var randomUnit = depositUnits[Game.Random.Int( depositUnits.Count - 1 )];
					randomUnit.Item.PlayDepositSound( caller );
				}

				if ( repairUnits.Count > 0 )
				{
					var command = new RepairCommand
					{
						Target = building
					};

					StartOrQueue( repairUnits, command, shouldQueue );

					var randomUnit = repairUnits[Game.Random.Int( repairUnits.Count - 1 )];
					randomUnit.Item.PlayConstructSound( caller );
				}

				ShowMarker( To.Single( caller ), building.Position );
			}
		}

		[ConCmd.Server]
		public static void Gather( int id, bool shouldQueue = false )
		{
			var caller = ConsoleSystem.Caller.Pawn as RTSPlayer;

			if ( !caller.IsValid() || caller.IsSpectator )
				return;

			var target = Entity.FindByIndex( id );

			if ( target.IsValid() && target is ResourceEntity resource )
			{
				var command = new GatherCommand
				{
					Target = resource
				};

				if ( ShouldPlaceRallyPoint( caller.GetAllSelected(), out var trainer ) )
				{
					trainer.SetRallyCommand( command, resource.Position );
					return;
				}

				var resourceType = resource.Resource;
				var units = caller.ForEachSelected<UnitEntity>( unit =>
				{
					if ( unit.IsUsingAbility() )
						return false;

					if ( unit.CanGather( resourceType ) )
						return true;

					return false;
				} );

				if ( units.Count > 0 )
				{
					StartOrQueue( units, command, shouldQueue );

					var randomUnit = units[Game.Random.Int( units.Count - 1 )];
					randomUnit.Item.PlayGatherSound( caller, resourceType );

					ShowMarker( To.Single( caller ), resource.Position );
				}
			}
		}

		[ConCmd.Server]
		public static void Construct( int id, bool shouldQueue )
		{
			var caller = ConsoleSystem.Caller.Pawn as RTSPlayer;

			if ( !caller.IsValid() || caller.IsSpectator )
				return;

			var target = Entity.FindByIndex( id );

			if ( target.IsValid() && target is BuildingEntity building )
			{
				if ( building.IsUnderConstruction && building.Player == caller )
				{
					var command = new ConstructCommand()
					{
						Target = building
					};

					if ( ShouldPlaceRallyPoint( caller.GetAllSelected(), out var trainer ) )
					{
						trainer.SetRallyCommand( command, building.Position );
						return;
					}

					var units = caller.ForEachSelected<UnitEntity>( unit =>
					{
						if ( unit.IsUsingAbility() )
							return false;

						if ( unit.CanConstruct )
							return true;

						return false;
					} );

					if ( units.Count > 0 )
					{
						StartOrQueue( units, command, shouldQueue );

						var randomUnit = units[Game.Random.Int( units.Count - 1 )];
						randomUnit.Item.PlayConstructSound( caller );

						ShowMarker( To.Single( caller ), building.Position );
					}
				}
			}
		}
		
		[ConCmd.Server]
		public static void MoveToLocation( string csv, bool shouldQueue = false )
		{
			var caller = ConsoleSystem.Caller.Pawn as RTSPlayer;

			if ( !caller.IsValid() || caller.IsSpectator )
				return;

			var entries = csv.Split( ',', StringSplitOptions.TrimEntries )
				.Select( i => Convert.ToSingle( i ) )
				.ToList();

			if ( entries.Count == 3 )
			{
				var position = new Vector3( entries[0], entries[1], entries[2] );
				var command = new MoveCommand()
				{
					Destinations = new List<Vector3>() { position }
				};

				if ( ShouldPlaceRallyPoint( caller.GetAllSelected(), out var trainer ) )
				{
					trainer.SetRallyCommand( command, position );
					return;
				}

				var units = caller.ForEachSelected<UnitEntity>( unit =>
				{
					if ( unit.IsUsingAbility() )
						return false;

					return true;
				} );

				if ( units.Count > 0 )
				{
					StartOrQueue( units, command, shouldQueue );

					// Don't play command sounds too frequently.
					if ( caller.LastCommandSound > 2 )
					{
						var randomUnit = units[Game.Random.Int( units.Count - 1 )];
						randomUnit.Item.PlayMoveSound( caller );
						caller.LastCommandSound = 0;
					}

                    ShowMarker( To.Single( caller ), position );
				}
			}
		}

		public static void StartOrQueue( List<UnitEntity> units, IMoveCommand command, bool shouldQueue = false )
		{
			var existingGroups = new HashSet<MoveGroup>();
			var idleUnits = new List<UnitEntity>();

			for ( int i = 0; i < units.Count; i++ )
			{
				var unit = units[i];

				if ( shouldQueue && unit.IsMoveGroupValid() )
					existingGroups.Add( unit.MoveGroup );
				else
					idleUnits.Add( unit );
			}

			if ( idleUnits.Count > 0 )
			{
				var moveGroup = new MoveGroup();
				var agents = idleUnits.Cast<IMoveAgent>().ToList();

				moveGroup.Initialize( agents, command );

				for ( var i = 0; i < idleUnits.Count; i++ )
				{
					var unit = idleUnits[i];

					unit.ClearMoveStack();
					unit.PushMoveGroup( moveGroup );
				}
			}

			if ( shouldQueue )
			{
				foreach ( var group in existingGroups )
				{
					group.Enqueue( command );
				}
			}
		}

		[ConCmd.Server]
		public static void RefineSelection( string itemId )
		{
			var caller = ConsoleSystem.Caller.Pawn as RTSPlayer;

			if ( !caller.IsValid() || caller.IsSpectator )
				return;

			var itemNetworkId = uint.Parse( itemId );
			var entities = caller.Selection
				.OfType<ISelectable>()
				.Where( v => v.ItemNetworkId == itemNetworkId )
				.ToList();

			caller.ClearSelection();

			foreach ( var entity in entities )
			{
				entity.Select();
			}
		}

		[ConCmd.Server]
		public static void CancelAction( int id )
		{
			var caller = ConsoleSystem.Caller.Pawn as RTSPlayer;

			if ( !caller.IsValid() || caller.IsSpectator )
				return;

			var target = Entity.FindByIndex( id );

			if ( target.IsValid() && target is ISelectable selectable )
			{
				if ( selectable.Player == caller )
				{
					selectable.CancelAction();
				}
			}
		}
		
		[ConCmd.Server]
		public static void Select( string csv = null, bool isAdditive = false )
		{
			var caller = ConsoleSystem.Caller.Pawn as RTSPlayer;

			if ( !caller.IsValid() || caller.IsSpectator )
				return;

			if ( string.IsNullOrEmpty( csv ) )
			{
				caller.ClearSelection();
				return;
			}

			// If this isn't an additive selection, clear our existing one.
			if ( !isAdditive ) caller.ClearSelection();

			var eligible = new List<ISelectable>();
			var entities = csv.Split( ',', StringSplitOptions.TrimEntries )
				.Select( i => Entity.FindByIndex( Convert.ToInt32( i ) ) )
				.OfType<ISelectable>()
				.ToList();

			entities.Sort( ( a, b ) => b.CanMultiSelect.CompareTo( a.CanMultiSelect ) );

			// We can show information about a single enemy unit.
			if ( entities.Count == 1 && entities[0] is UnitEntity target && target.Player != caller )
			{
				caller.Selection.Add( target );
				return;
			}

			foreach ( var entity in entities )
			{
				if ( entity is not ISelectable selectable )
					continue;

				if ( caller.Selection.Count > 0 && !selectable.CanMultiSelect )
					continue;

				if ( selectable.Player == caller )
				{
					selectable.Select();
					eligible.Add( selectable );

					if ( !selectable.CanMultiSelect )
					{
						break;
					}
				}
			}

			if ( eligible.Count > 0 )
			{
				var randomItem = eligible[Game.Random.Int( eligible.Count - 1 )];

				if ( randomItem is UnitEntity unit )
					unit.Item.PlaySelectSound( caller );
				else if ( randomItem is BuildingEntity building )
					building.Item.PlaySelectSound( caller );
			}
		}

		public static T Find<T>( string id ) where T : BaseItem
		{
			if ( Table.TryGetValue( id, out var item ) )
				return (item as T);

			return null;
		}

		public static T Find<T>( uint id ) where T : BaseItem
		{
			var index = id - 1;

			if ( index < List.Count )
				return (List[(int)index] as T);

			return null;
		}

		public static void CreateGhost( UnitEntity worker, BaseBuilding building )
		{
			if ( Ghost.IsValid() ) Ghost.Delete();

			Ghost = new GhostBuilding();
			Ghost.SetWorkerAndBuilding( worker, building );

			ClientTick();
		}

		private static Pathfinder GetLargestPathfinder( List<UnitEntity> units )
		{
			Pathfinder pathfinder = null;

			for ( var i = 0; i < units.Count; i++ )
			{
				var unit = units[i];

				if ( pathfinder == null || unit.Pathfinder.CollisionSize > pathfinder.CollisionSize )
					pathfinder = unit.Pathfinder;
			}

			return pathfinder;
		}

		[Event.Tick.Client]
		private static void ClientTick()
		{
			if ( !Ghost.IsValid() ) return;

			if ( !Ghost.Worker.IsValid() )
			{
				Ghost.Delete();
				return;
			}

			if ( Game.LocalPawn is not RTSPlayer player )
				return;

			var cursorOrigin = player.CursorOrigin;
			var cursorAim = player.CursorDirection;
			var trace = Ghost.GetPlacementTrace( Game.LocalClient, cursorOrigin, cursorAim );
			var valid = Ghost.IsPlacementValid( trace );

			Ghost.Position = trace.EndPosition;

			if ( valid )
				Ghost.ShowValid();
			else
				Ghost.ShowInvalid();

			if ( valid && Input.Down( InputButton.PrimaryAttack ) )
			{
				var isHoldingShift = Input.Down( InputButton.Run );
				StartBuilding( Ghost.Worker.NetworkIdent, Ghost.Building.NetworkId, cursorOrigin.ToCSV(), cursorAim.ToCSV(), isHoldingShift );
				Ghost.Delete();
			}
			else if ( Input.Down( InputButton.SecondaryAttack ) )
			{
				Ghost.Delete();
			}
		}

		private static bool ShouldPlaceRallyPoint( List<ISelectable> selection, out BuildingEntity trainer )
		{
			if ( selection.Count == 1 && selection[0] is BuildingEntity entity && entity.CanSetRallyPoint )
			{
				trainer = entity;
				return true;
			}
			else
			{
				trainer = null;
				return false;
			}
		}

		private static void BuildTable()
		{
			Table = new();
			List = new();

			var list = new List<BaseItem>();

			foreach ( var type in TypeLibrary.GetTypes<BaseItem>() )
			{
				if ( !type.IsAbstract && !type.IsGenericType )
				{
					var item = type.Create<BaseItem>();
					list.Add( item );
				}
			}

			// Sort alphabetically, this should result in the same index for client and server.
			list.Sort( ( a, b ) => a.UniqueId.CompareTo( b.UniqueId ) );

			for ( var i = 0; i < list.Count; i++ )
			{
				var item = list[i];

				Table.Add( item.UniqueId, item );
				List.Add( item );

				item.NetworkId = (uint)(i + 1);

				Log.Info( $"Adding {item.UniqueId} ({item}) to the available items (id = {item.NetworkId})" );
			}
		}
	}
}
