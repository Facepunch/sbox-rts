using Gamelib.Extensions;
using Facepunch.RTS.Buildings;
using Facepunch.RTS.Units;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using Gamelib.FlowFields;

namespace Facepunch.RTS
{
	public static partial class Items
	{
		public static Dictionary<string, BaseItem> Table { get; private set; }
		public static List<BaseItem> List { get; private set; }
		public static GhostBuilding Ghost { get; private set; }
		public static Dictionary<uint,Texture> Icons { get; private set; }

		public static void Initialize()
		{
			Icons = new();
			BuildTable();
		}

		public static bool IsGhostValid()
		{
			return Ghost.IsValid();
		}

		public static BuildingEntity Create( Player player, BaseBuilding item )
		{
			BuildingEntity building;

			if ( string.IsNullOrEmpty( item.Entity ) )
				building = new BuildingEntity();
			else
				building = Library.Create<BuildingEntity>( item.Entity );

			building.Assign( player, item );

			return building;
		}

		public static UnitEntity Create( Player player, BaseUnit item )
		{
			UnitEntity unit;

			if ( string.IsNullOrEmpty( item.Entity ) )
				unit = new UnitEntity();
			else
				unit = Library.Create<UnitEntity>( item.Entity );

			unit.Assign( player, item );

			return unit;
		}

		public static T Create<T>( Player player, string itemId ) where T : ISelectable, new()
		{
			T output;

			var item = Find<BaseItem>( itemId );

			if ( string.IsNullOrEmpty( item.Entity ) )
				output = new T();
			else
				output = Library.Create<T>( item.Entity );

			output.Assign( player, itemId );

			return output;
		}

