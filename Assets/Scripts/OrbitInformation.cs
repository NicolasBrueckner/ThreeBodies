using System.Runtime.InteropServices;
using UnityEngine;

[StructLayout( LayoutKind.Sequential )]
public struct OrbitInformation
{
	public string orbitName;
	public string year;
	public string freeGroupElement;
	public float period;
	public float energy;
	public float angularMomentum;
	public Vector2[] initialPositions;
	public Vector2[] initialVelocities;
	public Vector2[] allPositions;
}
