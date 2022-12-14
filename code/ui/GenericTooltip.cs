
using Gamelib.Extensions;
using Facepunch.RTS.Units;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Facepunch.RTS
{
	[StyleSheet( "/ui/GenericTooltip.scss" )]
	public class GenericTooltip : Panel
	{
		public static GenericTooltip Instance { get; private set; }

		public Label Name { get; private set; }
		public Label Desc { get; private set; }
		public float HideTime { get; private set; }
		public bool IsShowing { get; private set; }
		public object Target { get; private set; }
		public string CurrentClass { get; private set; }

		public GenericTooltip()
		{
			Name = Add.Label( "", "name" );
			Desc = Add.Label( "", "desc" );

			Instance = this;
		}

		public void Show( float hideTime = 0f )
		{
			IsShowing = true;
			HideTime = hideTime > 0f ? Time.Now + hideTime : 0f;
		}

		public void Hide()
		{
			IsShowing = false;
			HideTime = 0f;
		}

		public void Hover( Entity entity )
		{
			Target = entity;
			UpdatePosition();
		}

		public void Hover( Panel panel )
		{
			Target = panel;
			UpdatePosition();
		}

		public void Update( string name, string description, string className = null )
		{
			if ( !string.IsNullOrEmpty( CurrentClass ) )
			{
				RemoveClass( CurrentClass );
				CurrentClass = null;
			}

			if ( !string.IsNullOrEmpty( className ) )
			{
				AddClass( className );
				CurrentClass = className;
			}

			description = Regex.Replace( description, "(\\+iv_[a-zA-Z0-9]+)", ( match ) =>
			{
				return Input.GetKeyWithBinding( match.Value );
			} );

			Name.Text = name;
			Desc.Text = description;
		}

		public override void Tick()
		{
			SetClass( "hidden", !IsShowing || (HideTime > 0f && Time.Now >= HideTime) );

			if ( IsShowing && HideTime > 0f && Time.Now > HideTime )
			{
				HideTime = 0f;
				IsShowing = false;
			}

			if ( IsShowing )
				UpdatePosition();

			base.Tick();
		}

		private void UpdatePosition()
		{
			if ( Target is Panel panel )
			{
				var targetBox = panel.Box.Rect * ScaleFromScreen;

				Style.Left = Length.Pixels( MathF.Ceiling( targetBox.Center.x ) );
				Style.Top = Length.Pixels( MathF.Ceiling( targetBox.Top - 32 ) );
				Style.Dirty();
			}
			else if ( Target is Entity entity && entity.IsValid() )
			{
				var position = entity.Position.ToScreen() * new Vector3( Screen.Width, Screen.Height ) * ScaleFromScreen;

				Style.Left = Length.Pixels( MathF.Ceiling( position.x ) );
				Style.Top = Length.Pixels( MathF.Ceiling( position.y - 32 ) );
				Style.Dirty();
			}
		}
	}
}
