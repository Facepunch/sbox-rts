/* The MIT License (MIT)

Copyright (c) 2013 Daniel "BlueRaja" Pflughoeft

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE. */

using System;
using System.Collections;
using System.Collections.Generic;

namespace Gamelib.Data
{
	public class SimplePriorityQueue<TItem, TPriority> : IPriorityQueue<TItem, TPriority>
	{
		private class SimpleNode : GenericPriorityQueueNode<TPriority>
		{
			public TItem Data { get; private set; }

			public SimpleNode( TItem data )
			{
				Data = data;
			}
		}

		private const int INITIAL_QUEUE_SIZE = 10;
		private readonly GenericPriorityQueue<SimpleNode, TPriority> _queue;
		private readonly Dictionary<TItem, IList<SimpleNode>> _itemToNodesCache;
		private readonly IList<SimpleNode> _nullNodesCache;

		#region Constructors
		public SimplePriorityQueue() : this( Comparer<TPriority>.Default, EqualityComparer<TItem>.Default ) { }
		public SimplePriorityQueue( IComparer<TPriority> priorityComparer ) : this( priorityComparer.Compare, EqualityComparer<TItem>.Default ) { }
		public SimplePriorityQueue( Comparison<TPriority> priorityComparer ) : this( priorityComparer, EqualityComparer<TItem>.Default ) { }
		public SimplePriorityQueue( IEqualityComparer<TItem> itemEquality ) : this( Comparer<TPriority>.Default, itemEquality ) { }
		public SimplePriorityQueue( IComparer<TPriority> priorityComparer, IEqualityComparer<TItem> itemEquality ) : this( priorityComparer.Compare, itemEquality ) { }

		public SimplePriorityQueue( Comparison<TPriority> priorityComparer, IEqualityComparer<TItem> itemEquality )
		{
			_queue = new GenericPriorityQueue<SimpleNode, TPriority>( INITIAL_QUEUE_SIZE, priorityComparer );
			_itemToNodesCache = new Dictionary<TItem, IList<SimpleNode>>( itemEquality );
			_nullNodesCache = new List<SimpleNode>();
		}
		#endregion

		private SimpleNode GetExistingNode( TItem item )
		{
			if ( item == null )
			{
				return _nullNodesCache.Count > 0 ? _nullNodesCache[0] : null;
			}

			IList<SimpleNode> nodes;

			if ( !_itemToNodesCache.TryGetValue( item, out nodes ) )
			{
				return null;
			}

			return nodes[0];
		}

		private void AddToNodeCache( SimpleNode node )
		{
			if ( node.Data == null )
			{
				_nullNodesCache.Add( node );
				return;
			}

			IList<SimpleNode> nodes;

			if ( !_itemToNodesCache.TryGetValue( node.Data, out nodes ) )
			{
				nodes = new List<SimpleNode>();
				_itemToNodesCache[node.Data] = nodes;
			}

			nodes.Add( node );
		}

		private void RemoveFromNodeCache( SimpleNode node )
		{
			if ( node.Data == null )
			{
				_nullNodesCache.Remove( node );
				return;
			}

			IList<SimpleNode> nodes;

			if ( !_itemToNodesCache.TryGetValue( node.Data, out nodes ) )
			{
				return;
			}

			nodes.Remove( node );

			if ( nodes.Count == 0 )
			{
				_itemToNodesCache.Remove( node.Data );
			}
		}

		public int Count
		{
			get
			{
				lock ( _queue )
				{
					return _queue.Count;
				}
			}
		}

		public TItem First
		{
			get
			{
				lock ( _queue )
				{
					if ( _queue.Count <= 0 )
					{
						throw new InvalidOperationException( "Cannot call .First on an empty queue" );
					}

					return _queue.First.Data;
				}
			}
		}

		public void Clear()
		{
			lock ( _queue )
			{
				_queue.Clear();
				_itemToNodesCache.Clear();
				_nullNodesCache.Clear();
			}
		}

		public bool Contains( TItem item )
		{
			lock ( _queue )
			{
				return item == null ? _nullNodesCache.Count > 0 : _itemToNodesCache.ContainsKey( item );
			}
		}

		public TItem Dequeue()
		{
			lock ( _queue )
			{
				if ( _queue.Count <= 0 )
				{
					throw new InvalidOperationException( "Cannot call Dequeue() on an empty queue" );
				}

				SimpleNode node = _queue.Dequeue();
				RemoveFromNodeCache( node );
				return node.Data;
			}
		}

		private SimpleNode EnqueueNoLockOrCache( TItem item, TPriority priority )
		{
			SimpleNode node = new SimpleNode( item );
			if ( _queue.Count == _queue.MaxSize )
			{
				_queue.Resize( _queue.MaxSize * 2 + 1 );
			}
			_queue.Enqueue( node, priority );
			return node;
		}

		public void Enqueue( TItem item, TPriority priority )
		{
			lock ( _queue )
			{
				IList<SimpleNode> nodes;
				if ( item == null )
				{
					nodes = _nullNodesCache;
				}
				else if ( !_itemToNodesCache.TryGetValue( item, out nodes ) )
				{
					nodes = new List<SimpleNode>();
					_itemToNodesCache[item] = nodes;
				}
				SimpleNode node = EnqueueNoLockOrCache( item, priority );
				nodes.Add( node );
			}
		}

