#region

using Unity.Mathematics;
using UnityEngine;

#endregion

public class CalculationResult
{
	public float[] times;
	public float[] positions;

	public Vector3[] GetPositionsOfBody( int bodyIndex )
	{
		Vector3[] result = new Vector3[ times.Length ];

		for( int i = 0; i < times.Length; i++ )
		{
			float x = positions[ 6 * i + 2 * bodyIndex ];
			float y = positions[ 6 * i + 2 * bodyIndex + 1 ];
			result[ i ] = new( x, y, 0 );
		}

		return result;
	}

	public Vector3 GetPositionAtStep( int step, int bodyIndex )
	{
		step = math.clamp( step, 0, times.Length - 1 );
		bodyIndex = math.clamp( bodyIndex, 0, 2 );

		int index = step * 6 + bodyIndex * 2;
		return new Vector3( positions[ index ], positions[ index + 1 ], 0 );
	}
}