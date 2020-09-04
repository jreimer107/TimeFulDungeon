using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

[RequireComponent(typeof(Animator), typeof(Rigidbody2D))]
public class MovementController : MonoBehaviour {
	private float horizontalMove;
	private float verticalMove;
	private Vector2 waypoint;
	private Vector2 destination;
	private Vector2 start;

	private PathFollow pathFollow;
	private DynamicBuffer<PathPosition> path;
	
	public bool havePath { private set; get; }

	private Transform simpleFollowTarget;
	public bool movementEnabled = true;

	[SerializeField] private float speed = 20f;
	[Range(0, .3f)] [SerializeField] private float MovementSmoothing = .05f;

	private new Rigidbody2D rigidbody;
	private SpriteRenderer spriteRenderer;
	private Animator animator;
	private bool hasVerticalAnimation;
	private bool hasHorizontalAnimation;
	private bool FacingRight;

	private Vector2 velocity;

	// [SerializeField] private ConvertedEntityHolder convertedEntityHolder = null;
	private Entity entity;
	private EntityManager entityManager;

	private void Awake() {
		horizontalMove = 0;
		verticalMove = 0;
		velocity = Vector3.zero;
		waypoint = Vector2.zero;

		rigidbody = GetComponent<Rigidbody2D>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		animator = GetComponent<Animator>();
		if (animator.runtimeAnimatorController) {
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

		// Kick off pathfinding fetch loop
		if (entity != null) {
			InvokeRepeating("GetUpdatedPath", 0f, 1f);
		}

	}

	private void Update() {
		// Update animation
		if (hasHorizontalAnimation)
			animator.SetFloat("Horizontal", horizontalMove);
		if (hasVerticalAnimation)
			animator.SetFloat("Vertical", verticalMove);

		if (entity != null && havePath) {
			if (Vector2.Distance(waypoint, transform.position) < 0.25f) {
				GetNextWaypoint();
			} else {
				Vector2 move = (waypoint - (Vector2)transform.position).normalized;
				SetMoveDirection(move.x, move.y);
			}
		} else if (simpleFollowTarget) {
			SetMoveDirection((simpleFollowTarget.position - transform.position).normalized);
		}
	}

	private void FixedUpdate() {
		if (movementEnabled) {
			Move(horizontalMove * Time.fixedDeltaTime, verticalMove * Time.fixedDeltaTime);
		}
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
			SetMoveDirection(0, 0);
			destination = Vector2.zero;
		}
	}

	public void StartSimpleFollow(Transform target) {
		this.simpleFollowTarget = target;
	}

	public void StopSimpleFollow() {
		this.simpleFollowTarget = null;
	}

	public void Push(Vector2 pushDirection) {
		// SetMoveDirection(0, 0);
		rigidbody.velocity = pushDirection;
		velocity = pushDirection;
		movementEnabled = false;
	}

	public void SetMoveDirection(Vector2 moveDir) {
		SetMoveDirection(moveDir.x, moveDir.y);
	}

	public void SetMoveDirection(float horizontal, float vertical) {
		horizontalMove = horizontal * speed;
		verticalMove = vertical * speed;
		movementEnabled = true;
	}

	private void Move(float horizontal, float vertical) {
		//Move player by finding target velocity
		Vector2 targetVelocity = new Vector2(horizontal * 10f, vertical * 10f);
		//And then smoothing it out and applying it to the character
		rigidbody.velocity = Vector2.SmoothDamp(rigidbody.velocity, targetVelocity, ref velocity, MovementSmoothing);

		//If input is moving the player right and player is facing left
		if (spriteRenderer != null) {
			if (horizontal > 0 && !FacingRight) {
				Flip();
			} else if (horizontal < 0 && FacingRight) {
				Flip();
			}
		}
	}

	private void Flip() {
		FacingRight = !FacingRight;
		spriteRenderer.flipX = !spriteRenderer.flipX;
	}


}

