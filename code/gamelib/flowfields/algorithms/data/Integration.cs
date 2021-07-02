using Gamelib.FlowFields.Grid;

namespace Gamelib.FlowFields.Algorithms
{
    public class Integration : IIntegration
    {
        public const int NoIndex = -1;
        public bool IsIntegrated;

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

            _sort.Reset();
        }

        public void SetValue( int index, int value )
        {
            _values[index] = value;
        }

        public void Enqueue( int index )
        {
			_sort.Enqueue( index );
        }

        public int Dequeue()
        {
			return _sort.Dequeue();
        }
    }
}
