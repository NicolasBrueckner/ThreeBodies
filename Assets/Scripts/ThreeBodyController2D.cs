#region

using UnityEngine;

#endregion

public class ThreeBodyController2D : MonoBehaviour
{
	[ Header( "Bodies (Assign 3 Rigidbodies)" ) ]
	public Rigidbody2D[] bodies = new Rigidbody2D[ 3 ];

	public InitialValues2DCollection collection;

	private void Start()
	{
		for( int i = 0; i < 3; i++ )
		{
			bodies[ i ].position = collection.groups[ 0 ].sequences[ 0 ].orbits[ 1 ].initialValues.p[ i ];
			bodies[ i ].linearVelocity = collection.groups[ 0 ].sequences[ 0 ].orbits[ 1 ].initialValues.v[ i ];
			bodies[ i ].mass = collection.groups[ 0 ].sequences[ 0 ].orbits[ 1 ].initialValues.m[ i ];
			bodies[ i ].gravityScale = 0f;
		}
	}

	private void FixedUpdate()
	{
		AddForceToBodies();
	}

	private void AddForceToBodies()
	{
		for( int i = 0; i < bodies.Length; i++ )
		{
			Vector2 netForce = Vector2.zero;

			for( int j = 0; j < bodies.Length; j++ )
			{
				if( i == j )
					continue;

				Vector2 dir = bodies[ j ].position - bodies[ i ].position;
				float distSqr = dir.sqrMagnitude + 1e-6f;
				float forceMag = bodies[ i ].mass * bodies[ j ].mass / distSqr;
				netForce += dir.normalized * forceMag;
			}

			bodies[ i ].AddForce( netForce );
		}
	}
}