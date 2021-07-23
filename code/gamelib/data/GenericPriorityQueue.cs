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
	public sealed class GenericPriorityQueue<TItem, TPriority> : IFixedSizePriorityQueue<TItem, TPriority>
		where TItem : GenericPriorityQueueNode<TPriority>
	{
		private int _numNodes;
		private TItem[] _nodes;
		private long _numNodesEverEnqueued;
		private readonly Comparison<TPriority> _comparer;

		public GenericPriorityQueue( int maxNodes ) : this( maxNodes, Comparer<TPriority>.Default ) { }

		public GenericPriorityQueue( int maxNodes, IComparer<TPriority> comparer ) : this( maxNodes, comparer.Compare ) { }

		public GenericPriorityQueue( int maxNodes, Comparison<TPriority> comparer )
		{
			_numNodes = 0;
			_nodes = new TItem[maxNodes + 1];
			_numNodesEverEnqueued = 0;
			_comparer = comparer;
		}

		public int Count => _numNodes
		public int MaxSize => _nodes.Length - 1;

		public void Clear()
		{
			Array.Clear( _nodes, 1, _numNodes );
			_numNodes = 0;
		}

		public bool Contains( TItem node )
		{
			return (_nodes[node.QueueIndex] == node);
		}

		public void Enqueue( TItem node, TPriority priority )
		{
			node.Priority = priority;
			_numNodes++;
			_nodes[_numNodes] = node;
			node.QueueIndex = _numNodes;
			node.InsertionIndex = _numNodesEverEnqueued++;

			CascadeUp( node );
		}

		private void CascadeUp( TItem node )
		{
			int parent;

			if ( node.QueueIndex > 1 )
			{
				parent = node.QueueIndex >> 1;
				TItem parentNode = _nodes[parent];

				if ( HasHigherPriority( parentNode, node ) )
					return;

				_nodes[node.QueueIndex] = parentNode;
				parentNode.QueueIndex = node.QueueIndex;

				node.QueueIndex = parent;
			}
			else
			{
				return;
			}

			while ( parent > 1 )
			{
				parent >>= 1;
				TItem parentNode = _nodes[parent];

				if ( HasHigherPriority( parentNode, node ) )
					break;

				_nodes[node.QueueIndex] = parentNode;
				parentNode.QueueIndex = node.QueueIndex;

				node.QueueIndex = parent;
			}
			_nodes[node.QueueIndex] = node;
		}

		private void CascadeDown( TItem node )
		{
			int finalQueueIndex = node.QueueIndex;
			int childLeftIndex = 2 * finalQueueIndex;

			if ( childLeftIndex > _numNodes )
			{
				return;
			}

			int childRightIndex = childLeftIndex + 1;
			TItem childLeft = _nodes[childLeftIndex];

			if ( HasHigherPriority( childLeft, node ) )
			{
				if ( childRightIndex > _numNodes )
				{
					node.QueueIndex = childLeftIndex;
					childLeft.QueueIndex = finalQueueIndex;
					_nodes[finalQueueIndex] = childLeft;
					_nodes[childLeftIndex] = node;
					return;
				}

				TItem childRight = _nodes[childRightIndex];

				if ( HasHigherPriority( childLeft, childRight ) )
				{
					childLeft.QueueIndex = finalQueueIndex;
					_nodes[finalQueueIndex] = childLeft;
					finalQueueIndex = childLeftIndex;
				}
				else
				{
					childRight.QueueIndex = finalQueueIndex;
					_nodes[finalQueueIndex] = childRight;
					finalQueueIndex = childRightIndex;
				}
			}
			else if ( childRightIndex > _numNodes )
			{
				return;
			}
			else
			{
				TItem childRight = _nodes[childRightIndex];

				if ( HasHigherPriority( childRight, node ) )
				{
					childRight.QueueIndex = finalQueueIndex;
					_nodes[finalQueueIndex] = childRight;
					finalQueueIndex = childRightIndex;
				}
				else
				{
					return;
				}
			}

			while ( true )
			{
				childLeftIndex = 2 * finalQueueIndex;

				if ( childLeftIndex > _numNodes )
				{
					node.QueueIndex = finalQueueIndex;
					_nodes[finalQueueIndex] = node;
					break;
				}

				childRightIndex = childLeftIndex + 1;
				childLeft = _nodes[childLeftIndex];

				if ( HasHigherPriority( childLeft, node ) )
				{
					if ( childRightIndex > _numNodes )
					{
						node.QueueIndex = childLeftIndex;
						childLeft.QueueIndex = finalQueueIndex;
						_nodes[finalQueueIndex] = childLeft;
						_nodes[childLeftIndex] = node;
						break;
					}

					TItem childRight = _nodes[childRightIndex];

					if ( HasHigherPriority( childLeft, childRight ) )
					{
						childLeft.QueueIndex = finalQueueIndex;
						_nodes[finalQueueIndex] = childLeft;
						finalQueueIndex = childLeftIndex;
					}
					else
					{
						childRight.QueueIndex = finalQueueIndex;
						_nodes[finalQueueIndex] = childRight;
						finalQueueIndex = childRightIndex;
					}
				}
				else if ( childRightIndex > _numNodes )
				{
					node.QueueIndex = finalQueueIndex;
					_nodes[finalQueueIndex] = node;
					break;
				}
				else
				{
					TItem childRight = _nodes[childRightIndex];

					if ( HasHigherPriority( childRight, node ) )
					{
						childRight.QueueIndex = finalQueueIndex;
						_nodes[finalQueueIndex] = childRight;
						finalQueueIndex = childRightIndex;
					}
					else
					{
						node.QueueIndex = finalQueueIndex;
						_nodes[finalQueueIndex] = node;
						break;
					}
				}
			}
		}

		private bool HasHigherPriority( TItem higher, TItem lower )
		{
			var cmp = _comparer( higher.Priority, lower.Priority );
			return (cmp < 0 || (cmp == 0 && higher.InsertionIndex < lower.InsertionIndex));
		}

		public TItem Dequeue()
		{
			TItem returnMe = _nodes[1];

			if ( _numNodes == 1 )
			{
				_nodes[1] = null;
				_numNodes = 0;
				return returnMe;
			}

			TItem formerLastNode = _nodes[_numNodes];
			_nodes[1] = formerLastNode;
			formerLastNode.QueueIndex = 1;
			_nodes[_numNodes] = null;
			_numNodes--;

			CascadeDown( formerLastNode );
			return returnMe;
		}

		public void Resize( int maxNodes )
		{
			TItem[] newArray = new TItem[maxNodes + 1];
			int highestIndexToCopy = Math.Min( maxNodes, _numNodes );
			Array.Copy( _nodes, newArray, highestIndexToCopy + 1 );
			_nodes = newArray;
		}

		public TItem First => _nodes[1];

		public void UpdatePriority( TItem node, TPriority priority )
		{
			node.Priority = priority;
			OnNodeUpdated( node );
		}

		private void OnNodeUpdated( TItem node )
		{
			int parentIndex = node.QueueIndex >> 1;

			if ( parentIndex > 0 && HasHigherPriority( node, _nodes[parentIndex] ) )
			{
				CascadeUp( node );
			}
			else
			{
				CascadeDown( node );
			}
		}

		public void Remove( TItem node )
		{
			if ( node.QueueIndex == _numNodes )
			{
				_nodes[_numNodes] = null;
				_numNodes--;
				return;
			}

			TItem formerLastNode = _nodes[_numNodes];
			_nodes[node.QueueIndex] = formerLastNode;
			formerLastNode.QueueIndex = node.QueueIndex;
			_nodes[_numNodes] = null;
			_numNodes--;

			OnNodeUpdated( formerLastNode );
		}

		public void ResetNode( TItem node )
		{
			node.QueueIndex = 0;
		}


		public IEnumerator<TItem> GetEnumerator()
		{
            IEnumerable<TItem> e = new ArraySegment<TItem>(_nodes, 1, _numNodes);
            return e.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public bool IsValidQueue()
		{
			for ( int i = 1; i < _nodes.Length; i++ )
			{
				if ( _nodes[i] != null )
				{
					int childLeftIndex = 2 * i;
					if ( childLeftIndex < _nodes.Length && _nodes[childLeftIndex] != null && HasHigherPriority( _nodes[childLeftIndex], _nodes[i] ) )
						return false;

					int childRightIndex = childLeftIndex + 1;
					if ( childRightIndex < _nodes.Length && _nodes[childRightIndex] != null && HasHigherPriority( _nodes[childRightIndex], _nodes[i] ) )
						return false;
				}
			}

			return true;
		}
	}
}
