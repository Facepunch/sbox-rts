using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.RTS.Tech
{
	public class ResistanceModifierTech<T> : BaseTech
	{
		public virtual Dictionary<string, float> ResistanceModifiers => new();
		public virtual string Tag => "unit";

		public ResistanceModifierTech()
		{
			Events.UnitTrained += OnUnitTrained;
		}

		public override void OnCreated( Player player, ISelectable target )
		{
			var units = player.GetUnits().Where( v => v.Item is T && v.Tags.Has( Tag ) );

			foreach ( var unit in units )
			{
				foreach ( var kv in ResistanceModifiers )
					unit.Modifiers.AddResistance( kv.Key, kv.Value );
			}

			base.OnCreated( player, target );
		}

		private void OnUnitTrained( Player player, UnitEntity unit )
		{
			if ( Has( player ) && unit.Item is T && unit.Tags.Has( Tag ) )
			{
				foreach ( var kv in ResistanceModifiers )
					unit.Modifiers.AddResistance( kv.Key, kv.Value );
			} 
		}
	}
}
