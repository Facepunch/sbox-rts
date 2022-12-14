using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS
{
    public abstract partial class BaseRound : BaseNetworkable
	{
		public virtual int RoundDuration => 0;
		public virtual string RoundName => "";
		public virtual bool ShowTimeLeft => false;
		public virtual bool ShowRoundInfo => false;

		public List<RTSPlayer> Players = new();
		public RealTimeUntil NextSecondTime { get; private set; }
		public float RoundEndTime { get; set; }

		public float TimeLeft
		{
			get
			{
				return RoundEndTime - Time.Now;
			}
		}

		[Net] public int TimeLeftSeconds { get; set; }

		public void Start()
		{
			if ( Game.IsServer && RoundDuration > 0 )
				RoundEndTime = Time.Now + RoundDuration;
			
			OnStart();
		}

		public void Finish()
		{
			if ( Game.IsServer )
			{
				RoundEndTime = 0f;
				Players.Clear();
			}

			OnFinish();
		}

		public void AddPlayer( RTSPlayer player )
		{
			Game.AssertServer();

			if ( !Players.Contains(player) )
				Players.Add( player );
		}

		public virtual void OnPlayerJoin( RTSPlayer player ) { }

		public virtual void OnPlayerLeave( RTSPlayer player )
		{
			Players.Remove( player );
		}

		public virtual void OnTick()
		{
			if ( NextSecondTime )
			{
				OnSecond();
				NextSecondTime = 1f;
			}
		}

		public virtual void OnSecond()
		{
			if ( Game.IsServer )
			{
				if ( RoundEndTime > 0 && Time.Now >= RoundEndTime )
				{
					RoundEndTime = 0f;
					OnTimeUp();
				}
				else
				{
					TimeLeftSeconds = TimeLeft.CeilToInt();
				}
			}
		}

		protected virtual void OnStart() { }

		protected virtual void OnFinish() { }

		protected virtual void OnTimeUp() { }
	}
}
