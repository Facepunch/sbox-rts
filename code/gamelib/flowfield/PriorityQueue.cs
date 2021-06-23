using System.Collections.Generic;

namespace Gamelib.FlowField
{
	public class PriorityQueue<T>
	{
		public class Tuple<T1, T2>
		{
			public T1 First;
			public T2 Second;

			internal Tuple( T1 first, T2 second )
			{
				First = first;
				Second = second;
			}
		}

		private List<Tuple<T, float>> Elements = new();

		public int Count
		{
			get { return Elements.Count; }
		}

		public void Clear()
		{
			Elements.Clear();
		}

		public void Enqueue( T item, float priority )
		{
			Elements.Add( new Tuple<T, float>( item, priority ) );
		}

		public T Dequeue()
		{
			var bestIndex = 0;

			for ( int i = 0; i < Elements.Count; i++ )
			{
				if ( Elements[i].Second < Elements[bestIndex].Second )
				{
					bestIndex = i;
				}
			}

			T bestItem = Elements[bestIndex].First;
			Elements.RemoveAt( bestIndex );
			return bestItem;
		}
	}
}
