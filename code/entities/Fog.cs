using Sandbox;

namespace RTS
{
	public partial class Fog : RenderEntity
	{
		public Material FogMaterial = Material.Load( "materials/rts/fog.vmat" );
		public Texture Texture { get; set; }
		public float MapSize { get; private set; }

		public Fog()
		{
			MapSize = 10000f;
			RenderBounds = new BBox( new Vector3( -MapSize, -MapSize ), new Vector3( MapSize, MapSize ) );
		}

		public override void DoRender( SceneObject sceneObject  )
		{
			var vertexBuffer = Render.GetDynamicVB( true );
			var halfMapSize = MapSize / 2f;

			var a = new Vertex( Position + new Vector3( -halfMapSize, -halfMapSize, 0.1f ), Vector3.Up, Vector3.Right, new Vector4( 0, 0, 0, 0 ) );
			var b = new Vertex( Position + new Vector3( halfMapSize, -halfMapSize, 0.1f ), Vector3.Up, Vector3.Right, new Vector4( 1, 0, 0, 0 ) );
			var c = new Vertex( Position + new Vector3( halfMapSize, halfMapSize, 0.1f ), Vector3.Up, Vector3.Right, new Vector4( 1, 1, 0, 0 ) );
			var d = new Vertex( Position + new Vector3( -halfMapSize, halfMapSize, 0.1f ), Vector3.Up, Vector3.Right, new Vector4( 0, 1, 0, 0 ) );

			vertexBuffer.AddQuad( a, b, c, d );
			vertexBuffer.Draw( FogMaterial );
		}
	}
}
