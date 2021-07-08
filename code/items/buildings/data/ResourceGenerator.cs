using System.Collections.Generic;

namespace Facepunch.RTS.Buildings
{
	public class ResourceGenerator
	{
		public Dictionary<ResourceType, int> Resources { get; set; }
		public bool PerOccupant { get; set; }
		public float Interval { get; set; }
	}
}
