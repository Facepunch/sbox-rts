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

		private Dictionary<string, BaseItem> _itemTable;
		private List<BaseItem> _itemList;

		public ItemManager()
		{
			Instance = this;
			Transmit = TransmitType.Always;
			BuildItemTable();
		}

		[ServerCmd]
		public static void Queue( int entityId, uint itemId )
		{
			var caller = ConsoleSystem.Caller.Pawn as Player;

			if ( !caller.IsValid() || caller.IsSpectator )
				return;

			var item = ItemManager.Instance.Find<BaseItem>( itemId );

			if ( FindByIndex( entityId ) is BuildingEntity building )
			{
				if ( building.Player == caller )
					building.StartBuild( item );
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
					building.StopBuild( queueId );
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
						unit.FollowTarget = true;
						unit.Target = target;
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
			if ( _itemTable.TryGetValue( id, out var item ) )
				return (item as T);

			return null;
		}

		public T Find<T>( uint id ) where T : BaseItem
		{
			if ( id < _itemList.Count )
				return (_itemList[(int)id] as T);

			return null;
		}
		
		private void BuildItemTable()
		{
			_itemTable = new();
			_itemList = new();

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

				_itemTable.Add( item.UniqueId, item );
				_itemList.Add( item );

				item.NetworkId = (uint)i;

				Log.Info( $"Adding {item.UniqueId} to the available items (id = {i})" );
			}
		}
	}
}
