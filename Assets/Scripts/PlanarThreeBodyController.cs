using UnityEngine;

public class PlanarThreeBodyController : MonoBehaviour
{
	private void Start()
	{
		OrbitInformationBinaryLoader.LoadSequenceDict( "additional-Broucke" );

		Debug.Log( "shouldve loaded sequence" );
		foreach ( var kvp in OrbitInformationBinaryLoader.currentOrbitsDict )
			Debug.Log( $"name: {kvp.Key}, offset: {kvp.Value}" );
	}
}
