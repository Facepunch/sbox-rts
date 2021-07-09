using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.RTS
{
    public interface IOccupiableItem
	{
		public HashSet<string> AllowedOccupants { get; }
		public float OccupantDamageScale { get; }
		public uint MaxOccupants { get; }
	}
}
