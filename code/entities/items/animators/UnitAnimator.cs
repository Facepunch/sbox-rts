using System;
using Sandbox;

namespace Facepunch.RTS
{
	public class UnitAnimator
	{
		public float Speed;
		public bool Attacking;
		public int HoldType;

		public virtual void Reset()
		{
			Speed = 0f;
			HoldType = 0;
			Attacking = false;
		}

		public virtual void Apply( AnimatedEntity entity )
		{
			
		}
	}
}
