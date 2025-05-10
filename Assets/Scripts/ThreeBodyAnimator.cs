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
			string[] cols = lines[ i ].Split( ',' );
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
		if( _i >= times.Count )
			return;

		body0.transform.position = new( pos0[ _i ].x, pos0[ _i ].y, 0 );
		body1.transform.position = new( pos1[ _i ].x, pos1[ _i ].y, 0 );
		body2.transform.position = new( pos2[ _i ].x, pos2[ _i ].y, 0 );

		_i++;
	}
}