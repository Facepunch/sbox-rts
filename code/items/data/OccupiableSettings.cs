using System;
using System.Collections.Generic;

namespace Facepunch.RTS
{
	public class OccupiableSettings
	{
		public HashSet<string> Whitelist = new();
		public string[] AttackAttachments = Array.Empty<string>();
		public float DamageScale = 0f;
		public uint MaxOccupants = 0;
		public bool Enabled = false;
	}
}
