using System.Collections.Generic;
using System.Linq;
using Gamelib.FlowFields.Grid;

namespace Gamelib.FlowFields.Algorithms
{
    public class Integration : IIntegration
    {
        public const int NoIndex = -1;
        public bool IsIntegrated;

        private readonly List<int> _openIndexes = new();
        private readonly IntegrationSort _sort;
        private readonly int[] _values;

        public Integration( GridDefinition definition )
        {
            Definition = definition;
            _values = new int[Definition.Size];
            _sort = new IntegrationSort( this );
            Reset();
        }

        public GridDefinition Definition { get; }

        public int GetValue( int index )
        {
            return _values[index];
        }

        private void Reset()
        {
            for ( var i = 0; i < Definition.Size; i++ )
                _values[i] = IntegrationService.UnIntegrated;

            _openIndexes.Clear();
            _sort.Reset();
        }

        public void SetValue( int index, int value )
        {
            _values[index] = value;
        }

        public void Enqueue( int index )
        {
            _openIndexes.Add( index );
            _openIndexes.Sort( (index1, index2) => _values[index1].CompareTo( _values[index2] ) );
        }

        public int Dequeue()
        {
            if (_openIndexes.Count == 0) return NoIndex;
            
            var index = _openIndexes[0];
			_openIndexes.RemoveAt(0);
            return index;
        }
    }
}