		public bool EnqueueWithoutDuplicates( TItem item, TPriority priority )
		{
			lock ( _queue )
			{
				IList<SimpleNode> nodes;

				if ( item == null )
				{
					if ( _nullNodesCache.Count > 0 )
					{
						return false;
					}
					nodes = _nullNodesCache;
				}
				else if ( _itemToNodesCache.ContainsKey( item ) )
				{
					return false;
				}
				else
				{
					nodes = new List<SimpleNode>();
					_itemToNodesCache[item] = nodes;
				}

				SimpleNode node = EnqueueNoLockOrCache( item, priority );
				nodes.Add( node );

				return true;
			}
		}

		public void Remove( TItem item )
		{
			lock ( _queue )
			{
				SimpleNode removeMe;
				IList<SimpleNode> nodes;

				if ( item == null )
				{
					if ( _nullNodesCache.Count == 0 )
					{
						throw new InvalidOperationException( "Cannot call Remove() on a node which is not enqueued: " + item );
					}
					removeMe = _nullNodesCache[0];
					nodes = _nullNodesCache;
				}
				else
				{
					if ( !_itemToNodesCache.TryGetValue( item, out nodes ) )
					{
						throw new InvalidOperationException( "Cannot call Remove() on a node which is not enqueued: " + item );
					}

					removeMe = nodes[0];

					if ( nodes.Count == 1 )
					{
						_itemToNodesCache.Remove( item );
					}
				}

				_queue.Remove( removeMe );
				nodes.Remove( removeMe );
			}
		}

		public void UpdatePriority( TItem item, TPriority priority )
		{
			lock ( _queue )
			{
				SimpleNode updateMe = GetExistingNode( item );

				if ( updateMe == null )
				{
					throw new InvalidOperationException( "Cannot call UpdatePriority() on a node which is not enqueued: " + item );
				}

				_queue.UpdatePriority( updateMe, priority );
			}
		}

		public TPriority GetPriority( TItem item )
		{
			lock ( _queue )
			{
				SimpleNode findMe = GetExistingNode( item );

				if ( findMe == null )
				{
					throw new InvalidOperationException( "Cannot call GetPriority() on a node which is not enqueued: " + item );
				}

				return findMe.Priority;
			}
		}

		#region Try* methods for multithreading
		public bool TryFirst( out TItem first )
		{
			if ( _queue.Count > 0 )
			{
				lock ( _queue )
				{
					if ( _queue.Count > 0 )
					{
						first = _queue.First.Data;
						return true;
					}
				}
			}

			first = default( TItem );

			return false;
		}

		public bool TryDequeue( out TItem first )
		{
			if ( _queue.Count > 0 )
			{
				lock ( _queue )
				{
					if ( _queue.Count > 0 )
					{
						SimpleNode node = _queue.Dequeue();
						first = node.Data;
						RemoveFromNodeCache( node );
						return true;
					}
				}
			}

			first = default( TItem );
			return false;
		}

		public bool TryRemove( TItem item )
		{
			lock ( _queue )
			{
				SimpleNode removeMe;
				IList<SimpleNode> nodes;

				if ( item == null )
				{
					if ( _nullNodesCache.Count == 0 )
					{
						return false;
					}
					removeMe = _nullNodesCache[0];
					nodes = _nullNodesCache;
				}
				else
				{
					if ( !_itemToNodesCache.TryGetValue( item, out nodes ) )
					{
						return false;
					}
					removeMe = nodes[0];
					if ( nodes.Count == 1 )
					{
						_itemToNodesCache.Remove( item );
					}
				}

				_queue.Remove( removeMe );
				nodes.Remove( removeMe );

				return true;
			}
		}

		public bool TryUpdatePriority( TItem item, TPriority priority )
		{
			lock ( _queue )
			{
				SimpleNode updateMe = GetExistingNode( item );
				if ( updateMe == null )
				{
					return false;
				}
				_queue.UpdatePriority( updateMe, priority );
				return true;
			}
		}

		public bool TryGetPriority( TItem item, out TPriority priority )
		{
			lock ( _queue )
			{
				SimpleNode findMe = GetExistingNode( item );
				if ( findMe == null )
				{
					priority = default( TPriority );
					return false;
				}
				priority = findMe.Priority;
				return true;
			}
		}
		#endregion

		public IEnumerator<TItem> GetEnumerator()
		{
			List<TItem> queueData = new List<TItem>();

			lock ( _queue )
			{
				foreach ( var node in _queue )
				{
					queueData.Add( node.Data );
				}
			}

			return queueData.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public bool IsValidQueue()
		{
			lock ( _queue )
			{
				foreach ( IList<SimpleNode> nodes in _itemToNodesCache.Values )
				{
					foreach ( SimpleNode node in nodes )
					{
						if ( !_queue.Contains( node ) )
						{
							return false;
						}
					}
				}

				foreach ( SimpleNode node in _queue )
				{
					if ( GetExistingNode( node.Data ) == null )
					{
						return false;
					}
				}

				return _queue.IsValidQueue();
			}
		}
	}

	public class SimplePriorityQueue<TItem> : SimplePriorityQueue<TItem, float>
	{
		public SimplePriorityQueue() { }
		public SimplePriorityQueue( IComparer<float> comparer ) : base( comparer ) { }
		public SimplePriorityQueue( Comparison<float> comparer ) : base( comparer ) { }
	}
}
