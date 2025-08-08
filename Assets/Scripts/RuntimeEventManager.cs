#region

using System;

#endregion

public static class RuntimeEventManager
{
	public static event Action FileLoaded;
	public static event Action<OrbitInformation> OrbitInfoLoaded;
	public static event Action<CalculationResult> OrbitCalculated;
	public static event Action<float> TrailSliderValueChanged;
	public static event Action<int> StepUpdated;

	public static void InvokeFileLoaded()                                => FileLoaded?.Invoke();
	public static void InvokeOrbitInfoLoaded( OrbitInformation info )    => OrbitInfoLoaded?.Invoke( info );
	public static void InvokeOrbitCalculated( CalculationResult result ) => OrbitCalculated?.Invoke( result );
	public static void InvokeTrailSliderValueChanged( float value )      => TrailSliderValueChanged?.Invoke( value );
	public static void InvokeStepUpdated( int step )                     => StepUpdated?.Invoke( step );
}