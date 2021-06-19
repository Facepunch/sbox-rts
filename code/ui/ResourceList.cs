
using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;
using System;

namespace RTS
{
	public class ResourceList : Panel
	{
		public Dictionary<ResourceType, ItemResourceValue> Resources { get; private set; }
		public Dictionary<ResourceType, int> Cache { get; private set; }

		public ResourceList()
		{
			StyleSheet.Load( "/ui/ResourceList.scss" );

			Cache = new();
			Resources = new();

			AddResource( ResourceType.Stone );
			AddResource( ResourceType.Beer );
			AddResource( ResourceType.Metal );
			AddResource( ResourceType.Plasma );
		}

		public override void Tick()
		{
			SetClass( "hidden", true);

			var player = Local.Pawn as Player;
			if ( player == null || player.IsSpectator ) return;

			var game = Game.Instance;
			if ( game == null ) return;

			var round = game.Round;
			if ( round == null ) return;

			if ( round is PlayRound )
				SetClass( "hidden", false );

			UpdateResource( player, ResourceType.Stone );
			UpdateResource( player, ResourceType.Beer );
			UpdateResource( player, ResourceType.Metal );
			UpdateResource( player, ResourceType.Plasma );

			base.Tick();
		}

		private void UpdateResource( Player player, ResourceType type )
		{
			var amount = player.GetResource( type );
			var cached = Cache[type];

			if ( cached == amount ) return;

			Resources[type].LerpTo( amount, 1f );

			Cache[type] = amount;
		}

		private void AddResource( ResourceType type )
		{
			var resource = AddChild<ItemResourceValue>();
			resource.Update( type, 0 );
			Resources.Add( type, resource );

			Cache.Add( type, 0 );
		}
	}
}
