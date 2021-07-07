using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Units
{
	[Library]
	public class Medic : BaseUnit
	{
		public override string Name => "Medic";
		public override string UniqueId => "unit.medic";
		public override string Description => "A special Terry that can heal nearby units.";
		public override Texture Icon => Texture.Load( "textures/rts/icons/naked.png" );
		public override int BuildTime => 10;
		public override string[] MoveSounds => new string[]
		{
			"brute.alright_move_out",
			"brute.as_you_wish",
			"brute.going_there",
			"brute.got_it",
			"brute.lets_do_this",
			"brute.lets_get_it_done"
		};
		public override string[] SelectSounds => new string[]
		{
			"brute.ready",
			"brute.ready2",
			"brute.tell_me_what_to_do",
			"brute.tell_me_what_to_do2",
			"brute.yes_boss"
		};
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 150
		};
	}
}
