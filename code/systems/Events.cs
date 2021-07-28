using Sandbox;

namespace Facepunch.RTS
{
	public static partial class Events
	{
		public delegate void OnBuildingConstructed( Player player, BuildingEntity building );	
		public delegate void OnUnitTrained( Player player, UnitEntity unit );

		public static event OnUnitTrained UnitTrained;
		public static event OnBuildingConstructed BuildingConstructed;

		internal static void InvokeBuildingConstructed( Player player, BuildingEntity building )
		{
			BuildingConstructed?.Invoke( player, building );
		}

		internal static void InvokeUnitTrained( Player player, UnitEntity unit )
		{
			UnitTrained?.Invoke( player, unit );
		}
	}
}
