namespace Facepunch.RTS
{
	public interface IMapIconEntity
	{
		public Vector3 Position { get; }
		public Color IconColor { get; }
		public bool ShouldShowOnMap();
	}
}
