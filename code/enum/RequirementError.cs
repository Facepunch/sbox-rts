using Sandbox;

namespace Facepunch.RTS
{
	public enum RequirementError
	{
		Success = 0,
		NotEnoughStone = 1,
		NotEnoughBeer = 2,
		NotEnoughMetal = 3,
		NotEnoughPlasma = 4,
		NotEnoughPopulation = 5,
		InvalidTarget = 6,
		InvalidPlayer = 7,
		Dependencies = 8,
		Cooldown = 9,
		Unknown = 10
	}
}
