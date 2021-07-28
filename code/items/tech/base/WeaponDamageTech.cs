using Sandbox;
using System.Linq;

namespace Facepunch.RTS.Tech
{
	public class WeaponDamageTech<T> : BaseTech
	{
		public virtual int DamageModifier => 0;

		public WeaponDamageTech()
		{
			Events.UnitTrained += OnUnitTrained;
		}

		public override void OnCreated( Player player, ISelectable target )
		{
			var units = player.GetUnits().Where( v => v.Weapon is T );

			foreach ( var unit in units )
			{
				unit.Modifiers.Damage += DamageModifier;
			}

			base.OnCreated( player, target );
		}

		private void OnUnitTrained( Player player, UnitEntity unit )
		{
			if ( Has( player ) && unit.Weapon is T )
			{
				unit.Modifiers.Damage += DamageModifier;
			} 
		}
	}
}
