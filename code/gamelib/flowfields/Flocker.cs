using Sandbox;
using System.Collections.Generic;

namespace Gamelib.FlowFields
{
	public struct Flocker
	{
		public Vector3 Position;
		public Vector3 Force;
		public IFlockAgent Agent;
		public IEnumerable<IFlockAgent> Agents;

		public void Setup( IFlockAgent agent, IEnumerable<IFlockAgent> agents, Vector3 position )
		{
			Position = position;
			Force = Vector3.Zero;
			Agent = agent;
			Agents = agents;
		}

		public void Flock( Vector3 target )//
		{
			var seek = Seek( target );
			var sep = Separate() * 2.5f;
			var coh = Cohesion() * 0.1f;
			var ali = Align() * 0.7f;

			Force = ((seek + sep) + coh) + ali;
		}

		public Vector3 Seek( Vector3 target )
		{
			if ( target.Distance( Position ) <= 1f )
				return Vector3.Zero;

			var settings = Agent.FlockSettings;
			var desired = target - Position;
			desired *= (settings.MaxSpeed / desired.Length );
			var velocityChange = desired - Agent.Velocity;
			return velocityChange * (settings.MaxForce / settings.MaxSpeed);
		}

		private Vector3 Separate()
		{
			var totalForce = Vector3.Zero;
			var neighboursCount = 0;
			var settings = Agent.FlockSettings;

			foreach ( var agent in Agents )
			{
				if ( agent != Agent )
				{
					var distance = Agent.Position.Distance( agent.Position );

					if ( distance < settings.Radius && distance > 0 )
					{
						var pushForce = Agent.Position - agent.Position;
						pushForce = pushForce.Normal * (1f - (pushForce.Length / settings.Radius));
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
			return totalForce * settings.MaxForce;
		}

		private Vector3 Align()
		{
			var averageHeading = Vector3.Zero;
			var neighboursCount = 0;
			var settings = Agent.FlockSettings;

			foreach ( var agent in Agents )
			{
				var distance = Agent.Position.Distance( agent.Position );

				if ( distance < settings.Radius && agent.Velocity.Length > 0 )
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

			var desired = averageHeading * settings.MaxSpeed;
			var force = desired - Agent.Velocity;

			return force * (settings.MaxForce / settings.MaxSpeed);
		}

		private Vector3 Cohesion()
		{
			var centerOfMass = Agent.Position;
			var neighboursCount = 1;
			var settings = Agent.FlockSettings;

			foreach ( var agent in Agents )
			{
				if ( agent != Agent )
				{
					var distance = Agent.Position.Distance( agent.Position );

					if ( distance < settings.Radius )
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
