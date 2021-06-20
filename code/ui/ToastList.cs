
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace Facepunch.RTS
{
	public class ToastItem : Panel
	{
		public Label Text { get; set; }
		public Image Icon { get; set; }

		private float _endTime;

		public ToastItem()
		{
			Icon = Add.Image( "", "icon" );
			Text = Add.Label( "", "text" );
		}

		public void Update( string text, Texture icon = null )
		{
			Icon.Texture = icon;
			Text.Text = text;

			Log.Info( "Setting to " + icon.ToString() + " / " + icon.Width );

			Icon.SetClass( "hidden", icon == null );

			_endTime = Time.Now + 3f;
		}

		public override void Tick()
		{
			if ( !IsDeleting && Time.Now >= _endTime )
				Delete();
		}
	}

	public class ToastList : Panel
	{
		public static ToastList Instance { get; private set; }

		public ToastList()
		{
			StyleSheet.Load( "/ui/ToastList.scss" );
			Instance = this;
		}

		public void AddItem( string text, Texture icon = null )
		{
			var item = AddChild<ToastItem>();
			item.Update( text, icon );
		}
	}
}
