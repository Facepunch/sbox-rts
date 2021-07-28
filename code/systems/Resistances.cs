using System.Collections.Generic;
using Facepunch.RTS;
using Sandbox;

namespace Facepunch.RTS
{
	public static partial class Resistances
	{
		public static Dictionary<string, BaseResistance> Table = new();
		public static List<BaseResistance> List = new();

		public static void Initialize()
		{
			BuildTable();
		}

		public static DamageInfo Apply( DamageInfo info, IReadOnlyDictionary<string, float> resistances )
		{
			foreach ( var kv in resistances )
			{
				if ( IsApplicable( kv.Key, info.Flags ) )
				{
					info.Damage *= (1f - kv.Value);
				}
			}

			return info;
		}

		public static BaseResistance Find( string id )
		{
			if ( Table.TryGetValue( id, out var resistance ) )
				return resistance;

			return null;
		}

		public static T Find<T>( uint id ) where T : BaseResistance
		{
			if ( id < List.Count )
				return (List[(int)id] as T);

			return null;
		}

		public static bool IsApplicable( string id, DamageFlags flags )
		{
			var resistance = Find( id );
			if ( resistance == null ) return false;

			return flags.HasFlag( resistance.Flags );
		}

		private static void BuildTable()
		{
			var list = new List<BaseResistance>();

			foreach ( var type in Library.GetAll<BaseResistance>() )
			{
				var resistance = Library.Create<BaseResistance>( type );
				list.Add( resistance );
			}

			// Sort alphabetically, this should result in the same index for client and server.
			list.Sort( ( a, b ) => a.UniqueId.CompareTo( b.UniqueId ) );

			for ( var i = 0; i < list.Count; i++ )
			{
				var resistance = list[i];

				Table.Add( resistance.UniqueId, resistance );
				List.Add( resistance );

				resistance.NetworkId = (uint)i;

				Log.Info( $"Adding {resistance.UniqueId} to the available resistances (id = {i})" );
			}
		}
	}
}
