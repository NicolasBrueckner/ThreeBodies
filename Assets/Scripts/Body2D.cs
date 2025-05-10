using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Body2D : MonoBehaviour
{
	[Range(1f, 100f)]
	public float g = 1f;
	
	public bool randomizeInitialVelocity;
	public Vector3 initialVelocity;
	
	private Rigidbody2D _rb2D; 
	private static readonly List<Body2D> Bodies=new();
	
	public List<Body2D> exposedBodies;

	private void OnEnable()
	{
		Bodies.Add(this);
	}

	private void OnDisable()
	{
		Bodies.Remove( this );
	}

	private void Start()
	{
		exposedBodies = Bodies;
		
		_rb2D = GetComponent<Rigidbody2D>();
		_rb2D.linearVelocity = randomizeInitialVelocity ? GetRandomStartingVelocity() : initialVelocity;
	}

	
	private void FixedUpdate()
	{
		foreach( Body2D body2D in Bodies.Where( body => body != this ) )
		{
			Vector3 force = GetForceFromBody( body2D );
			_rb2D.AddForce( force, ForceMode2D.Force );
		}
	}
	
	private static Vector2 GetRandomStartingVelocity() => Random.insideUnitCircle;

	private Vector2 GetForceFromBody( Body2D body )
	{
		Vector2 direction = body.transform.position - transform.position;
		float distanceSquared = direction.sqrMagnitude + 0.01f;
		float forceMagnitude = g * ((_rb2D.mass * body._rb2D.mass) / distanceSquared);
		return direction.normalized * forceMagnitude;
	}
}
