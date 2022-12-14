using Sandbox;

namespace Facepunch.RTS
{
	public abstract class BaseTradeAbility : BaseAbility
	{
		public override AbilityTargetType TargetType => AbilityTargetType.Self;
		public virtual ResourceType Resource => ResourceType.Beer;
		public override float Cooldown => 3f;
		public override float Duration => 1f;
		public virtual int Amount => 0;

		public override string GetDescription()
		{
			return $"Buy {Amount} {Resource}";
		}

		public override void OnFinished()
		{
			base.OnFinished();

			if ( Game.IsClient ) return;
			if ( User is not BuildingEntity building ) return;

			ResourceHint.Send( building.Player, 2f, building.Position, Resource, Amount, Color.Green );

			building.Player.GiveResource( Resource, Amount );
		}
	}
}
