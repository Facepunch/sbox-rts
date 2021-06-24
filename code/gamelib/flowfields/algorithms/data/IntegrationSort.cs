using System.Collections.Generic;

namespace Gamelib.FlowFields.Algorithms
{
    public class IntegrationSort
    {
        private readonly IIntegration _integration;

        private readonly Queue<int> _openIndexes = new();
        private readonly Queue<int> _sortIndexes = new();
        private int _sortTop;
        private int _top;

        public IntegrationSort( IIntegration integration )
        {
            _integration = integration;
        }

        public void Reset()
        {
            _top = 0;
            _sortTop = 0;
            _openIndexes.Clear();
            _sortIndexes.Clear();
        }

        public void Enqueue( int index )
        {
            var value = _integration.GetValue( index );

            if (_top == 0)
                _top = value;

            if (_top > value)
            {
                _top = value;

                while ( _openIndexes.Count > 0 )
					_sortIndexes.Enqueue( _openIndexes.Dequeue() );
            }

            if ( _top + 1 >= _integration.GetValue( index ) )
            {
                _openIndexes.Enqueue(index);
            }
            else
            {
                if ( _sortTop == 0 )
                    _sortTop = _integration.GetValue( index );

                _sortIndexes.Enqueue(index);
            }
        }

        public int Dequeue()
        {
            if ( _sortTop < _top && _sortIndexes.Count > 0 )
            {
                var index = _sortIndexes.Dequeue();
                _sortTop = _integration.GetValue(index);
                return index;
            }

            if ( _openIndexes.Count > 0 )
            {
                var index = _openIndexes.Dequeue();
                _top = _integration.GetValue(index);
                return index;
            }

            if ( _sortIndexes.Count > 0 )
            {
                var index = _sortIndexes.Dequeue();
                _sortTop = _integration.GetValue(index);
                return index;
            }

            return Integration.NoIndex;
        }

        private int PeekValue()
        {
            if ( _sortTop < _top && _sortIndexes.Count > 0 ) return _integration.GetValue( _sortIndexes.Peek() );
            if ( _openIndexes.Count > 0 ) return _integration.GetValue( _openIndexes.Peek() );
            if ( _sortIndexes.Count > 0 ) return _integration.GetValue( _sortIndexes.Peek() );

            return Integration.NoIndex;
        }

        public void Dequeue( Queue<int> indexes, int value )
        {
            while ( PeekValue() == value )
                indexes.Enqueue(Dequeue());
        }
    }
}
