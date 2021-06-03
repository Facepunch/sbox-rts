using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTS
{
	public interface ISelectableEntity
	{
		public Player Player { get; set; }
		public bool IsSelected { get; set; }
		public void Select();
		public void Deselect();
		public void Highlight();
		public void Unhighlight();
	}
}
