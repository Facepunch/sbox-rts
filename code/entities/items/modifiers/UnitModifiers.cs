using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS
{
	public partial class UnitModifiers : NetworkComponent
	{
		public IReadOnlyDictionary<string, float> Resistances { get; set; }
		[Net] public float Speed { get; set; } = 1f;
		[Net] public int Damage { get; set; } = 0;
		[Net, OnChangedCallback] private List<float> ResistanceList { get; set; }
		private Dictionary<string, float> ResistanceTable { get; set; }

		public UnitModifiers()
		{
			ResistanceTable = new();
			ResistanceList = new List<float>();
			Resistances = ResistanceTable;
		}

		public void AddResistance( string id, float amount )
		{
			Host.AssertServer();

			var resistance = RTS.Resistances.Find( id );

			if ( ResistanceTable.ContainsKey( id ) )
				ResistanceTable[id] += amount;
			else
				ResistanceTable[id] = amount;

			var networkId = resistance.NetworkId;

			while ( ResistanceList.Count <= networkId )
			{
				ResistanceList.Add( 0f );
			}

			ResistanceList[(int)networkId] = ResistanceTable[id];
		}

		private void OnResistanceListChanged()
		{
			for ( var i = 0; i < ResistanceList.Count; i++ )
			{
				var resistance = RTS.Resistances.Find<BaseResistance>( (uint)i );
				var uniqueId = resistance.UniqueId;

				if ( ResistanceList[i] != 0f )
					ResistanceTable[uniqueId] = ResistanceList[i];
				else if ( ResistanceTable.ContainsKey( uniqueId ) )
					ResistanceTable.Remove( uniqueId );

			}
		}
	}
}
