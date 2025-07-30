#region

using UnityEngine;

#endregion

public class PlanarThreeBodyController : MonoBehaviour
{
	public OrbitInformationLoader loader;
	public int sampleRate = 1;
	public float scale = 1f;
	public LineRenderer[] lineRenderers;

	private CalculationResult currentResult;

	private void Start()
	{
		RuntimeEventManager.OrbitInfoLoaded += OnOrbitInfoLoaded;
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

		currentResult = calculator.Simulate( y0, info.period, info.masses, sampleRate );

		DrawLines(); //currently only for debugging
	}

	private void DrawLines()
	{
		int totalFrames = currentResult.positions.Length / 6;
		lineRenderers[ 0 ].positionCount = totalFrames;
		lineRenderers[ 1 ].positionCount = totalFrames;
		lineRenderers[ 2 ].positionCount = totalFrames;

		for( int i = 0; i < totalFrames; i++ )
		{
			int baseIndex = i * 6;

			for( int j = 0; j < 3; j++ )
			{
				int index = baseIndex + j * 2;
				float x = currentResult.positions[ index ] * 3;
				float y = currentResult.positions[ index + 1 ] * 3;
				lineRenderers[ j ].SetPosition( i, new Vector3( x, y, 0 ) * scale );
			}
		}
	}
}