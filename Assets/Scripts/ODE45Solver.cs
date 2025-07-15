#region

using System;

#endregion

public class Ode45Solver
{
	private const double Epsilon = 1e-14;

	private static double[] _scratch = new double[ 1024 ];
	private static double[] _k1TMP, _k2TMP, _k3TMP, _k4TMP, _k5TMP, _k6TMP, _w;

	private static double MinMag( double a, double b ) => a > 0 ? Math.Min( a, b ) : Math.Max( a, b );
	private static double MaxMag( double a, double b ) => a > 0 ? Math.Max( a, b ) : Math.Min( a, b );

	private static int NextPow2( int v )
	{
		v += v == 0 ? 1 : 0;
		v--;
		v |= v >> 1;
		v |= v >> 2;
		v |= v >> 4;
		v |= v >> 8;
		v |= v >> 16;
		return v + 1;
	}

	public class State
	{
		public double t;
		public double dt = 1;
		public double[] y;
		public double dtPrevious;
		public bool limitReached;
	}

	public class Options
	{
		public double tolerance = 1e-8;
		public const double MaxIncreaseFactor = 10;
		public const double MaxDecreaseFactor = 10;
		public double tLimit = double.PositiveInfinity;
	}

	public static State Ode45( State input, Func<double[], double[], double, double[]> f, Options options )
	{
		int n = input.y.Length;
		double[] y = input.y;
		double dt = input.dt;
		double t = input.t;

		if( n * 7 > _scratch.Length )
			_scratch = new double[ NextPow2( n * 7 ) ];

		if( _w == null || _w.Length != n )
		{
			_w = new double[ n ];
			_k1TMP = new double[ n ];
			_k2TMP = new double[ n ];
			_k3TMP = new double[ n ];
			_k4TMP = new double[ n ];
			_k5TMP = new double[ n ];
			_k6TMP = new double[ n ];
		}

		double tolerance2 = options.tolerance * options.tolerance;
		double thisDt = dt;
		const double safetyFactor = 0.9;
		double tLimit = options.tLimit;

		f( _k1TMP, y, t );
		double[] k1 = _k1TMP;
		double[] k2 = null, k3 = null, k4 = null, k5 = null, k6 = null;

		int trialStep = 0;
		double error2 = 0;

		while( trialStep++ < 1000 )
		{
			thisDt = MinMag( thisDt, tLimit - t );

			for( int i = 0; i < n; i++ )
				_w[ i ] = y[ i ] + thisDt * 0.2 * k1[ i ];

			f( _k2TMP, _w, t + thisDt * 0.2 );
			k2 = _k2TMP;

			for( int i = 0; i < n; i++ )
				_w[ i ] = y[ i ] + thisDt * ( 0.075 * k1[ i ] + 0.225 * k2[ i ] );

			f( _k3TMP, _w, t + thisDt * 0.3 );
			k3 = _k3TMP;

			for( int i = 0; i < n; i++ )
				_w[ i ] = y[ i ] + thisDt * ( 0.3 * k1[ i ] - 0.9 * k2[ i ] + 1.2 * k3[ i ] );

			f( _k4TMP, _w, t + thisDt * 0.6 );
			k4 = _k4TMP;

			for( int i = 0; i < n; i++ )
			{
				_w[ i ] = y[ i ] + thisDt * ( -0.203703703703703703 * k1[ i ] + 2.5 * k2[ i ] -
				                              2.592592592592592592 * k3[ i ] + 1.296296296296296296 * k4[ i ] );
			}

			f( _k5TMP, _w, t + thisDt );
			k5 = _k5TMP;

			for( int i = 0; i < n; i++ )
			{
				_w[ i ] = y[ i ] + thisDt * ( 0.029495804398148148 * k1[ i ] + 0.341796875 * k2[ i ] +
				                              0.041594328703703703 * k3[ i ] + 0.400345413773148148 * k4[ i ] +
				                              0.061767578125 * k5[ i ] );
			}

			f( _k6TMP, _w, t + thisDt * 0.875 );
			k6 = _k6TMP;

			error2 = 0;
			for( int i = 0; i < n; i++ )
			{
				double d = thisDt * (
					                    0.004293774801587301 * k1[ i ]
					                    - 0.018668586093857832 * k3[ i ]
					                    + 0.034155026830808080 * k4[ i ]
					                    + 0.019321986607142857 * k5[ i ]
					                    - 0.039102202145680406 * k6[ i ]
				                    );
				error2 += d * d;
			}

			if( error2 < tolerance2 || thisDt == 0.0 ) break;

			double nextDt = safetyFactor * thisDt * Math.Pow( tolerance2 / error2, 0.1 );
			thisDt = MaxMag( thisDt / Options.MaxDecreaseFactor, nextDt );
		}

		for( int i = 0; i < n; i++ )
		{
			y[ i ] += thisDt * (
				                   0.097883597883597883 * k1[ i ] +
				                   0.402576489533011272 * k3[ i ] +
				                   0.210437710437710437 * k4[ i ] +
				                   0.289102202145680406 * k6[ i ]
			                   );
		}

		input.dtPrevious = thisDt;
		input.t += thisDt;

		double nextDtFinal = safetyFactor * thisDt * Math.Pow( tolerance2 / error2, 0.125 );
		input.dt = MaxMag( thisDt / Options.MaxDecreaseFactor,
			MinMag( thisDt * Options.MaxIncreaseFactor, nextDtFinal ) );
		input.limitReached = double.IsFinite( tLimit ) && Math.Abs( ( input.t - options.tLimit ) / thisDt ) < Epsilon;

		return input;
	}
}