
using Gamelib.Extensions;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.RTS
{
	public class IdleUtilityUnitsButton : Panel
	{
		public List<UnitEntity> IdleUnits { get; private set; }
		public Label IdleCount { get; private set; }
		public Panel Icon { get; private set; }

		private RealTimeUntil NextUpdateList;

		protected override void OnClick( MousePanelEvent e )
		{
			if ( Local.Pawn is not RTSPlayer player ) return;

			if ( IdleUnits.Count == 0 ) return;

			if ( Input.Down( InputButton.Duck ) )
			{
				var list = string.Join( ",", IdleUnits.Select( u => u.NetworkIdent ) );
				Items.Select( list );
				return;
			}

			var currentUtilityUnit = player.GetSelected<UnitEntity>()
				.Where( IdleUnitMatcher )
				.FirstOrDefault();

			if ( currentUtilityUnit == null )
			{
				SelectFirstIdleUnit();
				return;
			}

			var indexOf = IdleUnits.IndexOf( currentUtilityUnit );

			if ( indexOf < 0 )
			{
				SelectFirstIdleUnit();
				return;
			}

			var nextIdleUnitIndex = indexOf + 1;

			if ( nextIdleUnitIndex >= IdleUnits.Count )
				nextIdleUnitIndex = 0;

			var idleUnit = IdleUnits[nextIdleUnitIndex];
			Items.Select( idleUnit.NetworkIdent.ToString() );

			if ( Input.Down( InputButton.Run ) )
				Items.FocusCameraOn( idleUnit );

			base.OnClick( e );
		}

		public IdleUtilityUnitsButton()
		{
			Icon = Add.Panel( "icon" );
			IdleCount = Add.Label( "", "label" );
			IdleUnits = new();
		}

		public override void Tick()
		{
			if ( Local.Pawn is RTSPlayer player )
			{
				if ( NextUpdateList )
				{
					IdleUnits.Clear();
					IdleUnits.AddRange( player.GetUnits().Where( IdleUnitMatcher ) );

					NextUpdateList = 1f;
				}

				IdleCount.Text = $"Idle: {IdleUnits.Count}";
			}
			else
			{
				IdleCount.Text = "N/A";
			}

			base.Tick();
		}

		private void SelectFirstIdleUnit()
		{
			var firstUnit = IdleUnits.First();

			Items.Select( firstUnit.NetworkIdent.ToString() );

			if ( Input.Down( InputButton.Run ) )
				Items.FocusCameraOn( firstUnit );
		}

		private bool IdleUnitMatcher( UnitEntity unit )
		{
			if ( !unit.CanConstruct && !unit.CanGatherAny() )
				return false;

			if ( unit.TargetType != UnitTargetType.None )
				return false;

			if ( unit.Occupiable != null )
				return false;

			return true;
 		}
	}

	public class IdleUtilityUnits : Panel
	{
		public IdleUtilityUnitsButton Button { get; private set; }

		public IdleUtilityUnits()
		{
			StyleSheet.Load( "/ui/IdleUtilityUnits.scss" );

			Button = AddChild<IdleUtilityUnitsButton>( "button" );
		}

		public override void Tick()
		{
			SetClass( "hidden", !Hud.IsLocalPlaying() );

			base.Tick();
		}
	}
}
