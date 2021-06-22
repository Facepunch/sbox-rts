using System.Collections.Generic;

namespace Facepunch.RTS.Units
{
    public abstract class BaseUnit : BaseItem
	{
		public override Color Color => Color.Cyan;
		public virtual float MaxHealth => 100f;
		public virtual string Model => "models/citizen/citizen.vmdl";
		public virtual HashSet<string> Clothing => new();
		public virtual bool CanConstruct => false;
		public virtual bool CanEnterBuildings => false;
		public virtual HashSet<ResourceType> Gatherables => new();
		public virtual float Speed => 200f;
		public virtual float LineOfSight => 600f;
		public virtual float ConstructRange => 1500f;
		public virtual float AttackRange => 600f;
		public virtual float InteractRange => 80f;
		public virtual List<string> DamageDecals => new()
		{
			"materials/rts/blood/blood1.vmat"
		};
		public virtual List<string> ImpactEffects => new()
		{
			"particles/impact.flesh.vpcf"
		};
		public virtual uint Population => 1;
		public virtual bool UseRenderColor => false;
		public virtual string Weapon => "";
		public virtual HashSet<string> Buildables => new();

		public override void OnQueued( Player player )
		{
			player.AddPopulation( Population );
		}

		public override void OnUnqueued( Player player )
		{
			player.TakePopulation( Population );
		}

		public override ItemCreateError CanCreate( Player player )
		{
			if ( !player.HasPopulationCapacity( Population ) )
				return ItemCreateError.NotEnoughPopulation;

			return base.CanCreate( player );
		}
	}
}
