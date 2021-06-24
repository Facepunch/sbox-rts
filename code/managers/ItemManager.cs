using Gamelib.Extensions;
using Facepunch.RTS.Buildings;
using Facepunch.RTS.Units;
using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Facepunch.RTS
{
	public partial class ItemManager : Entity
	{
		public static ItemManager Instance { get; private set; }

		public Dictionary<string, BaseItem> Table { get; private set; }
		public List<BaseItem> List { get; private set; }
		public GhostBuilding Ghost { get; private set; }
		public Dictionary<uint,Texture> Icons { get; private set; }

		public ItemManager()
		{
			Instance = this;
			Transmit = TransmitType.Always;
			Icons = new();

			BuildItemTable();
		}

		[ServerCmd]
		public static void StartBuilding( int workerId, uint itemId, string cursorOrigin, string cursorAim )
		{
			var caller = ConsoleSystem.Caller.Pawn as Player;

			if ( !caller.IsValid() || caller.IsSpectator )
				return;

			var item = Instance.Find<BaseBuilding>( itemId );

			if ( FindByIndex( workerId ) is UnitEntity worker && worker.CanConstruct )
			{
				if ( item.CanCreate( caller ) != ItemCreateError.Success ) return;

				if ( worker.Player == caller && worker.Item.Buildables.Contains( item.UniqueId ) )
				{
					// This is a bit shit isn't it.
					var ghost = new GhostBuilding();
					ghost.SetWorkerAndBuilding( worker, item );

					var trace = ghost.GetPlacementTrace( ConsoleSystem.Caller, cursorOrigin.ToVector3(), cursorAim.ToVector3() );
					var valid = ghost.IsPlacementValid( trace );
					ghost.Delete();

					if ( valid )
					{
						caller.TakeResourcesForItem( item );

						var building = new BuildingEntity();
						building.Position = trace.EndPos;
						building.Assign( caller, item );
						building.StartConstruction();
						worker.Construct( building );
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

			var item = Instance.Find<BaseItem>( itemId );

			if ( FindByIndex( entityId ) is BuildingEntity building )
			{
				if ( building.Player == caller && item.CanCreate( caller ) == ItemCreateError.Success )
				{
					caller.TakeResourcesForItem( item );
					building.QueueItem( item );
					item.OnQueued( caller );
				}
			}
		}

		[ServerCmd]
		public static void Unqueue( int entityId, uint queueId )
		{
			var caller = ConsoleSystem.Caller.Pawn as Player;

			if ( !caller.IsValid() || caller.IsSpectator )
				return;

			if ( FindByIndex( entityId ) is BuildingEntity building )
			{
				if ( building.Player == caller )
				{
					var item = building.UnqueueItem( queueId );

					if ( item != null )
					{
						caller.GiveResourcesForItem( item );
						building.UnqueueItem( queueId );
						item.OnUnqueued( caller );
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

			var target = FindByIndex( id );

			if ( target.IsValid() )
			{
				var hasUnitEntities = false;

				foreach ( var entity in caller.Selection )
				{
					if ( entity is UnitEntity unit )
					{
						hasUnitEntities = true;
						unit.Attack( target );
					}
				}

				if ( hasUnitEntities )
				{
					var soundOptions = new string[]
					{
						"lets_go",
						"lets_take_em_down",
						"ready_to_destroy"
					};

					RTS.Sound.Play( caller, Rand.FromArray( soundOptions ) );
				}
			}
		}

		[ServerCmd]
		public static void Evict( int buildingId, int unitId )
		{
			var caller = ConsoleSystem.Caller.Pawn as Player;

			if ( !caller.IsValid() || caller.IsSpectator )
				return;

			var building = FindByIndex( buildingId ) as BuildingEntity;
			var unit = FindByIndex( unitId ) as UnitEntity;

			if ( building.IsValid() && unit.IsValid() )
			{
				if ( unit.Player != caller || building.Player != caller )
					return;

				if ( unit.IsInsideBuilding )
				{
					building.EvictUnit( unit );
				}
			}
		}

		[ServerCmd]
		public static void Occupy( int id )
		{
			var caller = ConsoleSystem.Caller.Pawn as Player;

			if ( !caller.IsValid() || caller.IsSpectator )
				return;

			var target = FindByIndex( id ) as BuildingEntity;

			if ( target.IsValid() )
			{
				if ( target.Player != caller ) return;
				if ( !target.CanOccupyUnits ) return;

				foreach ( var entity in caller.Selection )
				{
					if ( entity is UnitEntity unit && unit.Item.CanEnterBuildings )
					{
						unit.Occupy( target );
					}
				}
			}
		}

		[ServerCmd]
		public static void Deposit( int id )
		{
			var caller = ConsoleSystem.Caller.Pawn as Player;

			if ( !caller.IsValid() || caller.IsSpectator )
				return;

			var target = FindByIndex( id );

			if ( target.IsValid() && target is BuildingEntity building )
			{
				if ( building.Player != caller ) return;

				foreach ( var entity in caller.Selection )
				{
					if ( entity is UnitEntity unit )
					{
						if ( building.CanDepositResources )
							unit.Deposit( building );
					}
				}
			}
		}

		[ServerCmd]
		public static void Gather( int id )
		{
			var caller = ConsoleSystem.Caller.Pawn as Player;

			if ( !caller.IsValid() || caller.IsSpectator )
				return;

			var target = FindByIndex( id );

			if ( target.IsValid() && target is ResourceEntity resource )
			{
				foreach ( var entity in caller.Selection )
				{
					if ( entity is UnitEntity unit && unit.CanGather( resource.Resource )  )
						unit.Gather( resource );
				}
			}
		}

		[ServerCmd]
		public static void Construct( int id )
		{
			var caller = ConsoleSystem.Caller.Pawn as Player;

			if ( !caller.IsValid() || caller.IsSpectator )
				return;

			var target = FindByIndex( id );

			if ( target.IsValid() && target is BuildingEntity building )
			{
				if ( building.IsUnderConstruction && building.Player == caller )
				{
					var willStartConstruction = false;

					foreach ( var entity in caller.Selection )
					{
						if ( entity is UnitEntity unit && unit.CanConstruct )
						{
							willStartConstruction = true;
							unit.Construct( building );
						}
					}

					if ( willStartConstruction )
					{
						RTS.Sound.Play( caller, "on_my_way" );
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
				var hasUnitEntities = false;
				var position = new Vector3( entries[0], entries[1], entries[2] );

				foreach ( var entity in caller.Selection )
				{
					if ( entity is UnitEntity unit )
					{
						hasUnitEntities = true;
						unit.MoveTo( position );
					}
				}

				if ( hasUnitEntities )
				{
					var soundOptions = new string[]
					{
						"lets_go",
						"on_my_way"
					};

					RTS.Sound.Play( caller, Rand.FromArray( soundOptions ) );
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

			var hasUnitEntities = false;
			var entities = csv.Split( ',', StringSplitOptions.TrimEntries )
				.Select( i => FindByIndex( Convert.ToInt32( i ) ) );

			foreach ( var entity in entities )
			{
				if ( entity is not ISelectable selectable )
					continue;

				if ( caller.Selection.Count > 0 && !selectable.CanMultiSelect )
					continue;

				if ( selectable.Player == caller )
				{
					hasUnitEntities = (selectable is UnitEntity);
					selectable.Select();
				}
			}

			if ( hasUnitEntities )
			{
				var soundOptions = new string[]
				{
					"ready_lower_serious",
					"tell_me_what_to_do"
				};

				RTS.Sound.Play( caller, Rand.FromArray( soundOptions ) );
			}
		}

		public T Find<T>( string id ) where T : BaseItem
		{
			if ( Table.TryGetValue( id, out var item ) )
				return (item as T);

			return null;
		}

		public T Find<T>( uint id ) where T : BaseItem
		{
			if ( id < List.Count )
				return (List[(int)id] as T);

			return null;
		}

		public void CreateGhost( UnitEntity worker, BaseBuilding building )
		{
			if ( Ghost.IsValid() ) Ghost.Delete();

			Ghost = new GhostBuilding();
			Ghost.SetWorkerAndBuilding( worker, building );

			ClientTick();
		}

		[Event.Tick.Client]
		private void ClientTick()
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

			if ( valid && Input.Released( InputButton.Attack1 ) )
			{
				StartBuilding( Ghost.Worker.NetworkIdent, Ghost.Building.NetworkId, cursorOrigin.ToCSV(), cursorAim.ToCSV() );
				Ghost.Delete();
			}
			else if ( Input.Released( InputButton.Attack2 ) )
			{
				Ghost.Delete();
			}
		}
		
		private void BuildItemTable()
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
