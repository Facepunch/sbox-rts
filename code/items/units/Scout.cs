using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Units
{
	[Library]
	public class Scout : BaseUnit
	{
		public override string Name => "Scout";
		public override string UniqueId => "unit.scout";
		public override string Model => "models/vehicles/humvee/humvee.vmdl";
		public override string Description => "A basic vehicle good for scouting large areas.";
		public override Texture Icon => Texture.Load( "textures/rts/icons/scout.png" );
		public override bool UseRenderColor => true;
		public override float LineOfSight => 1000f;
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
		public override float Speed => 500f;
		public override int BuildTime => 2;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 150,
			[ResourceType.Metal] = 100
		};
	}
}
