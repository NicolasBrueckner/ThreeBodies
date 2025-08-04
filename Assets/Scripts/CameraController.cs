#region

using System.Linq;
using UnityEngine;

#endregion

[ RequireComponent( typeof( Camera ) ) ]
public class CameraController : MonoBehaviour
{
	private Camera _cam;

	private void Start()
	{
		_cam = GetComponent<Camera>();
		RuntimeEventManager.OrbitCalculated += OnOrbitCalculated;
	}

	private void OnOrbitCalculated( CalculationResult result )
	{
		float maxValue = result.positions.AsParallel().Max();
		_cam.orthographicSize = maxValue;
	}
}