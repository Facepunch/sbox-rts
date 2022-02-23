using Facepunch.RTS.Units;
using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Tech
{
	[Library]
	public class ArmoredPlating : ResistanceModifierTech<IVehicleUnit>
	{
		public override string Name => "Armored Plating";
		public override string UniqueId => "tech.armoredplating";
		public override string Description => "Increases the bullet resistance of all vehicle units by 15%.";
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/icons/wheels.png" );
		public override int BuildTime => 60;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 100,
			[ResourceType.Metal] = 50
		};
		public override Dictionary<string, float> ResistanceModifiers => new()
		{
			["resistance.bullet"] = 0.15f
		};
	}
}
