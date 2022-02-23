using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Buildings
{
	[Library]
	public class Pub : BaseBuilding
	{
		public override string Name => "Pub";
		public override string UniqueId => "building.pub";
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "textures/rts/tempicons/pub.png" );
		public override string Description => "Increases the maximum population of your empire.";
		public override uint PopulationBoost => 6;
		public override int BuildTime => 20;
		public override float MaxHealth => 200f;
		public override OccupiableSettings Occupiable => new()
		{
			MaxOccupants = 3,
			DamageScale = 0.5f,
			Enabled = true
		};
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Stone] = 150,
			[ResourceType.Metal] = 100
		};
		public override string Model => "models/buildings/pub/pub.vmdl";
		public override HashSet<string> Dependencies => new()
		{
			"building.commandcentre"
		};
	}
}
