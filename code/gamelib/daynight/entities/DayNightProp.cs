using Sandbox;

namespace Gamelib.DayNight
{
	[Library( "daynight_prop" )]
	[Hammer.EntityTool( "Day Night Prop", "Day Night System" )]
	[Hammer.Model( Model = "", MaterialGroup = "default" )]
	public class DayNightProp : ModelEntity
	{
		[Property( Title = "Day Material Group" )]
		public int DayMaterialGroup { get; set; } = 0;
		[Property( Title = "Night Material Group" )]
		public int NightMaterialGroup { get; set; } = 1;

		public override void ClientSpawn()
		{
			base.ClientSpawn();

			DayNightManager.OnSectionChanged += HandleSectionChanged;
		}

		private void HandleSectionChanged( TimeSection section )
		{
			if ( section == TimeSection.Dawn )
			{
				SetMaterialGroup( DayMaterialGroup );
			}
			else if ( section == TimeSection.Dusk )
			{
				SetMaterialGroup( NightMaterialGroup );
			}
		}
	}
}
