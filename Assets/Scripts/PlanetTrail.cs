#region

using UnityEngine;

#endregion

[ RequireComponent( typeof( LineRenderer ) ) ]
public class PlanetTrail : MonoBehaviour
{
	public static float LineWidth = 0.2f;


	public int bodyIndex;

	private CalculationResult _currentResult;
	private LineRenderer _lineRenderer;
	private int _currentStep;
	private int _totalSteps;

	private void Awake()
	{
		RuntimeEventManager.OrbitCalculated += OnOrbitCalculated;
		RuntimeEventManager.TrailSliderValueChanged += OnTrailSliderChanged;
		RuntimeEventManager.StepUpdated += OnStepUpdated;
	}

	private void Start()
	{
		_lineRenderer = GetComponent<LineRenderer>();
		_lineRenderer.widthMultiplier = LineWidth;
	}

	private void Update()
	{
		//if( _lineRenderer.positionCount < 0.9f * _totalSteps )
		UpdateTrail();
		/*else
		{

		}*/
	}

	private void UpdateTrail()
	{
		int length = _lineRenderer.positionCount;

		for( int i = 0; i < length; i++ )
		{
			int index = _currentStep - i < 0
				            ? _totalSteps - 1 + ( _currentStep - i )
				            : _currentStep - i;
			_lineRenderer.SetPosition( i, _currentResult.GetPositionAtStep( index, bodyIndex ) );
		}
	}

	private void OnOrbitCalculated( CalculationResult obj )
	{
		_currentResult = obj;
		_totalSteps = _currentResult.times.Length;
	}

	private void OnStepUpdated( int value ) => _currentStep = value;

	private void OnTrailSliderChanged( float value ) =>
		_lineRenderer.positionCount = ( int )( _totalSteps * value );
}