		[ServerCmd]
		public static void StartBuilding( int workerId, uint itemId, string cursorOrigin, string cursorAim )
		{
			var caller = ConsoleSystem.Caller.Pawn as Player;

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
						ResourceHint.Send( caller, 2f, trace.EndPos, item.Costs, Color.Red );

						caller.TakeResources( item );

						var building = Create( caller, item );

						building.Position = trace.EndPos;
						building.StartConstruction();
						building.Item.PlayPlaceSound( caller );

						worker.Construct( building );
						worker.Item.PlayConstructSound( caller );
					}
				}
			}
		}

		[ServerCmd]
		public static void Queue( int entityId, uint itemId )
		{
			var caller = ConsoleSystem.Caller.Pawn as Player;

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

		[ServerCmd]
		public static void Unqueue( int entityId, uint queueId )
		{
			var caller = ConsoleSystem.Caller.Pawn as Player;

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

		[ServerCmd]
		public static void Attack( int id )
		{
			var caller = ConsoleSystem.Caller.Pawn as Player;

			if ( !caller.IsValid() || caller.IsSpectator )
				return;

			var target = Entity.FindByIndex( id );

			if ( target.IsValid() && target is ISelectable selectable )
			{
				var units = caller.ForEachSelected<UnitEntity>( unit =>
				{
					if ( unit.IsUsingAbility() || !unit.InVerticalRange( selectable ) )
						return false;

					unit.Attack( selectable );
					return true;
				} );

				if ( units.Count > 0 )
				{
					var randomUnit = units[Rand.Int( units.Count - 1 )];
					randomUnit.Item.PlayAttackSound( caller );
				}
			}
		}

		[ServerCmd]
		public static void Evict( int occupiableId, int unitId )
		{
			var caller = ConsoleSystem.Caller.Pawn as Player;

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

		[ServerCmd]
		public static void Occupy( int occupiableId )
		{
			var caller = ConsoleSystem.Caller.Pawn as Player;

			if ( !caller.IsValid() || caller.IsSpectator )
				return;

			if ( Entity.FindByIndex( occupiableId ) is IOccupiableEntity target )
			{
				if ( target.Player != caller ) return;
				if ( !target.CanOccupyUnits ) return;

				caller.ForEachSelected<UnitEntity>( unit =>
				{
					if ( unit.Item.Occupant == null )
						return false;

					if ( unit.IsUsingAbility() )
						return false;

					if ( !unit.CanOccupy( target ) )
						return false;

					unit.Occupy( target );
					return true;
				} );
			}
		}

		[ServerCmd]
		public static void Deposit( int id )
		{
			var caller = ConsoleSystem.Caller.Pawn as Player;

			if ( !caller.IsValid() || caller.IsSpectator )
				return;

			var target = Entity.FindByIndex( id );

			if ( target.IsValid() && target is BuildingEntity building )
			{
				if ( building.Player != caller ) return;
				if ( !building.CanDepositResources ) return;

				var units = caller.ForEachSelected<UnitEntity>( unit =>
				{
					if ( unit.Carrying.Count == 0 )
						return false;

					if ( unit.IsUsingAbility() )
						return false;

					unit.Deposit( building );
					return true;
				} );

				if ( units.Count > 0 )
				{
					var randomUnit = units[Rand.Int( units.Count - 1 )];
					randomUnit.Item.PlayDepositSound( caller );
				}
			}
		}

		[ServerCmd]
		public static void Gather( int id )
		{
			var caller = ConsoleSystem.Caller.Pawn as Player;

			if ( !caller.IsValid() || caller.IsSpectator )
				return;

			var target = Entity.FindByIndex( id );

			if ( target.IsValid() && target is ResourceEntity resource )
			{
				var resourceType = resource.Resource;
				var units = caller.ForEachSelected<UnitEntity>( unit =>
				{
					if ( unit.IsUsingAbility() )
						return false;

					if ( unit.CanGather( resourceType ) )
					{
						unit.Gather( resource );
						return true;
					}

					return false;
				} );

				if ( units.Count > 0 )
				{
					var randomUnit = units[Rand.Int( units.Count - 1 )];
					randomUnit.Item.PlayGatherSound( caller, resourceType );
				}
			}
		}

		[ServerCmd]
		public static void Construct( int id )
		{
			var caller = ConsoleSystem.Caller.Pawn as Player;

			if ( !caller.IsValid() || caller.IsSpectator )
				return;

			var target = Entity.FindByIndex( id );

			if ( target.IsValid() && target is BuildingEntity building )
			{
				if ( building.IsUnderConstruction && building.Player == caller )
				{
					var units = caller.ForEachSelected<UnitEntity>( unit =>
					{
						if ( unit.IsUsingAbility() )
							return false;

						if ( unit.CanConstruct )
						{
							unit.Construct( building );
							return true;
						}

						return false;
					} );

					if ( units.Count > 0 )
					{
						var randomUnit = units[Rand.Int( units.Count - 1 )];
						randomUnit.Item.PlayConstructSound( caller );
					}
				}
			}
		}
		
		[ServerCmd]
		public static void MoveToLocation( string csv )
		{
			var caller = ConsoleSystem.Caller.Pawn as Player;

			if ( !caller.IsValid() || caller.IsSpectator )
				return;

			var entries = csv.Split( ',', StringSplitOptions.TrimEntries )
				.Select( i => Convert.ToSingle( i ) )
				.ToList();

			if ( entries.Count == 3 )
			{
				var position = new Vector3( entries[0], entries[1], entries[2] );
				var units = caller.ForEachSelected<UnitEntity>( unit =>
				{
					if ( unit.IsUsingAbility() )
						return false;

					return true;
				} );

				if ( units.Count > 0 )
				{
					var agents = units.Cast<IMoveAgent>().ToList();
					var moveGroup = new MoveGroup();

					moveGroup.Initialize( agents, position );

					for ( int i = 0; i < units.Count; i++ )
					{
						var unit = units[i];
						unit.MoveTo( moveGroup );
					}

					var randomUnit = units[Rand.Int( units.Count - 1 )];
					randomUnit.Item.PlayMoveSound( caller );
				}
			}
		}

		[ServerCmd]
		public static void Select( string csv = null )
		{
			var caller = ConsoleSystem.Caller.Pawn as Player;

			if ( !caller.IsValid() || caller.IsSpectator )
				return;

			caller.ClearSelection();

			if ( string.IsNullOrEmpty( csv ) )
				return;

			var eligible = new List<ISelectable>();
			var entities = csv.Split( ',', StringSplitOptions.TrimEntries )
				.Select( i => Entity.FindByIndex( Convert.ToInt32( i ) ) )
				.OfType<ISelectable>()
				.ToList();

			entities.Sort( ( a, b ) => b.CanMultiSelect.CompareTo( a.CanMultiSelect ) );

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
				var randomItem = eligible[Rand.Int( eligible.Count - 1 )];

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
			if ( id < List.Count )
				return (List[(int)id] as T);

			return null;
		}

		public static void CreateGhost( UnitEntity worker, BaseBuilding building )
		{
			if ( Ghost.IsValid() ) Ghost.Delete();

			Ghost = new GhostBuilding();
			Ghost.SetWorkerAndBuilding( worker, building );

			ClientTick();
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

			var cursorOrigin = Input.Cursor.Origin;
			var cursorAim = Input.Cursor.Direction;
			var trace = Ghost.GetPlacementTrace( Local.Client, cursorOrigin, cursorAim );
			var valid = Ghost.IsPlacementValid( trace );

			Ghost.Position = trace.EndPos;

			if ( valid )
				Ghost.ShowValid();
			else
				Ghost.ShowInvalid();

			if ( valid && Input.Down( InputButton.Attack1 ) )
			{
				StartBuilding( Ghost.Worker.NetworkIdent, Ghost.Building.NetworkId, cursorOrigin.ToCSV(), cursorAim.ToCSV() );
				Ghost.Delete();
			}
			else if ( Input.Down( InputButton.Attack2 ) )
			{
				Ghost.Delete();
			}
		}
		
		private static void BuildTable()
		{
			Table = new();
			List = new();

			var list = new List<BaseItem>();

			foreach ( var type in Library.GetAll<BaseItem>() )
			{
				var item = Library.Create<BaseItem>( type );
				list.Add( item );
			}

			// Sort alphabetically, this should result in the same index for client and server.
			list.Sort( ( a, b ) => a.UniqueId.CompareTo( b.UniqueId ) );

			for ( var i = 0; i < list.Count; i++ )
			{
				var item = list[i];

				Table.Add( item.UniqueId, item );
				List.Add( item );

				item.NetworkId = (uint)i;

				Log.Info( $"Adding {item.UniqueId} to the available items (id = {i})" );
			}
		}
	}
}
