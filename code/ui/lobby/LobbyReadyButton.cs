
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
			if ( Local.Pawn is Player player )
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

	public class LobbyReadyButton : Panel
	{
		public LobbyReadyCheckbox Checkbox { get; private set; }
		public Label Label { get; private set; }

		public LobbyReadyButton()
		{
			StyleSheet.Load( "/ui/lobby/LobbyReadyButton.scss" );

			Checkbox = AddChild<LobbyReadyCheckbox>( "checkbox" );
			Label = Add.Label( "Ready", "label" );
		}

		public override void Tick()
		{
			if ( Local.Pawn is Player player )
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
