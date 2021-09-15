﻿using Sandbox;
using Gamelib.Elo;
using System.Collections.Generic;
using Facepunch.RTS.Buildings;
using System.Linq;
using Facepunch.RTS.Units;
using System;
using Gamelib.Extensions;
using Gamelib.FlowFields.Entities;

namespace Facepunch.RTS
{
	public partial class Player : Entity
	{
		[Net, Local, OnChangedCallback] public List<uint> Dependencies { get; set; }
		[Net, Local, OnChangedCallback] public List<uint> Researching { get; set; }
		[Net, Local, OnChangedCallback] public List<Entity> Selection { get; set; }
		[Net, Local] public uint MaxPopulation { get; set; }
		[Net, Local] public int Population { get; private set; }
		[Net] public bool IsSpectator { get; private set;  }
		[Net] public EloScore Elo { get; private set; }
		[Net] public Color TeamColor { get; set; }
		[Net] public bool IsReady { get; set; }
		[Net] public List<int> Resources { get; private set; }
		[Net] public int TeamGroup { get; set; }
		public bool SkipAllWaiting { get; set;  }

		public new RTSCamera Camera
		{
			get => base.Camera as RTSCamera;
			set => base.Camera = value;
		}

		public HashSet<uint> InstantBuildCache { get; private set; }
		public TimeSince LastCommandSound { get; set; }
		public Client Client => GetClientOwner();

		public Player()
		{
			Elo = new();
			Camera = new RTSCamera();
			Transmit = TransmitType.Always;
			Resources = new List<int>();
			Selection = new List<Entity>();
			Researching = new List<uint>();
			Dependencies = new List<uint>();
			InstantBuildCache = new HashSet<uint>();
			MaxPopulation = 8;
			LastCommandSound = 0;
		}

		public IEnumerable<UnitEntity> GetUnits( BaseUnit unit)
		{
			return All.OfType<UnitEntity>().Where( i => i.Player == this && i.Item == unit );
		}

		public IEnumerable<UnitEntity> GetUnits()
		{
			return All.OfType<UnitEntity>().Where( i => i.Player == this );
		}

		public IEnumerable<BuildingEntity> GetBuildingsProxiesIncluded( BaseBuilding building )
		{
			return All.OfType<BuildingEntity>().Where( i => i.Player == this && i.Item.IsProxyOf( building ) );
		}

		public IEnumerable<BuildingEntity> GetBuildings( BaseBuilding building )
		{
			return All.OfType<BuildingEntity>().Where( i => i.Player == this && i.Item == building );
		}

		public IEnumerable<BuildingEntity> GetBuildings()
		{
			return All.OfType<BuildingEntity>().Where( i => i.Player == this );
		}

		public void AddPopulation( int population )
		{
			Population += population;
		}

		public void TakePopulation( int population )
		{
			Population = Math.Max( Population - population, 0 );
		}

		public List<ISelectable> GetAllSelected()
		{
			return GetSelected<ISelectable>();
		}

		public List<T> GetSelected<T>() where T : ISelectable
		{
			var output = new List<T>();

			foreach ( var entity in Selection )
			{
				if ( entity is T selected && selected.Player == this )
				{
					output.Add( selected );
				}
			}

			return output;
		}

		public List<T> ForEachSelected<T>( Func<T, bool> callback ) where T : ISelectable
		{
			var output = new List<T>();

			foreach ( var entity in Selection )
			{
				if ( entity is T selected && selected.Player == this )
				{
					if ( callback( selected ) )
						output.Add( selected );
				}
			}

			return output;
		}

		public bool HasPopulationCapacity( int population )
		{
			return (Population + population <= MaxPopulation);
		}

		public bool CanAfford( Dictionary<ResourceType, int> costs, out ResourceType missingResource )
		{
			foreach ( var kv in costs )
			{
				if ( GetResource( kv.Key ) < kv.Value )
				{
					missingResource = kv.Key;
					return false;
				}
			}

			missingResource = default;

			return true;
		}

		public bool CanAfford( Dictionary<ResourceType,int> costs )
		{
			foreach ( var kv in costs )
			{
				if ( GetResource( kv.Key ) < kv.Value )
					return false;
			}

			return true;
		}
		public bool CanAfford( BaseAbility ability )
		{
			return CanAfford( ability.Costs );
		}

		public bool CanAfford( BaseAbility ability, out ResourceType missingResource )
		{
			return CanAfford( ability.Costs, out missingResource );
		}

		public bool CanAfford( BaseItem item )
		{
			return CanAfford( item.Costs );
		}

		public bool CanAfford( BaseItem item, out ResourceType missingResource )
		{
			return CanAfford( item.Costs, out missingResource );
		}

		public IEnumerable<Player> GetAllTeamPlayers()
		{
			return All.OfType<Player>()
				.Where( p => p.TeamGroup == TeamGroup );
		}

		public IEnumerable<Client> GetAllTeamClients()
		{
			return All.OfType<Player>()
				.Where( p => p.TeamGroup == TeamGroup )
				.Select( p => p.GetClientOwner() );
		}

		public void GiveResources( Dictionary<ResourceType, int> resources, int multiplier = 1 )
		{
			foreach ( var kv in resources )
			{
				GiveResource( kv.Key, kv.Value * multiplier );
			}
		}

