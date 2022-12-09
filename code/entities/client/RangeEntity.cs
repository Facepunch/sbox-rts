using Sandbox;

namespace Facepunch.RTS
{
	public partial class RangeEntity : RenderEntity
	{
		public Material CircleMaterial = Material.Load( "materials/rts/ability_circle.vmat" );
		public Color Color { get; set; }
		public float Size { get; set; } = 30f;

		public override void DoRender( SceneObject sceneObject  )
		{
			if ( !EnableDrawing ) return;

			var vb = new VertexBuffer();
			vb.Init( true );

			DrawCircle( vb, Size, Color, 0.3f );
		}

		private void DrawCircle( VertexBuffer vb, float size, Color color, float alpha )
		{
			var a = new Vertex( new Vector3( -size, -size, 0.1f ), Vector3.Up, Vector3.Right, new Vector4( 0, 1, 0, 0 ) );
			var b = new Vertex( new Vector3( size, -size, 0.1f ), Vector3.Up, Vector3.Right, new Vector4( 1, 1, 0, 0 ) );
			var c = new Vertex( new Vector3( size, size, 0.1f ), Vector3.Up, Vector3.Right, new Vector4( 1, 0, 0, 0 ) );
			var d = new Vertex( new Vector3( -size, size, 0.1f ), Vector3.Up, Vector3.Right, new Vector4( 0, 0, 0, 0 ) );

			vb.AddQuad( a, b, c, d );

			var attributes = new RenderAttributes();

			attributes.Set( "Opacity", alpha );
			attributes.Set( "Color", color );

			vb.Draw( CircleMaterial, attributes );
		}
	}
}
