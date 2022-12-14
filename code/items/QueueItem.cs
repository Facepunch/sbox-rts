using System;

namespace Facepunch.RTS
{
    public class QueueItem
	{
		public uint Id { get; set; }
		public BaseItem Item { get; set; }
		public float FinishTime { get; set; }

		public void Start()
		{
			FinishTime = RTSGame.Entity.ServerTime + Item.BuildTime;
		}

		public float GetDuration()
		{
			return Item.BuildTime;
		}

		public float GetTimeLeft()
		{
			return Math.Max( FinishTime - RTSGame.Entity.ServerTime, 0f );
		}
	}
}
