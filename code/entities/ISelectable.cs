using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTS
{
	public interface ISelectable
	{
		public Player Player { get; }
		public bool IsSelected { get; }
		public bool CanMultiSelect { get; }
		public float Health { get; set; }
		public void Select();
		public void Deselect();
	}
}
