using System;
using System.Collections;
using System.Collections.Generic;

namespace Gamelib.Data
{
	public sealed class FastPriorityQueue<T> : IFixedSizePriorityQueue<T, float> where T : FastPriorityQueueNode
	{
		private int _numNodes;
		private T[] _nodes;

		public FastPriorityQueue( int maxNodes )
		{
			_numNodes = 0;
			_nodes = new T[maxNodes + 1];
		}

		public int Count => _numNodes;
		public int MaxSize => _nodes.Length - 1;

		public void Clear()
		{
			Array.Clear( _nodes, 1, _numNodes );
			_numNodes = 0;
		}

		public bool Contains( T node )
		{
			return (_nodes[node.QueueIndex] == node);
		}

		public void Enqueue( T node, float priority )
		{
			node.Priority = priority;
			_numNodes++;
			_nodes[_numNodes] = node;
			node.QueueIndex = _numNodes;
			CascadeUp( node );
		}

		private void CascadeUp( T node )
		{
			int parent;

			if ( node.QueueIndex > 1 )
			{
				parent = node.QueueIndex >> 1;
				T parentNode = _nodes[parent];

				if ( HasHigherOrEqualPriority( parentNode, node ) )
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

				T parentNode = _nodes[parent];

				if ( HasHigherOrEqualPriority( parentNode, node ) )
					break;

				_nodes[node.QueueIndex] = parentNode;
				parentNode.QueueIndex = node.QueueIndex;
				node.QueueIndex = parent;
			}

			_nodes[node.QueueIndex] = node;
		}

		private void CascadeDown( T node )
		{
			int finalQueueIndex = node.QueueIndex;
			int childLeftIndex = 2 * finalQueueIndex;

			if ( childLeftIndex > _numNodes )
			{
				return;
			}

			int childRightIndex = childLeftIndex + 1;
			T childLeft = _nodes[childLeftIndex];

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

				T childRight = _nodes[childRightIndex];

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
				T childRight = _nodes[childRightIndex];

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

					T childRight = _nodes[childRightIndex];

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
					T childRight = _nodes[childRightIndex];

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

		private bool HasHigherPriority( T higher, T lower )
		{
			return (higher.Priority < lower.Priority);
		}

		private bool HasHigherOrEqualPriority( T higher, T lower )
		{
			return (higher.Priority <= lower.Priority);
		}

		public T Dequeue()
		{
			T returnMe = _nodes[1];

			if ( _numNodes == 1 )
			{
				_nodes[1] = null;
				_numNodes = 0;
				return returnMe;
			}

			T formerLastNode = _nodes[_numNodes];
			_nodes[1] = formerLastNode;
			formerLastNode.QueueIndex = 1;
			_nodes[_numNodes] = null;
			_numNodes--;

			CascadeDown( formerLastNode );

			return returnMe;
		}
		public void Resize( int maxNodes )
		{
			T[] newArray = new T[maxNodes + 1];
			int highestIndexToCopy = Math.Min( maxNodes, _numNodes );
			Array.Copy( _nodes, newArray, highestIndexToCopy + 1 );
			_nodes = newArray;
		}

		public T First => _nodes[1];

		public void UpdatePriority( T node, float priority )
		{
			node.Priority = priority;
			OnNodeUpdated( node );
		}

		private void OnNodeUpdated( T node )
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

		public void Remove( T node )
		{
			if ( node.QueueIndex == _numNodes )
			{
				_nodes[_numNodes] = null;
				_numNodes--;
				return;
			}

			T formerLastNode = _nodes[_numNodes];
			_nodes[node.QueueIndex] = formerLastNode;
			formerLastNode.QueueIndex = node.QueueIndex;
			_nodes[_numNodes] = null;
			_numNodes--;

			OnNodeUpdated( formerLastNode );
		}

		public void ResetNode( T node )
		{
			node.QueueIndex = 0;
		}

		public IEnumerator<T> GetEnumerator()
		{
			IEnumerable<T> e = new ArraySegment<T>(_nodes, 1, _numNodes);
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
