#region

using System;

#endregion

public static class RuntimeEventManager
{
	public static event Action FileLoaded;
	public static event Action<OrbitInformation> OrbitInfoLoaded;
	public static event Action<CalculationResult> OrbitCalculated;
	public static event Action<bool> OrbitToggleChanged;
	public static event Action<int> StepUpdated;
	public static event Action<float> CameraOrthographicSizeChanged;

	public static void InvokeFileLoaded()                                => FileLoaded?.Invoke();
	public static void InvokeOrbitInfoLoaded( OrbitInformation info )    => OrbitInfoLoaded?.Invoke( info );
	public static void InvokeOrbitCalculated( CalculationResult result ) => OrbitCalculated?.Invoke( result );
	public static void InvokeOrbitToggleChanged( bool value )            => OrbitToggleChanged?.Invoke( value );
	public static void InvokeStepUpdated( int step )                     => StepUpdated?.Invoke( step );

	public static void InvokeCameraOrthographicSizeChanged( float value ) =>
		CameraOrthographicSizeChanged?.Invoke( value );
}