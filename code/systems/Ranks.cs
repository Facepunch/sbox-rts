using System.Collections.Generic;
using Facepunch.RTS;
using Sandbox;

namespace Facepunch.RTS
{
	public static partial class Ranks
	{
		public static Dictionary<string, BaseRank> Table { get; private set; }
		public static List<BaseRank> List { get; private set; }

		public static void Initialize()
		{
			BuildTable();
		}

		public static BaseRank Find( int kills )
		{
			for ( var i = List.Count - 1; i >= 0; i-- )
			{
				var rank = List[i];

				if ( kills >= rank.Kills )
					return rank;
			}

			return null;
		}

		private static void BuildTable()
		{
			Table = new();
			List = new();

			var list = new List<BaseRank>();

			foreach ( var type in TypeLibrary.GetTypes<BaseRank>() )
			{
				if ( !type.IsAbstract && !type.ContainsGenericParameters )
				{
					var rank = TypeLibrary.Create<BaseRank>( type );
					list.Add( rank );
				}
			}

			for ( var i = 0; i < list.Count; i++ )
			{
				var rank = list[i];

				Table.Add( rank.UniqueId, rank );
				List.Add( rank );

				Log.Info( $"Adding {rank.UniqueId} to the available ranks (id = {i})" );
			}

			List.Sort();
		}
	}
}
