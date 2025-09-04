#region

using System;
using System.Linq;
using UnityEngine;

#endregion

[ RequireComponent( typeof( LineRenderer ) ) ]
public class PlanetTrail : MonoBehaviour
{
	public static float TrailWidth = 0.15f;
	public static float LineWidth = 0.01f;

	[ Header( "Config" ) ]
	public int bodyIndex;


	private enum Mode
	{
		None,
		Trail,
		Orbit,
	}

	private Mode _mode;
	private const float _orbitStart = 0.98f;

	private TrailRenderer _trail;
	private LineRenderer _line;
	private float _orbitTime;
	private float _sliderValue;

	private Vector3[] _positions;

	private void Awake()
	{
		_trail = GetComponent<TrailRenderer>();
		_line = GetComponent<LineRenderer>();

		_trail.widthMultiplier = TrailWidth;
		_line.widthMultiplier = LineWidth;

		RuntimeEventManager.OrbitCalculated += OnOrbitCalculated;
		RuntimeEventManager.TrailSliderValueChanged += OnTrailSliderChanged;
	}

	private void OnDestroy()
	{
		RuntimeEventManager.OrbitCalculated -= OnOrbitCalculated;
		RuntimeEventManager.TrailSliderValueChanged -= OnTrailSliderChanged;
	}

	private void OnOrbitCalculated( CalculationResult result )
	{
		_orbitTime = result.times.Last();

		_trail.Clear();
		_trail.enabled = false;
		_trail.time = 0;

		_trail.transform.position += Vector3.one * Mathf.Epsilon;

		_trail.enabled = true;
		_trail.time = _orbitTime;

		_line.positionCount = result.times.Length;
		_line.SetPositions( result.GetPositionsOfBody( bodyIndex ) );
	}

	private void OnTrailSliderChanged( float value )
	{
		_sliderValue = Mathf.Clamp01( value );

		if( _mode == Mode.Trail )
			_trail.time = _orbitTime * _sliderValue;

		switch( value )
		{
			case < _orbitStart:
				SetMode( Mode.Trail );
				break;
			case >= _orbitStart:
				SetMode( Mode.Orbit );
				break;
		}
	}

	private void SetMode( Mode mode )
	{
		if( mode == _mode )
			return;

		_mode = mode;

		switch( mode )
		{
			case Mode.Trail:
				_trail.widthMultiplier = TrailWidth;
				_line.widthMultiplier = 0;
				break;
			case Mode.Orbit:
				_trail.widthMultiplier = 0;
				_line.widthMultiplier = LineWidth;
				break;
			case Mode.None:
				break;
			default:
				throw new ArgumentOutOfRangeException( nameof( mode ), mode, null );
		}
	}
}