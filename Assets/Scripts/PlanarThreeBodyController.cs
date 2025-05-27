#region

using System.Collections.Generic;
using UnityEngine;

#endregion

public class PlanarThreeBodyController : MonoBehaviour
{
	private void Start()
	{
		OrbitInformationBinaryLoader.FillOrbitsDict( "IC_ic" );

		Debug.Log( "shouldve loaded sequence" );
		foreach( KeyValuePair<string, int> kvp in OrbitInformationBinaryLoader.currentOrbitsDict )
			Debug.Log( $"name: {kvp.Key}, offset: {kvp.Value}" );

		/*OrbitInformationBinaryLoader.LoadOrbitInfo( "II.C.300 i.c.", "IC_ic" );
		int arraySize = OrbitInformationBinaryLoader._positionsBuffer.Length * sizeof( float );
		Debug.Log( $"length: {OrbitInformationBinaryLoader._positionsBuffer.Length}, size: {arraySize}" );*/
	}
}