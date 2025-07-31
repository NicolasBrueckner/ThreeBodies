#region

using System.Linq;
using UnityEngine;

#endregion

public class ThreeBodyController : MonoBehaviour
{
	public OrbitInformationLoader loader;
	public int sampleRate = 1;
	public float scale = 1f;
	public float playbackSpeed = 1f;
	public GameObject[] bodies;
	public LineRenderer[] lineRenderers;

	private CalculationResult _currentResult;
	private float _simulationTime;
	private float[] _times;
	private float[] _positions;

	private void Start()
	{
		RuntimeEventManager.OrbitInfoLoaded += OnOrbitInfoLoaded;
	}

	private void FixedUpdate()
	{
		MoveBodies();
	}

	private void OnOrbitInfoLoaded( OrbitInformation info )
	{
		ThreeBodyOrbitCalculator calculator = new();

		double[] y0 = new double[ 12 ];
		for( int i = 0; i < 3; i++ )
		{
			y0[ i * 4 + 0 ] = info.initialPositions[ i ].x;
			y0[ i * 4 + 1 ] = info.initialPositions[ i ].y;
			y0[ i * 4 + 2 ] = info.initialVelocities[ i ].x;
			y0[ i * 4 + 3 ] = info.initialVelocities[ i ].y;
		}

		_currentResult = calculator.Simulate( y0, info.period, info.masses, sampleRate );
		_times = _currentResult.times;

		DrawLines(); //currently only for debugging
	}

	private void MoveBodies()
	{
		_simulationTime = ( _simulationTime + Time.deltaTime * playbackSpeed ) % _times.Last();

		int step = FindStepIndex( _simulationTime );

		float t0 = _times[ step ];
		float t1 = _times[ step + 1 ];
		float alpha = Mathf.Approximately( t0, t1 ) ? 0f : Mathf.InverseLerp( t0, t1, _simulationTime );

		for( int i = 0; i < 3; i++ )
		{
			Vector2 p0 = _currentResult.GetPositionAtStep( step, i );
			Vector2 p1 = _currentResult.GetPositionAtStep( step + 1, i );

			Vector2 interpolated = Vector2.Lerp( p0, p1, alpha );
			bodies[ i ].transform.position = interpolated * scale;
		}
	}

	private int FindStepIndex( float time )
	{
		int low = 0;
		int high = _times.Length - 2;

		while( low <= high )
		{
			int mid = ( low + high ) / 2;
			if( _times[ mid ] <= time && time <= _times[ mid + 1 ] )
				return mid;
			if( time < _times[ mid ] )
				high = mid - 1;
			else
				low = mid + 1;
		}

		return _times.Length - 2;
	}

	private void DrawLines()
	{
		int totalFrames = _currentResult.positions.Length / 6;
		lineRenderers[ 0 ].positionCount = totalFrames;
		lineRenderers[ 1 ].positionCount = totalFrames;
		lineRenderers[ 2 ].positionCount = totalFrames;

		for( int i = 0; i < totalFrames; i++ )
		{
			int baseIndex = i * 6;

			for( int j = 0; j < 3; j++ )
			{
				int index = baseIndex + j * 2;
				float x = _currentResult.positions[ index ];
				float y = _currentResult.positions[ index + 1 ];
				lineRenderers[ j ].SetPosition( i, new Vector3( x, y, 0 ) * scale );
			}
		}
	}
}