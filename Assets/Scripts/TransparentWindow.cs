#region

using System;
using System.Runtime.InteropServices;
using UnityEngine;

#endregion

public class TransparentWindow : MonoBehaviour
{
	[ DllImport( "user32.dll", SetLastError = true ) ]
	private static extern IntPtr FindWindow( string lpClassName, string lpWindowName );

	[ DllImport( "user32.dll" ) ]
	private static extern IntPtr SetParent( IntPtr hWndChild, IntPtr hWndNewParent );

	[ DllImport( "user32.dll" ) ]
	private static extern IntPtr SendMessageTimeout( IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam,
		SendMessageTimeoutFlags fuFlags, uint uTimeout, out IntPtr lpdwResult );

	[ DllImport( "user32.dll" ) ]
	private static extern bool EnumWindows( EnumWindowsProc lpEnumFunc, IntPtr lParam );

	[ DllImport( "user32.dll" ) ]
	private static extern IntPtr GetActiveWindow();

	[ DllImport( "user32.dll" ) ]
	private static extern bool SetWindowPos( IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy,
		uint uFlags );

	private static readonly IntPtr HWND_BOTTOM = new( 1 );

	private const uint SWP_NOMOVE = 0x0002;
	private const uint SWP_NOSIZE = 0x0001;
	private const uint SWP_SHOWWINDOW = 0x0040;


	private delegate bool EnumWindowsProc( IntPtr hWnd, IntPtr lParam );

	private const int WM_SPAWN_WORKERW = 0x052C;
	private IntPtr _hWnd;

	private void Start()
	{
#if !UNITY_EDITOR
		IntPtr _hWnd = GetActiveWindow();
		AttachToWorker( _hWnd );
#endif
	}

	private void OnApplicationQuit()
	{
#if !UNITY_EDITOR
		if( _hWnd != IntPtr.Zero )
			SetParent( _hWnd, IntPtr.Zero );
#endif
	}

	private static void AttachToWorker( IntPtr hWnd )
	{
		//Force creation of WorkerW behind everything
		IntPtr progman = FindWindow( "Progman", null );
		IntPtr result;
		SendMessageTimeout( progman, WM_SPAWN_WORKERW, IntPtr.Zero, IntPtr.Zero, SendMessageTimeoutFlags.SMTO_NORMAL,
			1000, out result );

		IntPtr workerW = IntPtr.Zero;

		EnumWindows( ( tophandle, topparamhandle ) =>
		{
			IntPtr p = FindWindowEx( tophandle, IntPtr.Zero, "SHELLDLL_DefView", null );
			if( p != IntPtr.Zero )
				workerW = FindWindowEx( IntPtr.Zero, tophandle, "WorkerW", null );

			return true;
		}, IntPtr.Zero );

		if( workerW != IntPtr.Zero )
		{
			SetParent( hWnd, workerW );

			SetWindowPos( hWnd, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW );
		}
	}

	[ DllImport( "user32.dll", SetLastError = true ) ]
	private static extern IntPtr FindWindowEx( IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass,
		string lpszWindow );

	[ Flags ]
	private enum SendMessageTimeoutFlags
	{
		SMTO_NORMAL = 0x0000,
	}
}