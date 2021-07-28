﻿using System.Collections.Generic;

namespace Facepunch.RTS.Buildings
{
    public abstract class BaseBuilding : BaseItem, IOccupiableItem, IResistorItem
	{
		public override Color Color => new Color( 0.8f, 0.8f, 0.8f );
		public virtual ResourceGenerator Generator => null;
		public virtual OccupiableSettings Occupiable => new();
		public virtual bool CanDepositResources => false;
		public virtual Dictionary<string, float> Resistances => new();
		public virtual float MinLineOfSight => 200f;
		public virtual uint PopulationBoost => 0;
		public virtual int AttackPriority => 0;
		public virtual float MaxHealth => 100f;
		public virtual string Model => "";
		public virtual float AttackRadius => 600f;
		public virtual float MaxVerticalRange => 100f;
		public virtual float MinVerticalRange => 0f;
		public virtual string Weapon => "";
	}
}
