#region

using UnityEngine;

#endregion

public class PlanarThreeBodyController : MonoBehaviour
{
	public OrbitInformationLoader loader;
	public int sampleRate = 1;
	public float scale = 1f;
	public GameObject[] bodies;
	public LineRenderer[] lineRenderers;

	private CalculationResult _currentResult;
	private int _currentStep;

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
		_currentStep = 0;

		DrawLines(); //currently only for debugging
	}

	private void MoveBodies()
	{
		bodies[ 0 ].transform.position = _currentResult.GetPositionAtStep( _currentStep, 0 ) * scale;
		bodies[ 1 ].transform.position = _currentResult.GetPositionAtStep( _currentStep, 1 ) * scale;
		bodies[ 2 ].transform.position = _currentResult.GetPositionAtStep( _currentStep, 2 ) * scale;

		_currentStep = ( _currentStep + 1 ) % _currentResult.times.Length;
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