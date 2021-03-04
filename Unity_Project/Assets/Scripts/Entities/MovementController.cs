using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Animator), typeof(Rigidbody2D))]
public class MovementController : MonoBehaviour {
	#region Configuration fields
	[SerializeField] private float maxSpeed = 10f;
	[SerializeField] private float maxAcceleration = 10f;
	[SerializeField] private float approachDistance = 10f;
	[SerializeField] private bool drawSteering = false;
	[SerializeField] private bool drawVelocity = false;
	[SerializeField] private bool drawDesired = false;
	[SerializeField] private bool freeze = false;
	[SerializeField] private bool drawPath = false;
	#endregion
	
	#region  Private fields
	// Controls where we are going, used by both modes
	private Vector2 desiredDirection;
	private bool wandering = false;

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
	private bool facingRight { get => spriteRenderer.flipX; set => spriteRenderer.flipX = value; }

	// Steering module
	private ContextSteering contextSteering;
	private WanderModule wanderModule;
	#endregion

	#region Public fields	
	/// <summary>
	/// Controls what type of movement is used. Players use manual.
	/// </summary>
	public bool automatedMovement = true;
	#endregion

	#region Public Methods
	/// <summary>
	/// Sets a direction to travel towards.
	/// </summary>
	/// <param name="desiredDirection">Direction from self to travel towards.</param>
	public void SetDesiredDirection(Vector2 desiredDirection) {
		this.desiredDirection = desiredDirection;
	}

	/// <summary>
	/// Sets a position to travel to. Entity will pathfind its way there.
	/// </summary>
	/// <param name="destination">Vector2 destination to travel to.</param>
	public void Travel(Vector2 destination) {
		this.destination = destination;
		this.start = transform.position;
		GetUpdatedPath();
	}
	#endregion

	#region Unity methods
	private void Start() {
		waypoint = Vector2.zero;
		spawn = transform.position;

		contextSteering = GetComponent<ContextSteering>();
		wanderModule = GetComponent<WanderModule>();

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
		if (spriteRenderer && desiredDirection.x != 0 && (desiredDirection.x < 0 ^ facingRight)) {
			Flip();
		}

		// Adjust our velocity
		Move(Time.deltaTime);
	}
	#endregion

	#region Private methods
	private void Move(float deltaTime) {
		float calculatedMaxSpeed = maxSpeed;
		if (wandering && !wanderModule.outsideWanderLimit) {
			calculatedMaxSpeed = 0.5f;
		}
		Vector2 desiredVelocity = desiredDirection * calculatedMaxSpeed;
        Vector2 acceleration = Vector2.ClampMagnitude(desiredVelocity - rb.velocity, maxAcceleration) / (rb.mass / 2);
		
		if (freeze) {
			desiredDirection = Vector2.zero;
		}
		if (drawSteering) {
			Debug.DrawRay(transform.position, acceleration, Color.blue);
		}
		if (drawVelocity) {
			Debug.DrawRay(transform.position, rb.velocity, Color.red);
		}
		if (drawDesired) {
			Debug.DrawRay(transform.position, desiredDirection, Color.green);
		}
		
		rb.velocity = Vector2.ClampMagnitude(rb.velocity + acceleration * deltaTime, maxSpeed);
	}

	private void AutomatedMovement() {
		// Steer towards our target
		// Vector2 desired = SteeringBehaviors.Arrive(target, transform.position, maxSpeed, approachDistance);
		// Vector2 target = SteeringBehaviors.Follow(path.ToArray(), rb.velocity, transform.position, maxSpeed, approachDistance);
		// if (target != Vector2.zero && target != waypoint) {
		// 	// Debug.Log("Target:" + target);
		// 	contextSteering.RemoveInterest(waypoint);
		// 	contextSteering.AddInterest(target);
		// 	waypoint = target;
		// }
		float calculatedMaxSpeed = maxSpeed;
		contextSteering.AddInterest(Player.instance.transform);
		// if (Vector2.Distance((Vector2) Player.instance.transform.position, transform.position) < 10) {
		// }
		// else {
		// 	contextSteering.RemoveInterest(Player.instance.transform);
		// }
		wandering = contextSteering.HasNoInterestsOrDangers();
		if (wandering) {
			wanderModule.enabled = true;
			contextSteering.defaultDirection = wanderModule.WanderDirection;
		}
		else {
			wanderModule.enabled = false;
			contextSteering.defaultDirection = Vector2.zero;
		}
		
		desiredDirection = contextSteering.direction;
	}

	private void GetUpdatedPath() {
		if (destination != default(Vector2)) {
			path = PathfindingGrid.Instance.RequestPath(transform.position, destination);
		}
	}

	private void Flip() {
		facingRight = !facingRight;
	}
	#endregion
}

