
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.RTS
{
	public class LobbyReadyCheckbox : Panel
	{
		public Panel Icon { get; private set; }

		public LobbyReadyCheckbox()
		{
			Icon = Add.Panel( "icon" );
		}

		protected override void OnClick( MousePanelEvent e )
		{
			if ( Game.LocalPawn is RTSPlayer player )
			{
				LobbyDialog.SetReadyStatus( !player.IsReady );
			}

			base.OnClick( e );
		}

		public void SetEnabled( bool isEnabled )
		{
			Icon.SetClass( "hidden", !isEnabled );
			SetClass( "enabled", isEnabled );
		}
	}

	[StyleSheet( "/ui/lobby/LobbyReadyButton.scss" )]
	public class LobbyReadyButton : Panel
	{
		public LobbyReadyCheckbox Checkbox { get; private set; }
		public Label Label { get; private set; }

		public LobbyReadyButton()
		{
			Checkbox = AddChild<LobbyReadyCheckbox>( "checkbox" );
			Label = Add.Label( "Ready", "label" );
		}

		public override void Tick()
		{
			if ( Game.LocalPawn is RTSPlayer player )
			{
				var isReady = player.IsReady;

				Label.Text = isReady ? "Ready" : "Not Ready";

				Checkbox.SetEnabled( isReady );
				SetClass( "ready", isReady );
			}

			base.Tick();
		}
	}
}
