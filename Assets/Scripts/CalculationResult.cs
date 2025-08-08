#region

using Unity.Mathematics;
using UnityEngine;

#endregion

public class CalculationResult
{
	public float[] times;
	public float[] positions;

	public Vector3 GetPositionAtStep( int step, int bodyIndex )
	{
		step = math.clamp( step, 0, times.Length - 1 );
		bodyIndex = math.clamp( bodyIndex, 0, 2 );

		int index = step * 6 + bodyIndex * 2;
		return new Vector3( positions[ index ], positions[ index + 1 ], 0 );
	}
}