using Sandbox;
using System.Linq;

namespace Facepunch.RTS
{
	public partial class FogRenderer : RenderEntity
	{
		[ServerVar( "rts_fog", Saved = true )]
		public static bool Enabled { get; set; } = true;

		public Material FogMaterial = Material.Load( "materials/rts/fog.vmat" );
		public Texture Texture { get; set; }

		public override void DoRender( SceneObject sceneObject )
		{
			if ( !Enabled || !Fog.IsActive ) return;

			var vertexBuffer = Render.GetDynamicVB( true );
			var bounds = Fog.Bounds;

			var a = new Vertex( bounds.TopLeft, Vector3.Up, Vector3.Right, new Vector4( 0, 0, 0, 0 ) );
			var b = new Vertex( bounds.TopRight, Vector3.Up, Vector3.Right, new Vector4( 1, 0, 0, 0 ) );
			var c = new Vertex( bounds.BottomRight, Vector3.Up, Vector3.Right, new Vector4( 1, 1, 0, 0 ) );
			var d = new Vertex( bounds.BottomLeft, Vector3.Up, Vector3.Right, new Vector4( 0, 1, 0, 0 ) );

			vertexBuffer.AddQuad( a, b, c, d );
			vertexBuffer.Draw( FogMaterial );
		}
	}
}
