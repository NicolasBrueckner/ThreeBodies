#region

using UnityEngine;

#endregion

public class OrbRotate : MonoBehaviour
{
	public float rotationSpeed = 100f;

	private void Update()
	{
		transform.Rotate( Vector3.up, rotationSpeed * Time.deltaTime );
	}
}