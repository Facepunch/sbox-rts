using Sandbox;

namespace Facepunch.RTS
{
	public static partial class Events
	{
		public delegate void OnBuildingConstructed( RTSPlayer player, BuildingEntity building );	
		public delegate void OnUnitTrained( RTSPlayer player, UnitEntity unit );

		public static event OnUnitTrained UnitTrained;
		public static event OnBuildingConstructed BuildingConstructed;

		internal static void InvokeBuildingConstructed( RTSPlayer player, BuildingEntity building )
		{
			BuildingConstructed?.Invoke( player, building );
		}

		internal static void InvokeUnitTrained( RTSPlayer player, UnitEntity unit )
		{
			UnitTrained?.Invoke( player, unit );
		}
	}
}
