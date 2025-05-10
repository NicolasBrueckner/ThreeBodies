#region

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

#endregion

public class ThreeBodyLoader : MonoBehaviour
{
	[ Tooltip( "Name of the CSV file in StreamingAssets" ) ]
	public string fileName = "positions.csv";

	[ HideInInspector ] public List<float> times = new();
	[ HideInInspector ] public List<Vector2> body0 = new();
	[ HideInInspector ] public List<Vector2> body1 = new();
	[ HideInInspector ] public List<Vector2> body2 = new();

	private void Awake()
	{
		string path = Path.Combine( Application.streamingAssetsPath, fileName );
		if( !File.Exists( path ) )
		{
			Debug.LogError( $"[ThreeBodyLoader] File not found: {path}" );
			return;
		}

		string[] lines = File.ReadAllLines( path );
		// skip header
		for( int i = 1; i < lines.Length; i++ )
		{
			string[] cols = lines[ i ].Split( ',' );
			// parse all columns with invariant culture (decimal point)
			float t = float.Parse( cols[ 0 ], CultureInfo.InvariantCulture );
			float x0 = float.Parse( cols[ 1 ], CultureInfo.InvariantCulture );
			float y0 = float.Parse( cols[ 2 ], CultureInfo.InvariantCulture );
			float x1 = float.Parse( cols[ 3 ], CultureInfo.InvariantCulture );
			float y1 = float.Parse( cols[ 4 ], CultureInfo.InvariantCulture );
			float x2 = float.Parse( cols[ 5 ], CultureInfo.InvariantCulture );
			float y2 = float.Parse( cols[ 6 ], CultureInfo.InvariantCulture );

			times.Add( t );
			body0.Add( new Vector2( x0, y0 ) );
			body1.Add( new Vector2( x1, y1 ) );
			body2.Add( new Vector2( x2, y2 ) );
		}

		Debug.Log( $"[ThreeBodyLoader] Loaded {times.Count} steps from '{fileName}'." );
	}
}