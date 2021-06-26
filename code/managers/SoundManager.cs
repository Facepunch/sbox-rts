using Gamelib.Extensions;
using Facepunch.RTS.Buildings;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.RTS
{
	public partial class SoundManager : Entity
	{
		public static SoundManager Instance { get; private set; }

		public SoundManager()
		{
			Instance = this;
			Transmit = TransmitType.Always;
		}

		public void Play( Player player, string sound )
		{
			Play( To.Single( player ), sound );
		}

		public void Play( Player player, string sound, Vector3 position )
		{
			Play( To.Single( player ), sound, position );
		}

		public void PlayAll( string sound )
		{
			Play( To.Everyone, sound );
		}

		public void PlayAll( string sound, Vector3 position )
		{
			Play( To.Everyone, sound, position );
		}

		[ClientRpc]
		public void Play( ItemCreateError error )
		{
			if ( error == ItemCreateError.NotEnoughStone )
				Play( "announcer.not_enough_stone" );

			if ( error == ItemCreateError.NotEnoughMetal )
				Play( "announcer.not_enough_metal" );

			if ( error == ItemCreateError.NotEnoughPlasma )
				Play( "announcer.not_enough_plasma" );

			if ( error == ItemCreateError.NotEnoughBeer )
				Play( "announcer.not_enough_beer" );

			if ( error == ItemCreateError.NotEnoughPopulation )
				Play( "announcer.need_additional_pubs" );
		}

		[ClientRpc]
		public void Play( string sound )
		{
			Sound.FromScreen( sound );
		}

		[ClientRpc]
		public void Play( string sound, Vector3 position )
		{
			Sound.FromWorld( sound, position );
		}
	}
}
