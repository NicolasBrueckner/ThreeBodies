#region

using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

#endregion

public class OrbitCalculator
{
	private class State
	{
		public readonly double t;
		public readonly double[] y = new double[ 12 ];
		public double? dt;

		public State( double tInit, double[] yInit, double[] vInit )
		{
			t = tInit;
			yInit.CopyTo( y, 0 );
			vInit.CopyTo( y, 6 );
		}
	}

	private struct InitialConditions
	{
		public float m0, m1, m2;
		public double tEnd;
		public double[] y0;
	}

	private const double Tolerance = 1e-9;
	private const double Epsilon = math.EPSILON;
	private const double MaxIncreaseFactor = 10;
	private const double MaxDecreaseFactor = 10;

	public List<float> times;
	public List<float> positions;

	private float MinMag( float a, float b ) => a > 0 ? math.min( a, b ) : math.max( a, b );
	private float MaxMag( float a, float b ) => a > 0 ? math.max( a, b ) : math.min( a, b );

	private readonly double[] scratch = new double[ 1024 ];
	private double[] k1tmp, k2tmp, k3tmp, k4tmp, k5tmp, k6tmp, w;

	private void Ode45( State state )
	{
		double i, tmp, k1, k2, k3, k4, k5, k6;
		double tolerance2 = Tolerance * Tolerance;
		double maxIncreaseFactor = 10;
	}

	private double[] PlanarThreeBodyDerivative( double[] yp, double[] y, float t )
	{
		double dx, dy, r3;
		float m0 = 0, m1 = 0, m2 = 0; //TODO: initial conditions

		// d(position)/dt = velocity
		yp[ 0 ] = y[ 2 ];
		yp[ 1 ] = y[ 3 ];
		yp[ 4 ] = y[ 6 ];
		yp[ 5 ] = y[ 7 ];
		yp[ 8 ] = y[ 10 ];
		yp[ 9 ] = y[ 11 ];

		// pairwise gravitational attractions
		dx = y[ 4 ] - y[ 0 ];
		dy = y[ 5 ] - y[ 1 ];
		r3 = math.pow( dx * dx + dy * dy, 1.5f );
		dx /= r3;
		dy /= r3;
		yp[ 2 ] = dx * m1;
		yp[ 3 ] = dy * m1;
		yp[ 6 ] = -dx * m0;
		yp[ 7 ] = -dy * m0;

		dx = y[ 8 ] - y[ 0 ];
		dy = y[ 9 ] - y[ 1 ];
		r3 = math.pow( dx * dx + dy * dy, 1.5 );
		dx /= r3;
		dy /= r3;
		yp[ 2 ] += dx * m2;
		yp[ 3 ] += dy * m2;
		yp[ 10 ] = -dx * m0;
		yp[ 11 ] = -dy * m0;

		dx = y[ 8 ] - y[ 4 ];
		dy = y[ 9 ] - y[ 5 ];
		r3 = math.pow( dx * dx + dy * dy, 1.5 );
		dx /= r3;
		dy /= r3;
		yp[ 6 ] += dx * m2;
		yp[ 7 ] += dy * m2;
		yp[ 10 ] -= dx * m1;
		yp[ 11 ] -= dy * m1;

		return yp;
	}

	private void Trajectory( InitialConditions init )
	{
		const double tolerance = 1e-9;
		double tLimit = init.tEnd;

		State state = new( init.tEnd, init.y0.Take( 6 ).ToArray(), init.y0.Skip( 6 ).ToArray() );
		times.Clear();
		positions.Clear();

		StoreStep( 0, state.y );

		int step = 0;
		while( step++ < 1e6 && true )
			//Ode45();
			StoreStep( state.t, state.y );
	}

	private void StoreStep( double t, double[] y )
	{
		times.Add( ( float )t );
		positions.AddRange( y.Take( 6 ).Select( x => ( float )x ) );
	}
}