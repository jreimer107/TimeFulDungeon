using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Animator), typeof(Rigidbody2D))]
public class MovementController : MonoBehaviour {
	// Movement configuration
	[SerializeField] private float maxSpeed = 10f;
	[SerializeField] private float maxAcceleration = 10f;
	[SerializeField] private float approachDistance = 10f;
	[SerializeField] private bool drawSteering = false;
	[SerializeField] private bool drawVelocity = false;
	[SerializeField] private bool drawDesired = false;
	[SerializeField] private bool freeze = false;

	
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
	private Vector2 spawn;

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

	public AnimationCurve plot = new AnimationCurve();

	private void Start() {
		waypoint = Vector2.zero;
		spawn = transform.position;

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
			animator.SetFloat("Horizontal", Mathf.Abs(rb.velocity.x));
		if (hasVerticalAnimation)
			animator.SetFloat("Vertical", Mathf.Abs(rb.velocity.y));

		
		if (automatedMovement) {
			AutomatedMovement();
			if (destination != default(Vector2) && path.Count != 0) {
				GetUpdatedPath();
			}
		}

		//If input is moving the player right and player is facing left
		if (spriteRenderer != null) {
			if (rb.velocity.x < 0 && !facingRight) {
				Flip();
			} else if (rb.velocity.x > 0 && facingRight) {
				Flip();
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
		// Gizmos.DrawSphere(waypoint, 0.2f);
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

	float lastAdjust = 0f;
	Vector2 wanderInterest = Vector2.right;
	public void AutomatedMovement() {
		// Steer towards our target
		// Vector2 desired = SteeringBehaviors.Arrive(target, transform.position, maxSpeed, approachDistance);
		// Vector2 target = SteeringBehaviors.Follow(path.ToArray(), rb.velocity, transform.position, maxSpeed, approachDistance);
		// if (target != Vector2.zero && target != waypoint) {
		// 	// Debug.Log("Target:" + target);
		// 	contextSteering.RemoveInterest(waypoint);
		// 	contextSteering.AddInterest(target);
		// 	waypoint = target;
		// }
		contextSteering.ClearDangers();
		contextSteering.ClearInterests();
		// Vector2 wanderInterest = SteeringBehaviors.Wander(rb.velocity, transform.position, spawn, ref lastAdjust);
		SteeringBehaviors.Wander(rb.velocity, transform.position, spawn, ref wanderInterest);
		contextSteering.AddInterest((Vector2) transform.position + wanderInterest);
		// contextSteering.AddInterest(transform.position + Player.instance.transform.position);
		Vector2 contextResult = contextSteering.direction;
		// Vector2 desired = SteeringBehaviors.Seek(contextResult, transform.position, maxSpeed);
		Vector2 desired = contextResult * maxSpeed;
		// Debug.Log("Desired: " + desired);
		steering = Vector2.ClampMagnitude(desired - rb.velocity, maxAcceleration);
		if (freeze) {
			steering = Vector2.zero;
		}
		if (drawSteering) {
			Debug.DrawRay(transform.position, steering, Color.blue);
		}
		if (drawVelocity) {
			Debug.DrawRay(transform.position, rb.velocity, Color.red);
		}
		if (drawDesired) {
			Debug.DrawRay(transform.position, desired, Color.green);
		}
	}


	/// <summary>
	/// Sets a position to travel to. Entity will pathfind its way there.
	/// </summary>
	/// <param name="destination">Vector2 destination to travel to.</param>
	public void Travel(Vector2 destination) {
		this.destination = destination;
		this.start = transform.position;
		GetUpdatedPath();
		// Debug.Log("Destination: " + destination);
		// Debug.Log("Path:");
		// foreach(Vector2 waypoint in path) {
		// 	Debug.Log(waypoint);
		// }
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

