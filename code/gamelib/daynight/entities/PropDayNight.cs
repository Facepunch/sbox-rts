using Sandbox;
using System.Threading.Tasks;
using System;

namespace Gamelib.DayNight
{
	[Library("prop_daynight")]
	[Hammer.Model(Model = "models/sbox_props/street_lamp/street_lamp_light_unit.vmdl" , MaterialGroup = "default")]
	public class PropDayNight : ModelEntity
	{
		[Property(Title = "Skin For Day")]
		public int SkinDay { get; set; } = 0;
		[Property(Title = "Skin For Night")]
		public int SkinNight { get; set; } = 1;

		public override void ClientSpawn()
		{
			base.ClientSpawn();

			DayNightManager.OnSectionChanged += HandleSectionChanged;
		}
		private void HandleSectionChanged(TimeSection section)
		{
			if (section == TimeSection.Dawn)
			{
				SetMaterialGroup(SkinDay);
			}
			else if (section == TimeSection.Dusk)
			{
				SetMaterialGroup(SkinNight);
			}
		}
	}
}
