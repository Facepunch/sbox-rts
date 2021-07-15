using Gamelib.Network;
using Sandbox;

namespace Facepunch.RTS
{
	public struct CameraConfig
	{
		public bool Ortho;
		public float PanSpeed;
		public float ZoomScale;
		public float ZNear;
		public float ZFar;
		public float FOV;
		public float Backward;
		public float Left;
		public float Up;
	}

	public struct GameConfig
	{
		public CameraConfig Camera;
	}

	internal partial class ConfigGlobals : Globals
	{
		[Net] public GameConfig Config { get; set; }

		public ConfigGlobals()
		{
			Config = RTS.Config.Default;
		}
	}

	public static partial class Config
	{
		private static Globals<ConfigGlobals> Variables = Globals.Define<ConfigGlobals>( "config" );

		public static GameConfig Default => new()
		{
			Camera = new CameraConfig
			{
				Ortho = false,
				PanSpeed = 5000f,
				ZoomScale = 0.6f,
				FOV = 30f,
				ZNear = 1000f,
				ZFar = 7500f,
				Backward = 2500f,
				Left = 2500f,
				Up = 5000f
			}
		};

		public static GameConfig Current => Variables.Value?.Config ?? Default;
		public static bool IsValid => Variables.Value.IsValid();
	}
}
