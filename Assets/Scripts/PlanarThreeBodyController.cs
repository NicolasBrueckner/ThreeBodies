#region

using System.Collections.Generic;
using UnityEngine;

#endregion

public class PlanarThreeBodyController : MonoBehaviour
{
	public string sequenceName, orbitName;

	public LineRenderer lineRenderer0, lineRenderer1, lineRenderer2;

	private void Start()
	{
		OrbitInformationBinaryLoader.FillOrbitsDict( sequenceName );

		foreach( KeyValuePair<string, int> kvp in OrbitInformationBinaryLoader.currentOrbitsDict )
			Debug.Log( $"name: {kvp.Key}, offset: {kvp.Value}" );

		OrbitInformationBinaryLoader.LoadOrbitInfo( orbitName, sequenceName );
		int arraySize = OrbitInformationBinaryLoader._positionsBuffer.Length * sizeof( float );
		Debug.Log( $"length: {OrbitInformationBinaryLoader._positionsBuffer.Length}, size: {arraySize}" );
		/*lineRenderer0.positionCount = arraySize / 3;
		lineRenderer1.positionCount = arraySize / 3;
		lineRenderer2.positionCount = arraySize / 3;

		for( int i = 0; i + 6 < arraySize; i += 6 )
		{
			lineRenderer0.SetPosition( i / 6,
				new Vector3( OrbitInformationBinaryLoader._positionsBuffer[ i ],
					OrbitInformationBinaryLoader._positionsBuffer[ i + 1 ], 0 ) );
			lineRenderer1.SetPosition( i / 6,
				new Vector3( OrbitInformationBinaryLoader._positionsBuffer[ i + 2 ],
					OrbitInformationBinaryLoader._positionsBuffer[ i + 3 ], 0 ) );
			lineRenderer2.SetPosition( i / 6,
				new Vector3( OrbitInformationBinaryLoader._positionsBuffer[ i + 4 ],
					OrbitInformationBinaryLoader._positionsBuffer[ i + 5 ], 0 ) );
		}*/
	}

	public float pointSize = 0.05f;

	private void OnDrawGizmos()
	{
		float[] buffer = OrbitInformationBinaryLoader._positionsBuffer;
		int length = buffer.Length;

		for( int i = 0; i + 5 < length; i += 6 )
		{
			Vector3 p0 = new( buffer[ i ], buffer[ i + 1 ], 0 );
			Vector3 p1 = new( buffer[ i + 2 ], buffer[ i + 3 ], 0 );
			Vector3 p2 = new( buffer[ i + 4 ], buffer[ i + 5 ], 0 );

			Gizmos.color = Color.red;
			Gizmos.DrawSphere( p0, pointSize );

			Gizmos.color = Color.green;
			Gizmos.DrawSphere( p1, pointSize );

			Gizmos.color = Color.blue;
			Gizmos.DrawSphere( p2, pointSize );
		}
	}
}