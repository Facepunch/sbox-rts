using Sandbox;

namespace Facepunch.RTS
{
	public partial class UnitModifiers : NetworkComponent
	{
		[Net] public float Speed { get; set; } = 1f;
	}
}
