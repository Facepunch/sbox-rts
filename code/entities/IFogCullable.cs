namespace RTS
{
	public interface IFogCullable
	{
		public Vector3 Position { get; }
		public bool IsLocalPlayers { get; }
		public void MakeVisible( bool isVisible );
	}
}
