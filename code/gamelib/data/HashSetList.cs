using System;
using System.Collections.Generic;

namespace Gamelib.Data
{
	public class HashSetList<T> : HashSet<T>
	{
		public IReadOnlyCollection<T> ReadOnlyList { get; private set; }

		private List<T> _list;

		public HashSetList()
		{
			_list = new();
			ReadOnlyList = _list.AsReadOnly();
		}

		public new bool Remove( T item )
		{
			if ( base.Remove( item ) )
			{
				_list.Remove( item );
				return true;
			}

			return false;
		}

		public new void Clear()
		{
			_list.Clear();
			base.Clear();
		}

		public new bool Add( T item )
		{
			if ( base.Add( item ) )
			{
				_list.Add( item );
				return true;
			}

			return false;
		}

		public void Sort( Comparison<T> comparer )
		{
			_list.Sort( comparer );
		}

		public new int Count
		{
			get { return _list.Count; }
		}

		public T this[int index]
		{
			get { return _list[index]; }
		}

		public new IEnumerator<T> GetEnumerator()
		{
			for ( int i = 0; i < _list.Count; i++ )
			{
				yield return _list[i];
			}
		}
	}
}
