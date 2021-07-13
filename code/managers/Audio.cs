using Sandbox;

namespace Facepunch.RTS.Managers
{
	public static partial class Audio
	{
		public static void Play( Player player, string sound )
		{
			Play( To.Single( player ), sound );
		}

		public static void Play( Player player, string sound, Vector3 position )
		{
			Play( To.Single( player ), sound, position );
		}

		public static void PlayAll( string sound )
		{
			Play( To.Everyone, sound );
		}

		public static void PlayAll( string sound, Vector3 position )
		{
			Play( To.Everyone, sound, position );
		}

		[ClientRpc]
		public static void Play( RequirementError error )
		{
			if ( error == RequirementError.NotEnoughStone )
				Play( "announcer.not_enough_stone" );

			if ( error == RequirementError.NotEnoughMetal )
				Play( "announcer.not_enough_metal" );

			if ( error == RequirementError.NotEnoughPlasma )
				Play( "announcer.not_enough_plasma" );

			if ( error == RequirementError.NotEnoughBeer )
				Play( "announcer.not_enough_beer" );

			if ( error == RequirementError.NotEnoughPopulation )
				Play( "announcer.need_additional_pubs" );
		}

		[ClientRpc]
		public static void Play( string sound )
		{
			Sound.FromScreen( sound );
		}

		[ClientRpc]
		public static void Play( string sound, Vector3 position )
		{
			Sound.FromWorld( sound, position );
		}
	}
}
