using System.Linq;

namespace Facepunch.RTS.Tech
{
	public class SpeedModifierTech<T> : BaseTech
	{
		public virtual float Speed => 0f;
		public virtual string Tag => "unit";

		public SpeedModifierTech()
		{
			Events.UnitTrained += OnUnitTrained;
		}

		public override void OnCreated( Player player, ISelectable target )
		{
			var units = player.GetUnits().Where( v => v.Item is T && v.Tags.Has( Tag ) );

			foreach ( var unit in units )
			{
				unit.Modifiers.Speed += Speed;
			}

			base.OnCreated( player, target );
		}

		private void OnUnitTrained( Player player, UnitEntity unit )
		{
			if ( Has( player ) && unit.Item is T && unit.Tags.Has( Tag ) )
			{
				unit.Modifiers.Speed += Speed;
			}
		}
	}
}
