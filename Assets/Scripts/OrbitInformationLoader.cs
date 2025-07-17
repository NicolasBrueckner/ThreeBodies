#region

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

#endregion

public class OrbitInformationLoader
{
	private JObject _root;
	private List<string> _keys;

	public OrbitInformationLoader( string jsonPath )
	{
		TextAsset jsonFile = Resources.Load<TextAsset>( jsonPath );
		if( jsonFile == null )
		{
			Debug.LogError( $"Orbit JSON file not found at Resources/{jsonPath}.json" );
			return;
		}

		string jsonText = jsonFile.text;
		_root = JObject.Parse( jsonText );
		_keys = _root.Properties().Select( p => p.Name ).ToList();
	}

	public List<string> GetKeys() => _keys;

	public OrbitInformation GetInformationByIndex( int index )
	{
		string key = _keys[ index ];
		return ConvertToOrbitInformation( key, ( JObject )_root[ key ] );
	}

	public OrbitInformation GetInformationByName( string name ) =>
		ConvertToOrbitInformation( name, ( JObject )_root[ name ] );

	private static OrbitInformation ConvertToOrbitInformation( string key, JObject obj )
	{
		return new OrbitInformation
		{
			orbitName = key,
			year = obj[ "year" ]?.ToString(),
			freeGroupElement = obj[ "G" ]?.ToString(),
			period = obj[ "T" ]?.Value<float>() ?? -1,
			energy = obj[ "E" ]?.Value<float>() ?? -1,
			angularMomentum = obj[ "L" ]?.Value<float>() ?? -1,
			initialPositions = obj[ "x" ]
			                   ?.Select( arr => new Vector2( arr[ 0 ].Value<float>(), arr[ 1 ].Value<float>() ) )
			                   .ToArray(),
			initialVelocities = obj[ "v" ]
			                    ?.Select( arr => new Vector2( arr[ 0 ].Value<float>(), arr[ 1 ].Value<float>() ) )
			                    .ToArray(),
			masses = obj[ "m" ]?.Select( m => m.Value<double>() ).ToArray() ?? new[] { 1.0, 1.0, 1.0 },
		};
	}

	public void Unload()
	{
		_root.RemoveAll();
		_root = null;
		_keys = null;
	}
}