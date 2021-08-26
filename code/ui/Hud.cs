
using Sandbox;
using Sandbox.UI;

namespace Facepunch.RTS
{
	[Library]
	public partial class Hud : HudEntity<RootPanel>
	{
		public static ItemTooltip Tooltip => ItemTooltip.Instance;
		public static ChatBox ChatBox { get; private set; }

		public static void ToastAll( string text, BaseItem item )
		{
			Toast( To.Everyone, text, item.NetworkId );
		}

		public static void Toast( Player player, string text, BaseItem item )
		{
			Toast( To.Single( player ), text, item.NetworkId );
		}

		public static void ToastAll( string text, string icon = "" )
		{
			Toast( To.Everyone, text, icon );
		}

		public static void Toast( Player player, string text, string icon = "" )
		{
			Toast( To.Single( player ), text, icon );
		}

		[ClientRpc]
		public static void Toast( string text, uint itemId )
		{
			var item = Items.Find<BaseItem>( itemId );

			if ( item != null )
			{
				ToastList.Instance.AddItem( text, item.Icon );
			}
		}

		[ClientRpc]
		public static void Toast( string text, string icon = "" )
		{
			ToastList.Instance.AddItem( text, Texture.Load( icon ) );
		}

		public static bool IsLocalPlaying()
		{
			Host.AssertClient();

			if ( Local.Pawn is not Player player )
				return false;

			if ( player.IsSpectator )
				return false;

			if ( Rounds.Current is not PlayRound )
				return false;

			return true;
		}

		public Panel TopBar { get; private set; }

		public Hud()
		{
			if ( !IsClient )
				return;

			RootPanel.StyleSheet.Load( "/ui/Hud.scss" );

			RootPanel.AddChild<CursorController>();
			RootPanel.AddChild<EntityHud>();
			RootPanel.AddChild<RoundInfo>();
			RootPanel.AddChild<MiniMap>();

			TopBar = RootPanel.Add.Panel( "topbar" );
			TopBar.AddChild<ResourceList>();
			TopBar.AddChild<PopulationCount>();

			RootPanel.AddChild<SelectionGroups>();
			RootPanel.AddChild<SelectedItem>();
			RootPanel.AddChild<IdleUtilityUnits>();
			RootPanel.AddChild<ItemTooltip>();
			RootPanel.AddChild<VoiceList>();

			ChatBox = RootPanel.AddChild<ChatBox>();
			ChatBox.StyleSheet.Load( "/ui/Hud.scss" );

			RootPanel.AddChild<ToastList>();
			RootPanel.AddChild<LoadingScreen>();
		}
	}
}
