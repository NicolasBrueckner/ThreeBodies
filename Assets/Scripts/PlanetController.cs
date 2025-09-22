#region

using UnityEngine;

#endregion

public class PlanetController : MonoBehaviour
{
	public Color planetColor;

	private Gradient _trailGradient;

	private void Start()
	{
		MeshRenderer planetRenderer = GetComponent<MeshRenderer>();
		TrailRenderer trailRenderer = GetComponent<TrailRenderer>();

		planetRenderer.material.color = planetColor;
		trailRenderer.colorGradient = InitializeGradient();
	}

	private Gradient InitializeGradient()
	{
		Gradient gradient = new();
		GradientColorKey[] colorKeys = new GradientColorKey[ 2 ];
		colorKeys[ 0 ] = new GradientColorKey( planetColor, 0.0f );
		colorKeys[ 1 ] = new GradientColorKey( planetColor, 1.0f );

		GradientAlphaKey[] alphaKeys = new GradientAlphaKey[ 2 ];
		alphaKeys[ 0 ] = new GradientAlphaKey( 1.0f, 0.0f );
		alphaKeys[ 1 ] = new GradientAlphaKey( 0.0f, 1.0f );

		gradient.SetKeys( colorKeys, alphaKeys );
		return gradient;
	}
}