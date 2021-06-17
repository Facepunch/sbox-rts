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
			var manager = FogManager.Instance;
			var bounds = FogBounds.Instance;

			if ( !bounds.IsValid() || !manager.IsActive ) return;

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
				var doubleSize = bounds.CollisionBounds;
				doubleSize.Mins *= 2f;
				doubleSize.Maxs *= 2f;
				RenderBounds = doubleSize;
			}
		}
	}
}
