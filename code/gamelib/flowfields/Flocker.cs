using Sandbox;
using System.Collections.Generic;

namespace Gamelib.FlowFields
{
	public struct Flocker
	{
		public Vector3 Position;
		public Vector3 Force;
		public IMoveAgent Agent;
		public List<IMoveAgent> Agents;
		public float MaxForce;
		public float MaxSpeed;

		public void Setup( IMoveAgent agent, List<IMoveAgent> agents, Vector3 position )
		{
			Position = position;
			Force = Vector3.Zero;
			Agent = agent;
			Agents = agents;
			MaxForce = 1f;
			MaxSpeed = 1f;
		}

		public void Flock( Vector3 target )
		{
			var seek = Seek( target );
			var sep = Separate() * 4f;
			var coh = Cohesion() * 0.1f;
			var ali = Align() * 0.5f;

			Force = ((seek + sep) + coh) + ali;
		}

		public Vector3 Seek( Vector3 target )
		{
			if ( target.Distance( Position ) <= 1f )
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

			foreach ( var agent in Agents )
			{
				if ( agent != Agent )
				{
					var distance = Agent.Position.Distance( agent.Position );

					if ( distance < Agent.AgentRadius && distance > 0 )
					{
						var pushForce = Agent.Position - agent.Position;
						pushForce = pushForce.Normal * (1f - (pushForce.Length / Agent.AgentRadius));
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

			foreach ( var agent in Agents )
			{
				var distance = Agent.Position.Distance( agent.Position );

				if ( distance < Agent.AgentRadius && agent.Velocity.Length > 0 )
				{
					averageHeading = averageHeading + agent.Velocity.Normal;
					neighboursCount++;
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

			foreach ( var agent in Agents )
			{
				if ( agent != Agent )
				{
					var distance = Agent.Position.Distance( agent.Position );

					if ( distance < Agent.AgentRadius )
					{
						centerOfMass = centerOfMass + agent.Position;
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
