﻿
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace Facepunch.RTS
{
	public class MiniMap : Panel
	{
		public Scene Scene;

		public MiniMap()
		{
			StyleSheet.Load( "/ui/MiniMap.scss" );
		}

		public override void Tick()
		{
			SetClass( "hidden", !Hud.IsLocalPlaying() );
		}
	}
}
