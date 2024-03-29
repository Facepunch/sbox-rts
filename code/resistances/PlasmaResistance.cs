﻿using Sandbox;

namespace Facepunch.RTS
{
	[Library]
	public class PlasmaResistance : BaseResistance
	{
		public override string Name => "Plasma Resistance";
		public override string UniqueId => "resistance.plasma";
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/resistances/plasma.png" );
		public override string DamageType => "plasma";
	}
}
