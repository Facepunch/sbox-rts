using Sandbox;

namespace Facepunch.RTS
{
	public partial class AbilityCircle : RenderEntity
	{
		public Material CircleMaterial = Material.Load( "materials/rts/ability_circle.vmat" );
		public Color TargetColor { get; set; }
		public Color EffectColor { get; set; }
		public float TargetSize { get; set; } = 30f;
		public float EffectSize { get; set; } = 60f;

		public override void DoRender( SceneObject sceneObject  )
		{
			if ( !EnableDrawing ) return;

			var vb = Render.GetDynamicVB( true );

			DrawCircle( vb, EffectSize, EffectColor, 0.2f );
			DrawCircle( vb, TargetSize, TargetColor, 0.3f );
		}

		private void DrawCircle( VertexBuffer vb, float size, Color color, float alpha )
		{
			var a = new Vertex( Position + new Vector3( -size, -size, 0.1f ), Vector3.Up, Vector3.Right, new Vector4( 0, 1, 0, 0 ) );
			var b = new Vertex( Position + new Vector3( size, -size, 0.1f ), Vector3.Up, Vector3.Right, new Vector4( 1, 1, 0, 0 ) );
			var c = new Vertex( Position + new Vector3( size, size, 0.1f ), Vector3.Up, Vector3.Right, new Vector4( 1, 0, 0, 0 ) );
			var d = new Vertex( Position + new Vector3( -size, size, 0.1f ), Vector3.Up, Vector3.Right, new Vector4( 0, 0, 0, 0 ) );

			vb.AddQuad( a, b, c, d );

			Render.Set( "Opacity", alpha );
			Render.Set( "Color", color );

			vb.Draw( CircleMaterial );
		}
	}
}
