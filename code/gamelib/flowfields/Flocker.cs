using Sandbox;
using System.Collections.Generic;

namespace Gamelib.FlowFields
{
	public struct Flocker
	{
		public Vector3 Position;
		public Vector3 Force;
		public IMoveAgent Agent;
		public IMoveAgent[] Agents;
		public float MaxForce;
		public float MaxSpeed;

		public void Setup( IMoveAgent agent, IMoveAgent[] agents, Vector3 position, float speed )
		{
			Position = position.WithZ( 0f );
			Force = Vector3.Zero;
			Agent = agent;
			Agents = agents;
			MaxForce = speed;
			MaxSpeed = speed * 0.5f;
		}

		public void Flock( Vector3 target )
		{
			var seek = Seek( target.WithZ( 0f ) );

			//Have each unit steer to avoid hitting its neighbors.
			var sep = Separate() * 4f; // MaxForce * 0.01f;
			// Have each unit steer toward the average position of its neighbors.
			var coh = Cohesion() * 0.5f;
			// Have each unit steer so as to align itself to the average heading of its neighbors.
			var ali = Align() * 0f;

			Force = ((seek + sep) + coh) + ali;
		}

		public Vector3 Seek( Vector3 target )
		{
			var tolerance = Agent.AgentRadius * 0.5f;

			if ( target.Distance( Position ) <= tolerance )
				return Vector3.Zero;

			var desired = target - Position;
			desired *= (MaxSpeed / desired.Length);
			var velocityChange = desired - Agent.Velocity;
			return velocityChange * (MaxForce / MaxSpeed);
		}

		private Vector3 Separate()
		{
			var totalForce = Vector3.Zero;
			var neighboursCount = 0;
			var ourPosition = Agent.Position.WithZ( 0f );
			var ourRadius = Agent.AgentRadius;

			for ( int i = 0; i < Agents.Length; i++ )
			{
				var agent = Agents[i];

				if ( agent == Agent )
					continue;

				if ( agent is Entity entity && entity.IsValid() )
				{
					var theirPosition = agent.Position.WithZ( 0f );
					var distance = ourPosition.Distance( theirPosition );

					if ( distance < ourRadius && distance > 0 )
					{
						var pushForce = ourPosition - theirPosition;
						pushForce = pushForce.Normal * (1f - (pushForce.Length / ourRadius));
						totalForce += pushForce;
						neighboursCount++;
					}
				}
			}

			if ( neighboursCount == 0 )
			{
				return Vector3.Zero;
			}

			totalForce /= neighboursCount;
			return totalForce * MaxForce;
		}

		private Vector3 Align()
		{
			var averageHeading = Vector3.Zero;
			var neighboursCount = 0;
			var ourPosition = Agent.Position.WithZ( 0f );
			var ourRadius = Agent.AgentRadius;

			for ( int i = 0; i < Agents.Length; i++ )
			{
				var agent = Agents[i];

				if ( agent == Agent )
					continue;

				if ( agent is Entity entity && entity.IsValid() )
				{
					var theirPosition = agent.Position.WithZ( 0f );
					var distance = ourPosition.Distance( theirPosition );

					if ( distance < ourRadius && agent.Velocity.Length > 0 )
					{
						averageHeading = averageHeading + agent.Velocity.Normal;
						neighboursCount++;
					}
				}
			}

			if ( neighboursCount == 0 )
			{
				return Vector2.Zero;
			}

			averageHeading /= neighboursCount;

			var desired = averageHeading * MaxSpeed;
			var force = desired - Agent.Velocity;

			return force * (MaxForce / MaxSpeed);
		}

		private Vector3 Cohesion()
		{
			var centerOfMass = Agent.Position;
			var neighboursCount = 1;
			var ourPosition = Agent.Position.WithZ( 0f );
			var ourRadius = Agent.AgentRadius;

			for ( int i = 0; i < Agents.Length; i++ )
			{
				var agent = Agents[i];

				if ( agent == Agent )
					continue;

				if ( agent is Entity entity && entity.IsValid() )
				{
					var theirPosition = agent.Position.WithZ( 0f );
					var distance = ourPosition.Distance( theirPosition );

					if ( distance < ourRadius )
					{
						centerOfMass = centerOfMass + theirPosition;
						neighboursCount++;
					}
				}
			}

			if ( neighboursCount == 1 )
			{
				return Vector2.Zero;
			}

			centerOfMass /= neighboursCount;

			return Seek( centerOfMass );
		}
	}
}
