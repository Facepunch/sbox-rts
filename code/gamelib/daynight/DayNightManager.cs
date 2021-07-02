using Gamelib.Extensions;
using Facepunch.RTS.Buildings;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gamelib.DayNight
{
	public partial class DayNightManager : Entity
	{
		public static DayNightManager Instance { get; private set; }

		public float TimeOfDay { get; set; } = 0f;
		public float Speed { get; set; } = 1f;

		public DayNightManager()
		{
			Instance = this;
			Transmit = TransmitType.Always;
		}

		[Event.Tick.Client]
		private void ClientTick()
		{
			TimeOfDay += Speed * Time.Delta;

			if ( TimeOfDay >= 24f )
			{
				TimeOfDay = 0f;
			}
		}
	}
}
