namespace Facepunch.RTS
{
	public interface IFogCullable
	{
		public Vector3 Position { get; }
		public bool HasBeenSeen { get; set; }
		public void MakeVisible( bool isVisible, bool wasVisible );
	}
}
