﻿using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using System.Collections.Generic;

[RequireComponent(typeof(Animator), typeof(Rigidbody2D))]
public class MovementController : MonoBehaviour {
	// Movement configuration
	[SerializeField] private float maxSpeed = 10f;
	[SerializeField] private float maxAcceleration = 10f;
	[SerializeField] private float approachDistance = 10f;
	
	/// <summary>
	/// Controls what type of movement is used. Players use manual.
	/// </summary>
	public bool automatedMovement = true;
	
	// Controls where we are going, used by both modes
	private Vector2 steering;
	
	// Manual pathfinding reference
	private Vector2 velocity;

	// Automatic pathfinding variables
	private Vector2 waypoint;
	private Vector2 destination;
	private Vector2 start;

	private List<Vector2> path;

	// Physics and animation
	private Rigidbody2D rb;
	private SpriteRenderer spriteRenderer;
	private Animator animator;
	private bool hasVerticalAnimation;
	private bool hasHorizontalAnimation;
	private bool facingRight;

	// Steering module
	private ContextSteering contextSteering;

	private void Start() {
		waypoint = Vector2.zero;

		contextSteering = GetComponent<ContextSteering>();

		rb = GetComponent<Rigidbody2D>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		animator = GetComponent<Animator>();
		foreach (AnimatorControllerParameter parameter in animator.parameters) {
			if (parameter.name == "Horizontal") {
				hasHorizontalAnimation = true;
			} else if (parameter.name == "Vertical") {
				hasVerticalAnimation = true;
			}
			if (hasHorizontalAnimation && hasVerticalAnimation) {
				break;
			}
		}
	}

	private void Update() {
		// Update animation
		if (hasHorizontalAnimation)
			animator.SetFloat("Horizontal", rb.velocity.x);
		if (hasVerticalAnimation)
			animator.SetFloat("Vertical", rb.velocity.y);

		
		if (automatedMovement && destination != default(Vector2) && path.Count != 0) {
			GetUpdatedPath();
			AutomatedMovement(waypoint);
		}

		//If input is moving the player right and player is facing left
		if (spriteRenderer != null) {
			if (rb.velocity.x > 0 && !facingRight) {
				Flip();
			} else if (rb.velocity.x < 0 && facingRight) {
				Flip();
			}
		}

		if (automatedMovement && path != null) {
			for (int i = 0; i < path.Count - 1; i++) {
				Debug.DrawLine(path[i], path[i+1], Color.magenta);
			}
		}
	}

	private void FixedUpdate() {
		if (automatedMovement) {
			SteeringBehaviors.Steer(rb, steering, maxSpeed);
		}
		else {
			ManualMovement();
		}
	}

	private void OnDrawGizmos() {
		Gizmos.DrawSphere(waypoint, 0.2f);
	}

	public void SetManualMoveDirection(float horizontal, float vertical) {
		steering = new Vector2(horizontal, vertical) * maxSpeed;
	}

	private void ManualMovement() {
		//Move player by finding target velocity
		Vector2 targetVelocity = steering * Time.fixedDeltaTime * 10f;
		//And then smoothing it out and applying it to the character
		rb.velocity = Vector2.SmoothDamp(rb.velocity, targetVelocity, ref velocity, 1 / maxAcceleration);
	}

	public void AutomatedMovement(Vector2 _) {
		// Steer towards our target
		// Vector2 desired = SteeringBehaviors.Arrive(target, transform.position, maxSpeed, approachDistance);
		Vector2 target = SteeringBehaviors.Follow(path.ToArray(), rb.velocity, transform.position, maxSpeed, approachDistance);
		if (target != Vector2.zero && target != waypoint) {
			// Debug.Log("Target:" + target);
			contextSteering.RemoveInterest(waypoint);
			contextSteering.AddInterest(target);
			waypoint = target;
		}
		Vector2 contextResult = contextSteering.direction;
		// Vector2 desired = SteeringBehaviors.Seek(contextResult, transform.position, maxSpeed);
		Vector2 desired = contextResult * maxSpeed;
		// Debug.Log("Desired: " + desired);
		steering = Vector2.ClampMagnitude(desired - rb.velocity, maxAcceleration);

		Debug.DrawRay(transform.position, steering, Color.blue);
		Debug.DrawRay(transform.position, rb.velocity, Color.red);
		Debug.DrawRay(transform.position, desired, Color.green);
	}


	/// <summary>
	/// Sets a position to travel to. Entity will pathfind its way there.
	/// </summary>
	/// <param name="destination">Vector2 destination to travel to.</param>
	public void Travel(Vector2 destination) {
		this.destination = destination;
		this.start = transform.position;
		GetUpdatedPath();
		Debug.Log("Destination: " + destination);
		Debug.Log("Path:");
		foreach(Vector2 waypoint in path) {
			Debug.Log(waypoint);
		}
	}

	private void GetUpdatedPath() {
		if (destination != default(Vector2)) {
			path = PathfindingGrid.Instance.RequestPath(transform.position, destination);
			// currentWaypoint = 0;
		}
	}

	private void Flip() {
		facingRight = !facingRight;
		spriteRenderer.flipX = !spriteRenderer.flipX;
	}
}

