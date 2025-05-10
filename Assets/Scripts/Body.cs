using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class Body : MonoBehaviour
{
	[Range(1f, 100f)]
	public float g = 1f;
	
	public bool randomizeInitialVelocity;
	public Vector3 initialVelocity;
	
	private Rigidbody _rb; 
	private static readonly List<Body> Bodies=new();
	
	public List<Body> exposedBodies;

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
		
		_rb = GetComponent<Rigidbody>();
		if( randomizeInitialVelocity )
		{
			Vector3 force = GetRandomStartingVelocity();
			_rb.AddForce( force, ForceMode.Impulse );
		}
		else
			_rb.AddForce( initialVelocity, ForceMode.Impulse );
	}

	
	private void FixedUpdate()
	{
		foreach( Body body in Bodies.Where( body => body != this ) )
		{
			Vector3 force = GetForceFromBody( body );
			_rb.AddForce( force, ForceMode.Force );
		}
	}

	
	private Vector3 GetRandomStartingVelocity()
	{
		Vector2 direction = UnityEngine.Random.insideUnitSphere;
		return new( direction.x, 0, direction.y );
	}

	private Vector3 GetForceFromBody( Body body )
	{
		Vector3 direction = body.transform.position - transform.position;
		float distanceSquared = direction.sqrMagnitude + 0.01f;
		float forceMagnitude = g * _rb.mass * body._rb.mass / distanceSquared;
		return direction.normalized * forceMagnitude;
	}
}
