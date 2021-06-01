using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTS
{
	public partial class StatsRound : BaseRound
	{
		public override string RoundName => "STATS";
		public override int RoundDuration => 10;

		protected override void OnStart()
		{
			
		}

		protected override void OnFinish()
		{
			
		}

		protected override void OnTimeUp()
		{
			Game.Instance.ChangeRound( new PlayRound() );
			base.OnTimeUp();
		}
	}
}
