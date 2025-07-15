#region

using System.Collections.Generic;
using UnityEngine;

#endregion

public class PlanarThreeBodyController : MonoBehaviour
{
	public LineRenderer[] lineRenderers;

	private void Start()
	{
		double[] y0 =
		{
			-1, 0, 0.0931957583, 0.5754064016, 1, 0, 0.0931957583, 0.5754064016, 0, 0, -0.1863915166, -1.1508128032,
		};

		List<double> positions = ThreeBody.Simulate( y0, 137.7843111275, new[] { 1d, 1d, 1d }, 5 ).position;
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
				lineRenderers[ j ].SetPosition( i, new Vector3( x, y, 0 ) );
			}
		}
	}
}