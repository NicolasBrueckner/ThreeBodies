#region

using System;

#endregion

public static class RuntimeEventManager
{
	public static event Action FileLoaded;
	public static event Action<OrbitInformation> OrbitInfoLoaded;

	public static void InvokeFileLoaded()                             => FileLoaded?.Invoke();
	public static void InvokeOrbitInfoLoaded( OrbitInformation info ) => OrbitInfoLoaded?.Invoke( info );
}