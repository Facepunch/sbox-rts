using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTS
{
	public interface ISelectable
	{
		public Player Player { get; set; }
		public bool IsSelected { get; }
		public bool CanMultiSelect { get; }
		public void Select();
		public void Deselect();
	}
}
