#region

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

#endregion

public class ThreeBodyAnimator : MonoBehaviour
{
	[ Header( "Assign in Inspector" ) ]
	public GameObject body0, body1, body2;

	public float playbackSpeed = 1.0f;
	public string csvFileName = "trajectory.csv";

	private List<float> times;
	private List<Vector2> pos0, pos1, pos2;

	private int _i;

	private void Start()
	{
		LoadCsv();
	}

	private void LoadCsv()
	{
		// CSV in StreamingAssets folder
		string path = Path.Combine( Application.streamingAssetsPath, csvFileName );
		string[] lines = File.ReadAllLines( path );

		times = new List<float>();
		pos0 = new List<Vector2>();
		pos1 = new List<Vector2>();
		pos2 = new List<Vector2>();

		for( int i = 1; i < lines.Length; i++ ) // skip header
		{
			string[] cols = lines[ i ].Split( ';' );
			float t = float.Parse( cols[ 0 ], CultureInfo.InvariantCulture );
			float x0 = float.Parse( cols[ 1 ], CultureInfo.InvariantCulture );
			float y0 = float.Parse( cols[ 2 ], CultureInfo.InvariantCulture );
			float x1 = float.Parse( cols[ 3 ], CultureInfo.InvariantCulture );
			float y1 = float.Parse( cols[ 4 ], CultureInfo.InvariantCulture );
			float x2 = float.Parse( cols[ 5 ], CultureInfo.InvariantCulture );
			float y2 = float.Parse( cols[ 6 ], CultureInfo.InvariantCulture );

			times.Add( t );
			pos0.Add( new Vector2( x0, y0 ) );
			pos1.Add( new Vector2( x1, y1 ) );
			pos2.Add( new Vector2( x2, y2 ) );

			Debug.Log( $"times: {times.Count}, pos0: {pos0.Count}, pos1: {pos1.Count}, pos2: {pos2.Count}" );
		}
	}

	private void Update()
	{
		/*if( !Input.GetMouseButtonDown( 0 ) )
			return;*/

		// 1) Compute your “virtual” time within the orbit:
		float playTime = Time.time * playbackSpeed % times[ times.Count - 1 ];

		// 2) Find i0 via binary‐search and i1 = i0+1
		int i0 = times.BinarySearch( playTime );
		if( i0 < 0 ) i0 = ~i0 - 1;
		i0 = Mathf.Clamp( i0, 0, times.Count - 2 );
		int i1 = i0 + 1;

		// 3) Compute alpha
		float t0 = times[ i0 ], t1 = times[ i1 ];
		float alpha = ( playTime - t0 ) / ( t1 - t0 );

		// 4) Interpolate each body
		Vector2 p0 = Vector2.Lerp( pos0[ i0 ], pos0[ i1 ], alpha );
		Vector2 p1 = Vector2.Lerp( pos1[ i0 ], pos1[ i1 ], alpha );
		Vector2 p2 = Vector2.Lerp( pos2[ i0 ], pos2[ i1 ], alpha );

		// 5) Assign transforms
		body0.transform.position = new Vector3( p0.x, p0.y, 0 ) * 10;
		body1.transform.position = new Vector3( p1.x, p1.y, 0 ) * 10;
		body2.transform.position = new Vector3( p2.x, p2.y, 0 ) * 10;
	}
}