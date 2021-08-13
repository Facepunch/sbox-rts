
using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;

namespace Facepunch.RTS
{
	public partial class ResourceHint : Panel
	{
		public static ResourceHint Create( float duration, Vector3 position, Dictionary<ResourceType, int> resources, Color color )
		{
			Host.AssertClient();

			if ( resources.Count == 0 )
				return null;

			var hint = Local.Hud.AddChild<ResourceHint>();
			hint.Initialize( duration, position, resources, color );
			return hint;
		}

		public static void Send( Player player, float duration, Vector3 position, ResourceType resource, int amount, Color color )
		{
			Host.AssertServer();

			var a = new ResourceType[1] { resource };
			var b = new int[1] { amount };

			CreateOnClient( To.Single( player ), duration, position, a, b, color );
		}

		public static void Send( Player player, float duration, Vector3 position, Dictionary<ResourceType, int> resources, Color color )
		{
			Host.AssertServer();

			if ( resources.Count == 0 )
				return;

			var a = new ResourceType[resources.Count];
			var b = new int[resources.Count];

			resources.Keys.CopyTo( a, 0 );
			resources.Values.CopyTo( b, 0 );

			CreateOnClient( To.Single( player ), duration, position, a, b, color );
		}

		// TODO: We can't send dictionaries in an RPC yet so use two arrays.
		[ClientRpc]
		public static void CreateOnClient( float duration, Vector3 position, ResourceType[] resources, int[] amounts, Color color )
		{
			var dictionary = new Dictionary<ResourceType, int>();

			for ( var i = 0; i < resources.Length; i++)
			{
				dictionary.Add( resources[i], amounts[i] );
			}

			Create( duration, position, dictionary, color );
		}

		public float HideTime { get; private set; }
		public float Duration { get; private set; }
		public Vector3 Position { get; private set; }
		public Panel Container { get; private set; }

		public ResourceHint()
		{
			StyleSheet.Load( "/ui/ResourceHint.scss" );

			Container = Add.Panel( "container" );
		}

		public void Initialize( float duration, Vector3 position, Dictionary<ResourceType, int> resources, Color color )
		{
			Position = position;
			Duration = duration;
			HideTime = Time.Now + duration;

			foreach ( var kv in resources )
			{
				AddResource( kv.Key, kv.Value, color );
			}
		}

		public override void Tick()
		{
			var position = Position.ToScreen() * new Vector3( Screen.Width, Screen.Height ) * ScaleFromScreen;
			var timeLeft = (HideTime - Time.Now);
			var fraction = (timeLeft / Duration);

			Style.Opacity = Easing.EaseOut( fraction );
			Style.Left = Length.Pixels( position.x );
			Style.Top = Length.Pixels( position.y - 32 - ((1f - fraction) * 64) );

			Style.Dirty();

			if ( fraction <= 0f )
			{
				Delete( true );
			}

			base.Tick();
		}

		private void AddResource( ResourceType type, int value, Color color )
		{
			var cost = Container.AddChild<ItemResourceValue>();
			cost.Label.Style.FontColor = color;
			cost.Update( type, value );
		}
	}
}
