﻿using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Units
{
	[Library]
	public class Cannonfodder : BaseUnit
	{
		public override string Name => "Cannonfodder";
		public override string UniqueId => "unit.cannonfodder";
		public override string Description => "A basic dispensable Terry armed with only a pistol.";
		public override bool CanOccupy => true;
		public override Texture Icon => Texture.Load( "textures/rts/icons/assault.png" );
		public override int BuildTime => 1;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 150
		};
		public override string[] AttackSounds => new string[]
		{
			"brute.alright",
			"brute.move_it",
			"brute.search_and_destroy",
			"brute.take_em_down"
		};
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
		public override string Weapon => "weapon_pistol";
	}
}
