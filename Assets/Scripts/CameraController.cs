#region

using UnityEngine;

#endregion

[ RequireComponent( typeof( Camera ) ) ]
public class CameraController : MonoBehaviour
{
	public float scale = 1f;
	public float horizontalPadding;
	public float verticalPadding;
	private Camera _cam;

	private void Start()
	{
		_cam = GetComponent<Camera>();
		RuntimeEventManager.OrbitCalculated += OnOrbitCalculated;
	}

	private void OnOrbitCalculated( CalculationResult result )
	{
		float minX = float.MaxValue;
		float minY = float.MaxValue;
		float maxX = float.MinValue;
		float maxY = float.MinValue;

		float[] positions = result.positions;

		for( int i = 0; i < positions.Length; i += 2 )
		{
			float x = positions[ i ];
			float y = positions[ i + 1 ];

			if( x < minX ) minX = x;
			if( x > maxX ) maxX = x;
			if( y < minY ) minY = y;
			if( y > maxY ) maxY = y;
		}

		minX -= horizontalPadding;
		maxX += horizontalPadding;
		minY -= verticalPadding;
		maxY += verticalPadding;

		Vector2 center = new( ( minX + maxX ) * 0.5f, ( minY + maxY ) * 0.5f );
		Vector2 size = new( maxX - minX, maxY - minY );

		_cam.transform.position = new( center.x, center.y, _cam.transform.position.z );

		float aspect = Screen.width / ( float )Screen.height;
		float verticalSize = size.y * 0.5f;
		float horizontalSize = size.x * 0.5f / aspect;

		_cam.orthographicSize = Mathf.Max( verticalSize, horizontalSize ) * scale;
		RuntimeEventManager.InvokeCameraOrthographicSizeChanged( _cam.orthographicSize );
	}
}