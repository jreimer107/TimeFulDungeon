using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

[RequireComponent(typeof(Animator), typeof(Rigidbody2D))]
public class MovementController : MonoBehaviour {
	private float horizontalMove;
	private float verticalMove;
	private Vector2 destination;
	public bool travelling { private set; get; }

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

	private void Start() {
		horizontalMove = 0;
		verticalMove = 0;
		velocity = Vector3.zero;
		destination = Vector2.zero;

		rigidbody = GetComponent<Rigidbody2D>();
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
		if (hasHorizontalAnimation)
			animator.SetFloat("Horizontal", horizontalMove);
		if (hasVerticalAnimation)
			animator.SetFloat("Vertical", verticalMove);


		if (travelling) {
			if (Vector2.Distance(destination, transform.position) < 0.1f) {
				SetMoveDirection(0f, 0f);
				destination = Vector2.zero;
				travelling = false;
			} else {
				Vector2 move = (destination - (Vector2)transform.position).normalized;
				SetMoveDirection(move.x, move.y);
			}
		}
	}

	private void FixedUpdate() {
		Move(horizontalMove * Time.fixedDeltaTime, verticalMove * Time.fixedDeltaTime);
	}

	public void CreateEntityForPathfinding() {
		entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
		entity = entityManager.CreateEntity();
		entityManager.AddComponentData(entity, new PathFollow { pathIndex = -1 });
		entityManager.AddBuffer<PathPosition>(entity);
	}

	public void SetDestination(Vector2 destination) {
		this.destination = destination;
		travelling = true;
	}

	public void RequestPath(Vector2 destination) {
		PathfindingGrid.Instance.RequestPath(entity, transform.position, destination);
	}

	public void UpdateWaypoint() {
		if (entity == null) {
			return;
		}

		PathFollow pathFollow = entityManager.GetComponentData<PathFollow>(entity);
		DynamicBuffer<PathPosition> path = entityManager.GetBuffer<PathPosition>(entity);
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

		int index = pathFollow.pathIndex;

		if (index >= 0) {
			if (!travelling) {
				int2 pathPosition = path[index].position;

				Vector2 wayPoint = PathfindingGrid.Instance.GetWorldPosition(pathPosition);
				SetDestination(wayPoint);
				Debug.LogFormat("Setting waypoint to {0}", wayPoint);
				pathFollow.pathIndex--;
				entityManager.SetComponentData(entity, pathFollow);
			}
		}
	}

	public void SetMoveDirection(float horizontal, float vertical) {
		horizontalMove = horizontal * speed;
		verticalMove = vertical * speed;
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

