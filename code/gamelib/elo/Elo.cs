using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gamelib.Elo
{
	public static class Elo
	{
		public static float GetWinChance( EloScore one, EloScore two )
		{
			return 1f / (1f + MathF.Pow( 10f, (two.Rating - one.Rating) / 400f ));
		}
	}
}
