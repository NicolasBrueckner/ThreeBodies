#region

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

#endregion

public class OrbitInformationLoader : MonoBehaviour
{
	public event Action NewFileLoaded;
	public event Action<OrbitInformation> NewOrbitInfoLoaded;

	private JObject _currentJObjectRoot;
	private List<string> _currentOrbitNames;

	public void LoadNewFile( string fileName )
	{
		TextAsset jsonFile = Resources.Load<TextAsset>( fileName );

		try
		{
			string jsonText = jsonFile.text;
			_currentJObjectRoot = JObject.Parse( jsonText );
			_currentOrbitNames = _currentJObjectRoot.Properties().Select( p => p.Name ).ToList();

			NewFileLoaded?.Invoke();
			GetInformationByIndex( 0 );
		}
		catch( Exception e )
		{
			Console.WriteLine( e );
			throw;
		}
	}

	public List<string> GetKeys() => _currentOrbitNames;

	public OrbitInformation GetInformationByIndex( int orbitIndex )
	{
		string key = _currentOrbitNames[ orbitIndex ];
		return ConvertToOrbitInformation( key, ( JObject )_currentJObjectRoot[ key ] );
	}

	public OrbitInformation GetInformationByName( string orbitName ) =>
		ConvertToOrbitInformation( orbitName, ( JObject )_currentJObjectRoot[ orbitName ] );

	private OrbitInformation ConvertToOrbitInformation( string key, JObject obj )
	{
		try
		{
			OrbitInformation info = new()
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

			NewOrbitInfoLoaded?.Invoke( info );
			return info;
		}
		catch( Exception e )
		{
			Console.WriteLine( e );
			throw;
		}
	}

	public void Unload()
	{
		_currentJObjectRoot.RemoveAll();
		_currentJObjectRoot = null;
		_currentOrbitNames = null;
	}
}