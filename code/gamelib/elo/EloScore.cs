using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gamelib.Elo
{
	public enum EloOutcome
	{
		Loss = 0,
		Win = 1
	}

	public partial class EloScore : BaseNetworkable
	{
		[Net] public int Rating { get; set; }
		[Net] public int Delta { get; set; }

		public PlayerRank GetRank()
		{
			if ( Rating < 1149 )
				return PlayerRank.Bronze;
			else if ( Rating < 1499 )
				return PlayerRank.Silver;
			else if ( Rating < 1849 )
				return PlayerRank.Gold;
			else if ( Rating < 2199 )
				return PlayerRank.Platinum;
			else
				return PlayerRank.Diamond;
		}

		public int GetNextLevelRating()
		{
			var roundedUp = System.Math.Max( ((int)MathF.Ceiling( Rating / 100 ) * 100) - 1, 0 );
			return Rating == roundedUp ? Rating + 100 : roundedUp;
		}

		public PlayerRank GetNextRank()
		{
			var rank = GetRank();

			if ( rank == PlayerRank.Bronze )
				return PlayerRank.Silver;
			else if ( rank == PlayerRank.Silver )
				return PlayerRank.Gold;
			else if ( rank == PlayerRank.Gold )
				return PlayerRank.Platinum;
			else
				return PlayerRank.Diamond;
		}

		public int GetLevel()
		{
			return Rating / 100;
		}

		public void Update( EloScore opponent, EloOutcome outcome )
		{
			var eloK = 32;
			var delta = (int)(eloK * ((int)outcome - Elo.GetWinChance( this, opponent )));

			Rating = System.Math.Max( Rating + delta, 0 );
			Delta = delta;

			opponent.Rating = System.Math.Max( opponent.Rating - delta, 0 );
			opponent.Delta = Delta;
		}
	}
}
