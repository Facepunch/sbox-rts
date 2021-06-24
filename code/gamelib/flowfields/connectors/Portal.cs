namespace Gamelib.FlowFields.Connectors
{
    public class Portal
    {
        public Gateway Gateway1;
        public Gateway Gateway2;
        public long Index;

        public Portal( long index, Gateway gateway1, Gateway gateway2 )
        {
            Gateway1 = gateway1;
            Gateway2 = gateway2;
            Gateway1.Portal = this;
            Gateway2.Portal = this;
            Index = index;
        }

        public bool HasChunk( int index )
        {
            return Gateway1.Chunk == index || Gateway2.Chunk == index;
        }

        public Gateway GetGatewayInChunk( int index )
        {
            return index == Gateway1.Chunk ? Gateway1 : Gateway2;
        }

        public Gateway OppositeGateway( Gateway connectionGateway )
        {
            if ( connectionGateway.Equals( Gateway1 ) ) return Gateway2;
            return Gateway1;
        }

        public override string ToString()
        {
            return $"{Gateway1.Chunk}:{Gateway1.Median()} => {Gateway2.Chunk}:{Gateway2.Median()}";
        }

        public Vector3 GetVector( Pathfinder pathfinder )
        {
            return (pathfinder.GetPosition( Gateway1.Chunk, Gateway1.Median() ) +
                    pathfinder.GetPosition( Gateway2.Chunk, Gateway2.Median() )) / 2;
        }
    }
}
