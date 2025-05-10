#region

using System;
using UnityEngine;

#endregion

[ Serializable ]
public struct ThreeBody2DInitialValues
{
	public Vector2[] p;
	public Vector2[] v;
	public float[] m;
}

[ Serializable ]
public struct ThreeBody2DOrbit
{
	public string name;
	public ThreeBody2DInitialValues initialValues;
}

[ Serializable ]
public struct ThreeBody2DSequence
{
	public string name;
	public ThreeBody2DOrbit[] orbits;
}

[ Serializable ]
public struct ThreeBody2DGroup
{
	public string name;
	public ThreeBody2DSequence[] sequences;
}

[ CreateAssetMenu( menuName = "ThreeBody/InitialValues2D" ) ]
public class InitialValues2DCollection : ScriptableObject
{
	public ThreeBody2DGroup[] groups;
}