using Sandbox;

namespace Facepunch.RTS
{
	public partial class GhostBounds : RenderEntity
	{
		public Material Material = Material.Load( "materials/rts/blueprint.vmat" );
		public Color Color { get; set; }
		public float Alpha { get; set; } = 1f;

		public override void DoRender( SceneObject sceneObject  )
		{
			if ( !EnableDrawing ) return;

			var vertexBuffer = Render.GetDynamicVB( true );
			var boundsSize = RenderBounds.Size / 2f;

			var a = new Vertex( new Vector3( -boundsSize.x, -boundsSize.y, 0.1f ), Vector3.Up, Vector3.Right, new Vector4( 0, 1, 0, 0 ) );
			var b = new Vertex( new Vector3( boundsSize.x, -boundsSize.y, 0.1f ), Vector3.Up, Vector3.Right, new Vector4( 1, 1, 0, 0 ) );
			var c = new Vertex( new Vector3( boundsSize.x, boundsSize.y, 0.1f ), Vector3.Up, Vector3.Right, new Vector4( 1, 0, 0, 0 ) );
			var d = new Vertex( new Vector3( -boundsSize.x, boundsSize.y, 0.1f ), Vector3.Up, Vector3.Right, new Vector4( 0, 0, 0, 0 ) );

			vertexBuffer.AddQuad( a, b, c, d );

			Render.Attributes.Set( "TintColor", Color );
			Render.Attributes.Set( "Translucency", Alpha );

			if ( Material != null )
			{
				vertexBuffer.Draw( Material );
			}
		}
	}
}
