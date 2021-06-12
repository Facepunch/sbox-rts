using Sandbox;
using System.Linq;

namespace RTS
{
	public partial class Fog : RenderEntity
	{
		public Material FogMaterial = Material.Load( "materials/rts/fog.vmat" );
		public Texture Texture { get; set; }

		public override void DoRender( SceneObject sceneObject )
		{
			var bounds = FogBounds.Instance;

			if ( !bounds.IsValid() ) return;

			var vertexBuffer = Render.GetDynamicVB( true );

			var a = new Vertex( FogBounds.TopLeft, Vector3.Up, Vector3.Right, new Vector4( 0, 0, 0, 0 ) );
			var b = new Vertex( FogBounds.TopRight, Vector3.Up, Vector3.Right, new Vector4( 1, 0, 0, 0 ) );
			var c = new Vertex( FogBounds.BottomRight, Vector3.Up, Vector3.Right, new Vector4( 1, 1, 0, 0 ) );
			var d = new Vertex( FogBounds.BottomLeft, Vector3.Up, Vector3.Right, new Vector4( 0, 1, 0, 0 ) );

			vertexBuffer.AddQuad( a, b, c, d );
			vertexBuffer.Draw( FogMaterial );
		}

		[Event.Tick.Client]
		private void ClientTick()
		{
			var bounds = FogBounds.Instance;

			if ( bounds.IsValid() )
			{
				RenderBounds = bounds.CollisionBounds;
			}
		}
	}
}
