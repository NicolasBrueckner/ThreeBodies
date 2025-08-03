#region

using UnityEngine;
using Random = UnityEngine.Random;

#endregion

public class BodyRotation : MonoBehaviour
{
	public Vector2 rotationSpeedRange;

	private Vector3 _rotationAxis;
	private float _rotationSpeed;

	private void Start()
	{
		_rotationAxis = Random.onUnitSphere;
		_rotationSpeed = Random.Range( rotationSpeedRange.x, rotationSpeedRange.y );
	}

	private void FixedUpdate()
	{
		transform.RotateAround( transform.position, _rotationAxis, _rotationSpeed );
	}
}