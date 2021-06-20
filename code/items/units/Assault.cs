using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Units
{
	[Library]
	public class Assault : BaseUnit
	{
		public override string Name => "Assault";
		public override string UniqueId => "unit.assault";
		public override string Description => "A basic Terry armed with only a pistol.";
		public override bool CanEnterBuildings => true;
		public override Texture Icon => Texture.Load( "textures/rts/icons/assault.png" );
		public override int BuildTime => 10;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 150
		};
		public override string Weapon => "weapon_pistol";
	}
}
