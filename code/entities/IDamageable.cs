using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facepunch.RTS
{
	public interface IDamageable
	{
		public void DoImpactEffects( TraceResult trace );
		public void CreateDamageDecals( Vector3 position );
	}
}
