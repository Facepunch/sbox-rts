using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facepunch.RTS
{
	public interface IFogViewer
	{
		public Vector3 Position { get; set; }
		public float LineOfSight { get; }
	}
}
