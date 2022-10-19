using Gamelib.Network;
using Sandbox;

namespace Facepunch.RTS
{
	internal partial class RoundGlobals : Globals
	{
		[Net, Change] public BaseRound Round { get; set; }

		private void OnRoundChanged( BaseRound newRound, BaseRound oldRound )
		{
			oldRound?.Finish();
			newRound?.Start();
		}

		[Event.Tick]
		private void Tick()
		{
			Round?.OnTick();
		}
	}

	public static partial class Rounds
	{
		private static Globals<RoundGlobals> Variables => Globals.Define<RoundGlobals>( "rounds" );
		public static BaseRound Current => Variables.Value?.Round;

		public static void Change( BaseRound round )
		{
			Assert.NotNull( round );

			var entity = Variables.Value;

			if ( entity.IsValid() )
			{
				entity.Round?.Finish();
				entity.Round = round;
				entity.Round?.Start();
			}
		}
	}
}
