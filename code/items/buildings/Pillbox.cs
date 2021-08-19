using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Buildings
{
	[Library]
	public class Pillbox : BaseBuilding
	{
		public override string Name => "Pillbox";
		public override string UniqueId => "building.pillbox";
		public override Texture Icon => Texture.Load( "textures/rts/tempicons/pillbox.png" );
		public override string Description => "Occupy with units to have them fire at enemies from cover.";
		public override int BuildTime => 30;
		public override float MaxHealth => 300f;
		public override float AttackRadius => 0f;
		public override float MinLineOfSight => 500f;
		public override OccupiableSettings Occupiable => new()
		{
			AttackAttachments = new string[] { "window1", "window2", "window3", "window4", "window5", "window6" },
			DamageScale = 0.2f,
			MaxOccupants = 4,
			Enabled = true
		};
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Stone] = 150,
			[ResourceType.Metal] = 100
		};
		public override string Model => "models/buildings/pillbox/pillbox.vmdl";
		public override HashSet<string> Dependencies => new()
		{
			"building.headquarters",
			"tech.armoredplating"
		};
	}
}
