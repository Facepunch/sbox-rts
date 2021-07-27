using Sandbox;

namespace Facepunch.RTS
{
	[Library]
    public class ShieldAbsorber : ItemComponent
	{
		public DomeShieldEntity Shield { get; set; }

		public override DamageInfo TakeDamage( DamageInfo info )
		{
			if ( info.Attacker is ISelectable selectable )
			{
				var component = selectable.GetComponent<ShieldAbsorber>();

				if ( component?.Shield == Shield )
					return base.TakeDamage( info );
			}

			var absorbed = new DamageInfo()
			{
				Damage = info.Damage * 0.8f
			};

			Shield.TakeDamage( absorbed );

			info.Damage *= 0.2f;

			return base.TakeDamage( info );
		}
	}
}
