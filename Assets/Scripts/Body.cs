#region

using System.Collections;
using UnityEngine;

#endregion

public class Body : MonoBehaviour
{
	public int bodyIndex;
	public Color planetColor;
	public int lineRendererPixelWidth;

	private Gradient _trailGradient;
	private Transform _cachedTransform;
	private LineRenderer _orbitRenderer;
	private TrailRenderer _trailRenderer;

	private const float UniformMaxScale = 5.0f;
	private const float UniformMinScale = 0.1f;

	private void Start()
	{
		RuntimeEventManager.OrbitCalculated += OnOrbitCalculated;
		RuntimeEventManager.OrbitInfoLoaded += OnOrbitInfoLoaded;
		RuntimeEventManager.OrbitToggleChanged += OnOrbitToggleChanged;
		RuntimeEventManager.CameraOrthographicSizeChanged += OnCameraOrthographicSizeChanged;

		MeshRenderer planetRenderer = GetComponent<MeshRenderer>();
		_trailRenderer = GetComponent<TrailRenderer>();
		_cachedTransform = transform;
		_orbitRenderer = GetComponent<LineRenderer>();

		planetRenderer.material.color = planetColor;
		_trailRenderer.colorGradient = InitializeGradient( planetColor, a2: 0.0f );
		_orbitRenderer.colorGradient = InitializeGradient( planetColor );
	}

	private void OnDestroy()
	{
		RuntimeEventManager.OrbitCalculated -= OnOrbitCalculated;
		RuntimeEventManager.OrbitToggleChanged -= OnOrbitToggleChanged;
		RuntimeEventManager.OrbitToggleChanged -= OnOrbitToggleChanged;
		RuntimeEventManager.CameraOrthographicSizeChanged -= OnCameraOrthographicSizeChanged;
	}

	private void OnOrbitToggleChanged( bool value )
	{
		_orbitRenderer.enabled = value;
	}

	private void OnOrbitCalculated( CalculationResult obj )
	{
		_orbitRenderer.positionCount = obj.times.Length;
		_orbitRenderer.SetPositions( obj.GetPositionsOfBody( bodyIndex ) );
	}

	private void OnOrbitInfoLoaded( OrbitInformation obj )
	{
		StartCoroutine( ResetTrail() );
	}

	private void OnCameraOrthographicSizeChanged( float value )
	{
		float pixelPerUnit = Screen.height / ( value * 2f );
		float worldUnitWidth = lineRendererPixelWidth / pixelPerUnit;

		_orbitRenderer.widthMultiplier = worldUnitWidth;
	}

	private IEnumerator ResetTrail()
	{
		_trailRenderer.enabled = false;
		yield return new WaitForEndOfFrame();
		_trailRenderer.Clear();
		_trailRenderer.enabled = true;
	}

	private static Gradient InitializeGradient( Color c1, Color? c2 = null, float a1 = 1f, float? a2 = null )
	{
		Color finalC2 = c2 ?? c1;
		float finalA2 = a2 ?? a1;

		return new Gradient
		{
			colorKeys = new[] { new GradientColorKey( c1, 0.0f ), new GradientColorKey( finalC2, 1.0f ) },
			alphaKeys = new[] { new GradientAlphaKey( a1, 0.0f ), new GradientAlphaKey( finalA2, 1.0f ) },
		};
	}

	public void ChangeSize( float max, float alpha )
	{
		float inverseAlpha = Mathf.InverseLerp( 0.0f, max, alpha );
		float newScale = Mathf.Lerp( UniformMinScale, UniformMaxScale, inverseAlpha );

		_cachedTransform.localScale = Vector3.one * newScale;
	}
}