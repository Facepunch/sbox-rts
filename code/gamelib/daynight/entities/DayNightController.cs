using Sandbox;
using System;
using System.Linq;

namespace Gamelib.DayNight
{
	public class DayNightGradient
	{
		private struct GradientNode
		{
			public Color Color;
			public float Time;

			public GradientNode( Color color, float time )
			{
				Color = color;
				Time = time;
			}
		}

		private GradientNode[] _nodes;

		public DayNightGradient( Color dawnColor, Color dayColor, Color duskColor, Color nightColor )
		{
			_nodes = new GradientNode[7];
			_nodes[0] = new GradientNode( nightColor, 0f );
			_nodes[1] = new GradientNode( nightColor, 0.2f );
			_nodes[2] = new GradientNode( dawnColor, 0.3f );
			_nodes[3] = new GradientNode( dayColor, 0.5f );
			_nodes[4] = new GradientNode( dayColor, 0.7f );
			_nodes[5] = new GradientNode( duskColor, 0.85f );
			_nodes[6] = new GradientNode( nightColor, 1f );
		}

		public Color Evaluate( float fraction )
		{
			for ( var i = 0; i < _nodes.Length; i++ )
			{
				var node = _nodes[i];
				var nextIndex = i + 1;

				if ( _nodes.Length < nextIndex )
					nextIndex = 0;

				var nextNode = _nodes[nextIndex];

				if ( fraction >= node.Time && fraction <= nextNode.Time )
				{
					var duration = (nextNode.Time - node.Time);
					var interpolate = (1f / duration) * (fraction - node.Time);

					return Color.Lerp( node.Color, nextNode.Color, interpolate );
				}
			}

			return _nodes[0].Color;
		}
	}

	/// <summary>
	/// A way to set the colour based on the time of day, it will smoothly blend between each colour when the time has changed. Also enables the day night cycle using a "light_environment"
	/// </summary>
	[Library( "daynight_controller" )]
	[Hammer.EntityTool( "Day Night Controller", "Day and Night" )]
	[Hammer.EditorSprite("editor/daynight_controller.vmat")]
	public partial class DayNightController : ModelEntity
	{
		[Property( "DawnColor", Title = "Dawn Color" )]
		public Color DawnColor { get; set; }

		[Property( "DawnSkyColor", Title = "Dawn Sky Color" )]
		public Color DawnSkyColor { get; set; }

		[Property( "DayColor", Title = "Day Color" )]
		public Color DayColor { get; set; }

		[Property( "DaySkyColor", Title = "Day Sky Color" )]
		public Color DaySkyColor { get; set; }

		[Property( "DuskColor", Title = "Dusk Color" )]
		public Color DuskColor { get; set; }
		[Property( "DuskSkyColor", Title = "Dusk Sky Color" )]
		public Color DuskSkyColor { get; set; }

		[Property( "NightColor", Title = "Night Color" )]
		public Color NightColor { get; set; }

		[Property( "NightSkyColor", Title = "Night Sky Color" )]
		public Color NightSkyColor { get; set; }

		protected Output OnBecomeNight { get; set; }
		protected Output OnBecomeDusk { get; set; }
		protected Output OnBecomeDawn { get; set; }
		protected Output OnBecomeDay { get; set; }

		public EnvironmentLightEntity Environment
		{
			get
			{
				if ( _environment == null )
					_environment = All.OfType<EnvironmentLightEntity>().FirstOrDefault();
				return _environment;
			}
		}

		private EnvironmentLightEntity _environment;
		private DayNightGradient _skyColorGradient;
		private DayNightGradient _colorGradient;

		public override void Spawn()
		{
			_colorGradient = new DayNightGradient( DawnColor, DayColor, DuskColor, NightColor );
			_skyColorGradient = new DayNightGradient( DawnSkyColor, DaySkyColor, DuskSkyColor, NightSkyColor );

			DayNightManager.OnSectionChanged += HandleTimeSectionChanged;

			base.Spawn();
		}

		private void HandleTimeSectionChanged( TimeSection section )
		{
			if ( section == TimeSection.Dawn )
				OnBecomeDawn.Fire( this );
			else if ( section == TimeSection.Day )
				OnBecomeDay.Fire( this );
			else if ( section == TimeSection.Dusk )
				OnBecomeDusk.Fire( this );
			else if ( section == TimeSection.Night )
				OnBecomeNight.Fire( this );
		}

		[Event.Tick.Server]
		private void Tick()
		{
			var environment = Environment;
			if ( environment == null ) return;

			var sunAngle = ((DayNightManager.TimeOfDay / 24f) * 360f);
			var radius = 10000f;

			environment.Color = _colorGradient.Evaluate( (1f / 24f) * DayNightManager.TimeOfDay );
			environment.SkyColor = _skyColorGradient.Evaluate( (1f / 24f) * DayNightManager.TimeOfDay );

			environment.Position = Vector3.Zero + Rotation.From( 0, 0, sunAngle + 60f ) * ( radius * Vector3.Right );
			environment.Position += Rotation.From( 0, sunAngle, 0 ) * ( radius * Vector3.Forward );

			var direction = (Vector3.Zero - environment.Position).Normal;
			environment.Rotation = Rotation.LookAt( direction, Vector3.Up );
		}
	}
}
