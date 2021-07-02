using Sandbox;

namespace Facepunch.RTS
{
	public partial class Placeholder : RenderEntity
	{
		public Material Material = Material.Load( "materials/placeholder.vmat" );
		public Color Color { get; set; }
		public float Alpha { get; set; } = 1f;

		public override void DoRender( SceneObject sceneObject  )
		{
			if ( !EnableDrawing ) return;

			var vertexBuffer = Render.GetDynamicVB( true );

			vertexBuffer.AddCube( Position, RenderBounds.Size, Rotation );

			Render.Set( "TintColor", Color );
			Render.Set( "Translucency", Alpha );

			vertexBuffer.Draw( Material );
		}
	}
}
