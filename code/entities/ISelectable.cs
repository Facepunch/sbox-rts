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
		public bool IsSelected { get; set; }
		public bool CanMultiSelect { get; }
		public void Select();
		public void Deselect();
		public void Highlight();
		public void Unhighlight();
	}
}
