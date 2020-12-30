﻿using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

[RequireComponent(typeof(Animator), typeof(Rigidbody2D))]
public class MovementController : MonoBehaviour {
	private Vector2 steering;
	private Vector2 waypoint;
	private Vector2 destination;
	private Vector2 start;

	private PathFollow pathFollow;
	private DynamicBuffer<PathPosition> path;
	
	public bool havePath { private set; get; }

	[SerializeField] private float maxSpeed = 10f;
	[SerializeField] private float maxAcceleration = 10f;
	[SerializeField] private float approachDistance = 10f;

	private Rigidbody2D rb;
	private SpriteRenderer spriteRenderer;
	private Animator animator;
	private bool hasVerticalAnimation;
	private bool hasHorizontalAnimation;
	private bool facingRight;

	/// <summary>
	/// Controls what type of movement is used. Players use manual.
	/// </summary>
	public bool automatedMovement = true;

	private Vector2 velocity;

	// [SerializeField] private ConvertedEntityHolder convertedEntityHolder = null;
	private Entity entity;
	private EntityManager entityManager;

	private void Start() {
		waypoint = Vector2.zero;

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

		// Kick off pathfinding fetch loop
		if (automatedMovement) {
			InvokeRepeating("GetUpdatedPath", 0f, 1f);
		}

	}

	private void Update() {
		// Update animation
		if (hasHorizontalAnimation)
			animator.SetFloat("Horizontal", rb.velocity.x);
		if (hasVerticalAnimation)
			animator.SetFloat("Vertical", rb.velocity.y);

		if (automatedMovement && havePath) {
			if (Vector2.Distance(waypoint, transform.position) < 0.25f) {
				GetNextWaypoint();
			} else {
				// Vector2 move = (waypoint - (Vector2)transform.position).normalized;
				// SetMoveDirection(move.x, move.y);
				Seek(waypoint);
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
		
		//If input is moving the player right and player is facing left
		if (spriteRenderer != null) {
			if (rb.velocity.x > 0 && !facingRight) {
				Flip();
			} else if (rb.velocity.x < 0 && facingRight) {
				Flip();
			}
		}
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

	public void Seek(Vector2 target) {
		Vector2 desired = SteeringBehaviors.Arrive(target, transform.position, maxSpeed, approachDistance);
		steering = Vector2.ClampMagnitude(desired - rb.velocity, maxAcceleration);

		Debug.DrawRay(transform.position, steering, Color.blue);
		Debug.DrawRay(transform.position, rb.velocity, Color.red);
		Debug.DrawRay(transform.position, desired, Color.green);
	}

	public void CreateEntityForPathfinding() {
		entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
		entity = entityManager.CreateEntity();
		entityManager.AddComponentData(entity, new PathFollow { pathIndex = -1 });
		entityManager.AddBuffer<PathPosition>(entity);
	}


	/// <summary>
	/// Sets a position to travel to. Entity will pathfind its way there.
	/// </summary>
	/// <param name="destination">Vector2 destination to travel to.</param>
	public void Travel(Vector2 destination) {
		this.destination = destination;
		PathfindingGrid.Instance.RequestPath(entity, transform.position, destination);
	}

	private void GetUpdatedPath() {
		if (destination != Vector2.zero) {
			// Debug.Log("Requesting new path and fetching.");
			PathfindingGrid.Instance.RequestPath(entity, transform.position, destination);
			pathFollow = entityManager.GetComponentData<PathFollow>(entity);
			if (pathFollow.pathIndex != -1) {
				// Debug.Log("Got path!");
				havePath = true;
				GetNextWaypoint();
			}
		}
	}

	private void GetNextWaypoint() {
		// Get index of waypoint from PathFollow struct
		int index = pathFollow.pathIndex;
		// Debug.LogFormat("Getting waypoint {0}", index);

		pathFollow = entityManager.GetComponentData<PathFollow>(entity);
		path = entityManager.GetBuffer<PathPosition>(entity);

		// If we have a waypoint left
		if (index >= 0) {
			if (path.Length > 0) {
				Debug.DrawLine(
					transform.position,
					PathfindingGrid.Instance.GetWorldPosition(path[path.Length - 1].position),
					Color.green,
					1f
				);
			}
			for (int i = 1; i < path.Length; i++) {
				Debug.DrawLine(
					PathfindingGrid.Instance.GetWorldPosition(path[i - 1].position),
					PathfindingGrid.Instance.GetWorldPosition(path[i].position),
					Color.green,
					1f
				);
			}

			// Get the waypoint
			int2 pathPosition = path[index].position;
			waypoint = PathfindingGrid.Instance.GetWorldPosition(pathPosition);
			// Debug.LogFormat("Setting waypoint to {0}", waypoint);

			// Update the index for the next time
			pathFollow.pathIndex--;
			entityManager.SetComponentData(entity, pathFollow);
		}
		else {
			Debug.Log("Done traveling.");
			havePath = false;
			// waypoint = Vector2.zero;
			// SetMoveDirection(0, 0);
			// destination = Vector2.zero;
		}
	}

	private void Flip() {
		facingRight = !facingRight;
		spriteRenderer.flipX = !spriteRenderer.flipX;
	}
}

