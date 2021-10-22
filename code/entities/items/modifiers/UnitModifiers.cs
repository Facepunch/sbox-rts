using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS
{
	public partial class UnitModifiers : BaseNetworkable
	{
		[Net] public float FireRate { get; set; } = 1f;
		[Net] public float Speed { get; set; } = 1f;
		[Net] public int Damage { get; set; } = 0;
	}
}
