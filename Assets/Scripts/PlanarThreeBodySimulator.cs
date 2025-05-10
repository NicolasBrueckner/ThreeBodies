#region

using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

#endregion

public class PlanarThreeBodySimulator : MonoBehaviour
{
	[ Serializable ]
	public class Body
	{
		public double mass = 1f;
		public double2 initialPosition = double2.zero;
		public double2 initialVelocity = double2.zero;

		[ HideInInspector ]
		public List<double3> trajectory;
	}

	[ Header( "Bodies" ) ]
	public Body body0 = new();

	public Body body1 = new();
	public Body body2 = new();

	[ Header( "Integration Settings" ) ]
	public float period = 10f;

	public int steps = 10000;

	private void Start()
	{
		ComputeTrajectories();
	}

	/// <summary>
	///     Performs fixed-step RK4 integration over the period.
	/// </summary>
	public void ComputeTrajectories()
	{
		int n = steps;
		float dt = period / n;
		double t = 0.0;

		// State vector: [x0,y0,vx0,vy0, x1,y1,vx1,vy1, x2,y2,vx2,vy2]
		double[] y = new double[ 12 ]
		{
			body0.initialPosition.x, body0.initialPosition.y, body0.initialVelocity.x, body0.initialVelocity.y,
			body1.initialPosition.x, body1.initialPosition.y, body1.initialVelocity.x, body1.initialVelocity.y,
			body2.initialPosition.x, body2.initialPosition.y, body2.initialVelocity.x, body2.initialVelocity.y,
		};

		// Prepare trajectory lists
		body0.trajectory = new List<double3>( n + 1 );
		body1.trajectory = new List<double3>( n + 1 );
		body2.trajectory = new List<double3>( n + 1 );

		// Store initial positions
		StoreStep( 0, ( float )t, y );

		// Integrate over each step
		for( int i = 0; i < n; i++ )
		{
			y = RK4Step( y, dt );
			t += dt;
			StoreStep( i + 1, t, y );
		}
	}

	private void StoreStep( int index, double time, double[] y )
	{
		body0.trajectory.Add( new double3( y[ 0 ], ( float )y[ 1 ], time ) );
		body1.trajectory.Add( new double3( y[ 4 ], ( float )y[ 5 ], time ) );
		body2.trajectory.Add( new double3( y[ 8 ], ( float )y[ 9 ], time ) );
	}

	private double[] RK4Step( double[] y, double dt )
	{
		double[] k1 = ComputeDerivatives( y );
		double[] y2 = new double[ 12 ];
		for( int i = 0; i < 12; i++ ) y2[ i ] = y[ i ] + k1[ i ] * dt * 0.5;
		double[] k2 = ComputeDerivatives( y2 );

		double[] y3 = new double[ 12 ];
		for( int i = 0; i < 12; i++ ) y3[ i ] = y[ i ] + k2[ i ] * dt * 0.5;
		double[] k3 = ComputeDerivatives( y3 );

		double[] y4 = new double[ 12 ];
		for( int i = 0; i < 12; i++ ) y4[ i ] = y[ i ] + k3[ i ] * dt;
		double[] k4 = ComputeDerivatives( y4 );

		double[] yNext = new double[ 12 ];
		for( int i = 0; i < 12; i++ )
			yNext[ i ] = y[ i ] + dt / 6.0 * ( k1[ i ] + 2 * k2[ i ] + 2 * k3[ i ] + k4[ i ] );

		return yNext;
	}

	private double[] ComputeDerivatives( double[] y )
	{
		double[] dydt = new double[ 12 ];
		double m0 = body0.mass, m1 = body1.mass, m2 = body2.mass;

		// Velocity -> position derivatives
		dydt[ 0 ] = y[ 2 ];
		dydt[ 1 ] = y[ 3 ];
		dydt[ 4 ] = y[ 6 ];
		dydt[ 5 ] = y[ 7 ];
		dydt[ 8 ] = y[ 10 ];
		dydt[ 9 ] = y[ 11 ];

		// Pairwise gravitational accelerations
		AccumulateForce( y[ 0 ], y[ 1 ], y[ 4 ], y[ 5 ], m1, m0, ref dydt[ 2 ], ref dydt[ 3 ], ref dydt[ 6 ],
			ref dydt[ 7 ] );
		AccumulateForce( y[ 0 ], y[ 1 ], y[ 8 ], y[ 9 ], m2, m0, ref dydt[ 2 ], ref dydt[ 3 ], ref dydt[ 10 ],
			ref dydt[ 11 ] );
		AccumulateForce( y[ 4 ], y[ 5 ], y[ 8 ], y[ 9 ], m2, m1, ref dydt[ 6 ], ref dydt[ 7 ], ref dydt[ 10 ],
			ref dydt[ 11 ] );

		return dydt;
	}

	private void AccumulateForce( double xA, double yA, double xB, double yB,
		double mB, double mA,
		ref double axA, ref double ayA,
		ref double axB, ref double ayB )
	{
		double dx = xB - xA, dy = yB - yA;
		double r3 = math.pow( dx * dx + dy * dy, 1.5 );
		double fx = dx / r3;
		double fy = dy / r3;
		axA += fx * mB;
		ayA += fy * mB;
		axB -= fx * mA;
		ayB -= fy * mA;
	}
}