using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Buildings
{
	[Library]
	public class Watchtower : BaseBuilding
	{
		public override string Name => "Watchtower";
		public override string UniqueId => "building.watchtower";
		public override Texture Icon => Texture.Load( "textures/rts/icons/pub.png" );
		public override string Description => "Useful for seeing across large distances and can hold one unit.";
		public override int BuildTime => 10;
		public override float MaxHealth => 250f;
		public override float MinLineOfSight => 1000f;
		public override OccupiableSettings Occupiable => new()
		{
			AttackAttachments = new string[] { "muzzle" },
			DamageScale = 0.5f,
			MaxOccupants = 1,
			Enabled = true
		};
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Stone] = 150,
			[ResourceType.Metal] = 100
		};
		public override string Model => "models/buildings/watchtower/watchtower.vmdl";
		public override HashSet<string> Dependencies => new()
		{
			"building.headquarters",
			"tech.infrastructure"
		};
	}
}
