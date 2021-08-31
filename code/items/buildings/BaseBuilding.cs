using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.RTS.Buildings
{
    public abstract class BaseBuilding : BaseItem, IOccupiableItem, IResistorItem
	{
		public override Color Color => new( 0.8f, 0.8f, 0.8f );
		public virtual ResourceGenerator Generator => null;
		public virtual OccupiableSettings Occupiable => new();
		public virtual bool CanDepositResources => false;
		public virtual bool CanSetRallyPoint => true;
		public virtual Dictionary<string, float> Resistances => new();
		public virtual string[] ActsAsProxyFor => Array.Empty<string>();
		public virtual string[] PlaceSounds => new string[]
		{
			"rts.placemetal"
		};
		public virtual string[] SelectSounds => new string[]
		{
			"rts.clickbeep"
		};
		public virtual string[] ConstructionSounds => new string[]
		{
			"rts.construct1",
			"rts.construct2"
		};
		public virtual string[] DestroySounds => new string[]
		{
			"rts.buildingexplode1"
		};
		public virtual string[] BuiltSounds => new string[]
		{
			"rts.constructioncomplete1"
		};
		public virtual float MinLineOfSight => 200f;
		public virtual uint PopulationBoost => 0;
		public virtual int AttackPriority => 0;
		public virtual float MaxHealth => 1000f;
		public virtual string Model => "";
		public virtual float AttackRadius => 600f;
		public virtual float MaxVerticalRange => 100f;
		public virtual float MinVerticalRange => 0f;
		public virtual bool BuildFirstInstantly => false;
		public virtual bool CanDemolish => true;
		public virtual string Weapon => "";

		public void PlayPlaceSound( Player player )
		{
			if ( PlaceSounds.Length > 0 )
				Audio.Play( player, Rand.FromArray( PlaceSounds ) );
		}

		public void PlaySelectSound( Player player )
		{
			if ( SelectSounds.Length > 0 )
				Audio.Play( player, Rand.FromArray( SelectSounds ) );
		}

		public void PlayDestroySound( BuildingEntity building )
		{
			if ( DestroySounds.Length > 0 )
				Audio.Play( Rand.FromArray( DestroySounds ), building.Position );
		}

		public void PlayBuiltSound( BuildingEntity building )
		{
			if ( BuiltSounds.Length > 0 )
				Audio.Play( Rand.FromArray( BuiltSounds ), building.Position );
		}

		public override bool IsAvailable( Player player, ISelectable target )
		{
			return true;
		}

		public bool IsProxyOf( BaseBuilding other, Dictionary<string, bool> checkedProxies )
		{
			if ( other == this ) return true;

			bool isChecked;

			checkedProxies.TryGetValue( UniqueId, out isChecked );

			if ( isChecked ) return false;

			checkedProxies.Add( UniqueId, true );

			for ( var i = 0; i < ActsAsProxyFor.Length; i++ )
			{
				var proxyItem = Items.Find<BaseBuilding>( ActsAsProxyFor[i] );

				if ( proxyItem != null && proxyItem.IsProxyOf( other, checkedProxies ) )
					return true;
			}

			return false;
		}

		public bool IsProxyOf( BaseBuilding other )
		{
			var checkedProxies = new Dictionary<string, bool>( 5 );

			return IsProxyOf( other, checkedProxies );
		}
	}
}
