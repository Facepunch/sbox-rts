using Sandbox;

namespace RTS
{
	public partial class UnitCircle : RenderEntity
	{
		public Material CircleMaterial = Material.Load( "materials/rts/unit_circle.vmat" );
		public Color Color { get; set; }
		public float Size { get; set; } = 30f;
		public float Alpha { get; set; } = 1f;

		public override void DoRender( SceneObject sceneObject  )
		{
			if ( !EnableDrawing ) return;

			var vertexBuffer = Render.GetDynamicVB( true );
			var circleSize = Size;

			var a = new Vertex( Position + new Vector3( -circleSize, -circleSize, 0.1f ), Vector3.Up, Vector3.Right, new Vector4( 0, 1, 0, 0 ) );
			var b = new Vertex( Position + new Vector3( circleSize, -circleSize, 0.1f ), Vector3.Up, Vector3.Right, new Vector4( 1, 1, 0, 0 ) );
			var c = new Vertex( Position + new Vector3( circleSize, circleSize, 0.1f ), Vector3.Up, Vector3.Right, new Vector4( 1, 0, 0, 0 ) );
			var d = new Vertex( Position + new Vector3( -circleSize, circleSize, 0.1f ), Vector3.Up, Vector3.Right, new Vector4( 0, 0, 0, 0 ) );

			vertexBuffer.AddQuad( a, b, c, d );

			Render.Set( "Opacity", Alpha );
			Render.Set( "Color", Color );

			vertexBuffer.Draw( CircleMaterial );
		}
	}
}