		public void TakeResources( Dictionary<ResourceType, int> resources )
		{
			foreach ( var kv in resources )
			{
				TakeResource( kv.Key, kv.Value );
			}
		}

		public void GiveResources( BaseAbility ability )
		{
			GiveResources( ability.Costs );
		}

		public void TakeResources( BaseAbility ability )
		{
			TakeResources( ability.Costs );
		}

		public void GiveResources( BaseItem item )
		{
			GiveResources( item.Costs );
		}

		public void TakeResources( BaseItem item )
		{
			TakeResources( item.Costs );
		}

		public void ClearResources()
		{
			Resources.Clear();
		}

		public int GetResource( ResourceType type )
		{
			if ( Resources == null ) return 0;
			if ( Resources.Count <= (int)type ) return 0;
			return Resources[(int)type];
		}

		public bool SetResource( ResourceType type, int amount )
		{
			if ( !Host.IsServer ) return false;
			if ( Resources == null ) return false;

			while ( Resources.Count <= (int)type )
			{
				Resources.Add( 0 );
			}

			Resources[(int)type] = amount;
			return true;
		}

		public bool GiveResource( ResourceType type, int amount )
		{
			if ( !Host.IsServer ) return false;
			if ( Resources == null ) return false;

			SetResource( type, GetResource( type ) + amount );
			return true;
		}

		public int TakeResource( ResourceType type, int amount )
		{
			if ( Resources == null ) return 0;

			var available = GetResource( type );
			amount = Math.Min( available, amount );

			SetResource( type, available - amount );
			return amount;
		}

		public void MakeSpectator( bool isSpectator )
		{
			IsSpectator = isSpectator;
		}

		public void RemoveDependency( BaseItem item )
		{
			if ( HasDependency( item ) )
				Dependencies.Remove( item.NetworkId );
		}

		public void AddDependency( BaseItem item )
		{
			if ( !HasDependency( item ) )
				Dependencies.Add( item.NetworkId );
		}

		public bool HasDependency( BaseItem item )
		{
			return Dependencies.Contains( item.NetworkId );
		}

		public void ClearSelection()
		{
			Host.AssertServer();

			var selection = GetAllSelected();

			for ( var i = selection.Count - 1; i >= 0; i-- )
			{
				var selectable = selection[i];

				if ( selectable.IsSelected )
					selectable.Deselect();
			}

			Selection.Clear();
		}

		[ClientRpc]
		public void LookAt( Vector3 position )
		{
			Camera.LookAt = position.WithZ( 0f );
		}

		[ClientRpc]
		public void LookAt( Entity other )
		{
			Camera.LookAt = other.Position.WithZ( 0f );
		}

		public override void Simulate( Client client )
		{
			if ( !Gamemode.Instance.IsValid() ) return;

			if ( IsServer && Input.Released( InputButton.Reload ) )
			{
				// TODO: This is just for testing, delete later.
				var trace = TraceExtension.RayDirection( Input.Cursor.Origin, Input.Cursor.Direction ).Run();
				var bot = Rounds.Current.Players.Where( player => player.GetClientOwner() != client ).FirstOrDefault();

				/*
				var worker = Items.Create<UnitEntity>( client.Pawn as Player, "unit.attackdrone" );
				worker.Position = trace.EndPos;
				*/

				var worker = Items.Create<UnitEntity>( bot, "unit.assault" );
				worker.Position = trace.EndPos;

				worker = Items.Create<UnitEntity>( client.Pawn as Player, "unit.assault" );
				worker.Position = trace.EndPos;

				/*
				var building = Items.Create<BuildingEntity>( bot, "building.commandcentre" );
				building.Position = trace.EndPos;
				building.FinishConstruction();
				*/
			}

			if ( IsServer && Input.Released( InputButton.Use ) )
			{
				// TODO: This is just for testing, delete later.
				var trace = TraceExtension.RayDirection( Input.Cursor.Origin, Input.Cursor.Direction ).Run();
				var bot = Rounds.Current.Players.Where( player => player.GetClientOwner() != client ).FirstOrDefault();

				/*
				var worker = Items.Create<UnitEntity>( client.Pawn as Player, "unit.attackdrone" );
				worker.Position = trace.EndPos;
				*/

				var worker = Items.Create<UnitEntity>( client.Pawn as Player, "unit.tank" );
				worker.Position = trace.EndPos;

				/*
				var building = Items.Create<BuildingEntity>( bot, "building.commandcentre" );
				building.Position = trace.EndPos;
				building.FinishConstruction();
				*/
			}

			if ( IsLocalPawn )
			{
				// We have to do this here for now because there's problems detecting it within panels themselves.
				if ( Input.Released( InputButton.Score ) )
				{
					SelectedItem.Instance.Next();
				}
			}

			base.Simulate( client );
		}

		private void OnResearchingChanged()
		{
			if ( IsLocalPawn ) SelectedItem.Instance.Update( Selection );
		}

		private void OnSelectionChanged()
		{
			if ( IsLocalPawn ) SelectedItem.Instance.Update( Selection );
		}

		private void OnDependenciesChanged()
		{
			if ( IsLocalPawn ) SelectedItem.Instance.Update( Selection );
		}
	}
}
