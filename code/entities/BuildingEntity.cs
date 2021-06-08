﻿using RTS.Buildings;
using Sandbox;
using Steamworks.Data;
using System;
using System.Collections.Generic;

namespace RTS
{
	public partial class BuildingEntity : ItemEntity<BaseBuilding>
	{
		public uint LastQueueId { get; set; }
		public List<QueueItem> Queue { get; set; }

		public BuildingEntity() : base()
		{
			Tags.Add( "building", "selectable" );
			Queue = new();
		}

		protected override void OnPlayerAssigned( Player player )
		{
			var item = Item;

			if ( !player.Dependencies.Contains( item.NetworkId ) )
				player.Dependencies.Add( item.NetworkId );

			base.OnPlayerAssigned( player );
		}

		protected override void OnItemChanged( BaseBuilding item )
		{
			if ( !string.IsNullOrEmpty( item.Model ) )
			{
				SetModel( item.Model );
				SetupPhysicsFromModel( PhysicsMotionType.Static );
			}

			Health = item.MaxHealth;

			base.OnItemChanged( item );
		}

		public void StartBuild( BaseItem item )
		{
			Host.AssertServer();

			LastQueueId++;

			var queueItem = new QueueItem
			{
				Item = item,
				Id = LastQueueId
			};

			Queue.Add( queueItem );

			AddToQueue( To.Single( Player ), LastQueueId, item.NetworkId );

			if ( Queue.Count == 1 )
			{
				queueItem.Start();
				StartQueueItem( To.Single( Player ), LastQueueId, queueItem.FinishTime );
			}
		}

		public void StopBuild( uint queueId )
		{
			Host.AssertServer();

			for ( var i = Queue.Count - 1; i >= 0; i-- )
			{
				if ( Queue[i].Id == queueId )
				{
					Queue.RemoveAt( i );
					break;
				}
			}

			RemoveFromQueue( To.Single( Player ), queueId );

			if ( Queue.Count > 0 )
			{
				var firstItem = Queue[0];

				if ( firstItem.FinishTime == 0f )
				{
					firstItem.Start();
					StartQueueItem( To.Single( Player ), firstItem.Id, firstItem.FinishTime );
				}
			}
		}

		[Event.Tick.Server]
		public virtual void ServerTick()
		{
			if ( Queue.Count > 0 )
			{
				var firstItem = Queue[0];

				if ( firstItem.FinishTime > 0f && Game.Instance.ServerTime >= firstItem.FinishTime )
				{
					OnBuildCompleted( firstItem );
					StopBuild( firstItem.Id );
				}
			}
		}

		protected virtual void OnBuildCompleted( QueueItem queueItem )
		{

		}

		[ClientRpc]
		private void StartQueueItem( uint queueId, float finishTime )
		{
			for ( var i = Queue.Count - 1; i >= 0; i-- )
			{
				if ( Queue[i].Id == queueId )
				{
					Queue[i].FinishTime = finishTime;
					return;
				}
			}
		}

		[ClientRpc]
		private void RemoveFromQueue( uint queueId )
		{
			for ( var i = Queue.Count - 1; i >= 0; i-- )
			{
				if ( Queue[i].Id == queueId )
				{
					Queue.RemoveAt( i );
					return;
				}
			}
		}

		[ClientRpc]
		private void AddToQueue( uint queueId, uint itemId )
		{
			var queueItem = new QueueItem
			{
				Item = ItemManager.Instance.Find<BaseItem>( itemId ),
				Id = queueId
			};

			Queue.Add( queueItem );
		}
	}
}
