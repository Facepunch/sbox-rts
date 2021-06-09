using RTS.Buildings;
using RTS.Units;
using Sandbox;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace RTS
{
	public partial class ItemManager : Entity
	{
		public static ItemManager Instance { get; private set; }

		public Dictionary<string, BaseItem> Table { get; private set; }
		public List<BaseItem> List { get; private set; }
		public GhostBuilding Ghost { get; private set; }

		public ItemManager()
		{
			Instance = this;
			Transmit = TransmitType.Always;
			BuildItemTable();
		}

		[ServerCmd]
		public static void StartBuilding( int workerId, uint itemId )
		{
			var caller = ConsoleSystem.Caller.Pawn as Player;

			if ( !caller.IsValid() || caller.IsSpectator )
				return;

			var item = Instance.Find<BaseBuilding>( itemId );

			if ( FindByIndex( workerId ) is UnitEntity worker && worker.CanConstruct )
			{
				if ( worker.Player == caller && worker.Item.Buildables.Contains( item.UniqueId ) )
				{
					// This is a bit shit isn't it.
					var ghost = new GhostBuilding();
					ghost.SetWorkerAndBuilding( worker, item );

					var trace = ghost.GetPlacementTrace( ConsoleSystem.Caller );
					var valid = ghost.IsPlacementValid( trace );
					ghost.Delete();

					if ( valid )
					{
						var building = new BuildingEntity();
						building.Assign( caller, item );
						building.StartConstruction();
						building.Position = trace.EndPos;
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
				if ( building.Player == caller )
					building.StartQueueItem( item );
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
					building.StopQueueItem( queueId );
			}
		}

		[ServerCmd]
		public static void Attack( string id )
		{
			var caller = ConsoleSystem.Caller.Pawn as Player;

			if ( !caller.IsValid() || caller.IsSpectator )
				return;

			var targetId = Convert.ToInt32( id );
			var target = FindByIndex( targetId );

			if ( target.IsValid() )
			{
				foreach ( var entity in caller.Selection )
				{
					if ( entity is UnitEntity unit )
					{
						unit.Attack( target );
					}
				}
			}
		}

		[ServerCmd]
		public static void Construct( string id )
		{
			var caller = ConsoleSystem.Caller.Pawn as Player;

			if ( !caller.IsValid() || caller.IsSpectator )
				return;

			var targetId = Convert.ToInt32( id );
			var target = FindByIndex( targetId );

			if ( target.IsValid() && target is BuildingEntity building )
			{
				if ( building.IsUnderConstruction && building.Player == caller )
				{
					foreach ( var entity in caller.Selection )
					{
						if ( entity is UnitEntity unit && unit.CanConstruct )
						{
							unit.Construct( building );
						}
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

				foreach ( var entity in caller.Selection )
				{
					if ( entity is UnitEntity unit )
					{
						unit.MoveTo( position );
					}
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
					selectable.Select();
				}
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

			var trace = Ghost.GetPlacementTrace( Local.Client );
			var valid = Ghost.IsPlacementValid( trace );

			Ghost.Position = trace.EndPos;

			if ( valid )
				Ghost.GlowColor = Color.Green;
			else
				Ghost.GlowColor = Color.Red;

			if ( valid && Local.Client.Input.Down( InputButton.Attack1 ) )
			{
				StartBuilding( Ghost.Worker.NetworkIdent, Ghost.Building.NetworkId );
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
