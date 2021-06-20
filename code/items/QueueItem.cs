using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.RTS
{
    public class QueueItem
	{
		public uint Id { get; set; }
		public BaseItem Item { get; set; }
		public float FinishTime { get; set; }

		public void Start()
		{
			FinishTime = RTS.Game.ServerTime + Item.BuildTime;
		}

		public float GetDuration()
		{
			return Item.BuildTime;
		}

		public float GetTimeLeft()
		{
			return Math.Max(FinishTime - RTS.Game.ServerTime, 0f);
		}
	}
}
