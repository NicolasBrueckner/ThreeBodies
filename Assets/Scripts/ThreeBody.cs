#region

using System;
using System.Collections.Generic;

#endregion

public static class ThreeBody
{
	private static double[] _masses;

	private static double[] Derivative( double[] yp, double[] y, double t )
	{
		Array.Clear( yp, 0, yp.Length );

		yp[ 0 ] = y[ 2 ];
		yp[ 1 ] = y[ 3 ];
		yp[ 4 ] = y[ 6 ];
		yp[ 5 ] = y[ 7 ];
		yp[ 8 ] = y[ 10 ];
		yp[ 9 ] = y[ 11 ];

		ComputePair( yp, y, 0, 4, 2, 6, _masses[ 1 ], _masses[ 0 ] );
		ComputePair( yp, y, 0, 8, 2, 10, _masses[ 2 ], _masses[ 0 ] );
		ComputePair( yp, y, 4, 8, 6, 10, _masses[ 2 ], _masses[ 1 ] );

		return yp;
	}

	private static void ComputePair( double[] yp, double[] y, int iA, int iB, int viA, int viB, double mB, double mA )
	{
		double dx = y[ iB ] - y[ iA ];
		double dy = y[ iB + 1 ] - y[ iA + 1 ];
		double r3 = Math.Pow( dx * dx + dy * dy, 1.5 );
		dx /= r3;
		dy /= r3;
		yp[ viA ] += dx * mB;
		yp[ viA + 1 ] += dy * mB;
		yp[ viB ] -= dx * mA;
		yp[ viB + 1 ] -= dy * mA;
	}

	public static (List<double> t, List<double> position) Simulate( double[] initialY, double tEnd, double[] masses,
		int sampleRate,
		double tolerance = 1e-8 )
	{
		Ode45Solver.Options config = new() { tolerance = tolerance, tLimit = tEnd };
		_masses = masses;
		int i = 0;

		List<double> resultT = new();
		List<double> resultPos = new();
		Ode45Solver.State state = new() { t = 0, dt = 1, y = ( double[] )initialY.Clone() };

		StoreStep( state.t, state.y );
		int step = 0;
		while( step++ < 1e6 && !state.limitReached )
		{
			Ode45Solver.Ode45( state, Derivative, config );

			if( i % sampleRate == 0 )
				StoreStep( state.t, state.y );
			i++;
		}

		return ( resultT, resultPos );

		void StoreStep( double t, double[] y )
		{
			resultT.Add( t );
			resultPos.AddRange( new[] { y[ 0 ], y[ 1 ], y[ 4 ], y[ 5 ], y[ 8 ], y[ 9 ] } );
		}
	}
}