using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.RTS
{
    public interface IResistorItem
	{
		public Dictionary<string, float> Resistances { get; }
	}
}
