using Sandbox;

namespace Facepunch.RTS
{
	[Library( "ability_demolish" )]
	public class DemolishAbility : BaseAbility
	{
		public override string Name => "Demolish";
		public override string Description => "Demolish this building to free up space.";
		public override AbilityTargetType TargetType => AbilityTargetType.Self;
		public override Texture Icon => Texture.Load( FileSystem.Mounted,  "ui/icons/heal.png" );
		public override float Cooldown => 0f;

		public override void OnFinished()
		{
			base.OnFinished();

			if ( Host.IsClient ) return;
			if ( User is not BuildingEntity building ) return;

			building.Kill();
		}
	}
}
