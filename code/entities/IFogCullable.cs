namespace RTS
{
	public interface IFogCullable
	{
		public Vector3 Position { get; }
		public bool HasBeenSeen { get; set; }
		public bool IsLocalPlayers { get; }
		public void MakeVisible( bool isVisible );
	}
}
