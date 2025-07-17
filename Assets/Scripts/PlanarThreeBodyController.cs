#region

using System.Collections.Generic;
using UnityEngine;

#endregion

public class PlanarThreeBodyController : MonoBehaviour
{
	public int sampleRate = 1;
	public float scale = 1f;
	public string fileName;
	public string orbitName;
	public LineRenderer[] lineRenderers;

	private void Start()
	{
		OrbitInformationLoader loader = new( fileName );
		OrbitInformation info = loader.GetInformationByName( orbitName );

		ThreeBodyOrbitCalculator calculator = new();

		double[] y0 =
		{
			info.initialPositions[ 0 ].x, info.initialPositions[ 0 ].y, info.initialVelocities[ 0 ].x,
			info.initialVelocities[ 0 ].y, info.initialPositions[ 1 ].x, info.initialPositions[ 1 ].y,
			info.initialVelocities[ 1 ].x, info.initialVelocities[ 1 ].y, info.initialPositions[ 2 ].x,
			info.initialPositions[ 2 ].y, info.initialVelocities[ 2 ].x, info.initialVelocities[ 2 ].y,
		};

		List<double> positions = calculator.Simulate( y0, info.period, info.masses, sampleRate ).position;
		Debug.Log( $"number of positions: {positions.Count}" );

		int totalFrames = positions.Count / 6;
		lineRenderers[ 0 ].positionCount = totalFrames;
		lineRenderers[ 1 ].positionCount = totalFrames;
		lineRenderers[ 2 ].positionCount = totalFrames;

		for( int i = 0; i < totalFrames; i++ )
		{
			int baseIndex = i * 6;

			for( int j = 0; j < 3; j++ )
			{
				int index = baseIndex + j * 2;
				float x = ( float )positions[ index ] * 3;
				float y = ( float )positions[ index + 1 ] * 3;
				lineRenderers[ j ].SetPosition( i, new Vector3( x, y, 0 ) * scale );
			}
		}

		if( Camera.main )
			Camera.main.orthographicSize *= scale;
	}
}