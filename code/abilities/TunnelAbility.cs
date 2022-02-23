using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS
{
	[Library( "ability_tunnel" )]
	public class TunnelAbility : BaseAbility
	{
		public override string Name => "Connect Tunnel";
		public override string Description => "Connect this Tunnel to another Tunnel.";
		public override AbilityTargetType TargetType => AbilityTargetType.Building;
		public override HashSet<string> TargetWhitelist => new()
		{
			"building.tunnel"
		};
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/icons/heal.png" );
		public override float MaxDistance => 15000f;
		public override float AreaOfEffectRadius => 200f;

		public override bool IsAvailable()
		{
			if ( User is TunnelEntity tunnel )
			{
				return !tunnel.Connection.IsValid();
			}

			return false;
		}

		public override bool CanTarget( ISelectable target )
		{
			if ( target is TunnelEntity tunnel && tunnel != User )
			{
				return !tunnel.Connection.IsValid();
			}

			return false;
		}

		public override void OnFinished()
		{
			if ( Host.IsServer )
			{
				var targetInfo = TargetInfo;

				if ( User is TunnelEntity a && targetInfo.Target is TunnelEntity b )
				{
					a.ConnectTo( b );
				}
			}

			base.OnFinished();
		}
	}
}
