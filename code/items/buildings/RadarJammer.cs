using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Buildings
{
	[Library]
	public class RadarJammer : BaseBuilding
	{
		public override string Name => "Radar Jammer";
		public override string Entity => "building_radar_jammer";
		public override string UniqueId => "building.radarjammer";
		public override string Description => "Base defense structure that automatically blocks radar scans within its range.";
		public override Texture Icon => Texture.Load( "textures/rts/tempicons/radarjammer.png" );
		public override int BuildTime => 30;
		public override float MaxHealth => 200f;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Stone] = 100,
			[ResourceType.Metal] = 200
		};
		public override string Model => "models/buildings/radar_jammer/radar_jammer.vmdl";
		public override HashSet<string> Dependencies => new()
		{
			"building.commandcentre2",
			"tech.infrastructure"
		};
	}
}
