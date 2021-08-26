using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Buildings
{
	[Library]
	public class Tunnel : BaseBuilding
	{
		public override string Name => "Tunnel";
		public override string Entity => "building_tunnel";
		public override string UniqueId => "building.tunnel";
		public override Texture Icon => Texture.Load( "textures/rts/tempicons/tunnel.png" );
		public override string Description => "Connect two of these together to move units across the map quickly.";
		public override float MaxHealth => 600f;
		public override int BuildTime => 30;
		public override HashSet<string> Abilities => new()
		{
			"ability_tunnel"
		};
		public override OccupiableSettings Occupiable => new()
		{
			MaxOccupants = 3,
			DamageScale = 0.5f,
			Enabled = true
		};
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Stone] = 300,
			[ResourceType.Metal] = 150
		};
		public override string Model => "models/buildings/tunnel/tunnel.vmdl";
		public override HashSet<string> Dependencies => new()
		{
			"building.commandcentre",
			"tech.boring"
		};
		public override Dictionary<string, float> Resistances => new()
		{
			["resistance.bullet"] = 0.2f,
			["resistance.fire"] = -0.5f
		};
	}
}
