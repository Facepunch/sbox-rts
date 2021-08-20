using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Buildings
{
	[Library]
	public class Watchtower : BaseBuilding
	{
		public override string Name => "Watchtower";
		public override string UniqueId => "building.watchtower";
		public override Texture Icon => Texture.Load( "textures/rts/tempicons/watchtower.png" );
		public override string Description => "Useful for seeing across large distances and can hold one unit.";
		public override int BuildTime => 20;
		public override float MaxHealth => 200f;
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
			[ResourceType.Stone] = 200,
			[ResourceType.Metal] = 50
		};
		public override string Model => "models/buildings/watchtower/watchtower.vmdl";
		public override HashSet<string> Dependencies => new()
		{
			"building.commandcentre",
			"tech.infrastructure"
		};
	}
}
