#region

using UnityEngine;

#endregion

public class PlanetController : MonoBehaviour
{
	public int bodyIndex;
	public Color planetColor;

	private Gradient _trailGradient;
	private LineRenderer _orbitRenderer;

	private void Start()
	{
		RuntimeEventManager.OrbitCalculated += OnOrbitCalculated;
		RuntimeEventManager.OrbitToggleChanged += OnOrbitToggleChanged;

		MeshRenderer planetRenderer = GetComponent<MeshRenderer>();
		TrailRenderer trailRenderer = GetComponent<TrailRenderer>();
		_orbitRenderer = GetComponent<LineRenderer>();

		planetRenderer.material.color = planetColor;
		trailRenderer.colorGradient = InitializeGradient( planetColor, a2: 0.0f );
		_orbitRenderer.colorGradient = InitializeGradient( planetColor );
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
}