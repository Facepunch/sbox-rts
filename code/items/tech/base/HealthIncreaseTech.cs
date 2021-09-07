using System.Linq;

namespace Facepunch.RTS.Tech
{
	public class HealthIncreaseTech<T> : BaseTech
	{
		public virtual float Health => 0f;
		public virtual string Tag => "unit";

		public HealthIncreaseTech()
		{
			Events.UnitTrained += OnUnitTrained;
		}

		public override void OnCreated( Player player, ISelectable target )
		{
			var units = player.GetUnits().Where( v => v.Item is T && v.Tags.Has( Tag ) );

			foreach ( var unit in units )
			{
				unit.MaxHealth += Health;
			}

			base.OnCreated( player, target );
		}

		private void OnUnitTrained( Player player, UnitEntity unit )
		{
			if ( Has( player ) && unit is T && unit.Tags.Has( Tag ) )
			{
				unit.MaxHealth += Health;
				unit.Health += Health;
			} 
		}
	}
}
