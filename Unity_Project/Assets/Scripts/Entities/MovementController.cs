using UnityEngine;

[RequireComponent(typeof(Animator), typeof(Rigidbody2D))]
public class MovementController : MonoBehaviour {
	private float horizontalMove;
	private float verticalMove;

	[SerializeField] private float speed = 20f;
	[Range(0, .3f)] [SerializeField] private float MovementSmoothing = .05f;

	private new Rigidbody2D rigidbody;
	private SpriteRenderer spriteRenderer;
	private Animator animator;
	private bool hasVerticalAnimation;
	private bool hasHorizontalAnimation;
	private bool FacingRight;

	private Vector2 velocity;

	private void Start() {
		horizontalMove = 0;
		verticalMove = 0;
		velocity = Vector3.zero;

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
	}

	private void FixedUpdate() {
		Move(horizontalMove * Time.fixedDeltaTime, verticalMove * Time.fixedDeltaTime);
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
		if (horizontal > 0 && !FacingRight) {
			Flip();
		} else if (horizontal < 0 && FacingRight) {
			Flip();
		}
	}

	private void Flip() {
		FacingRight = !FacingRight;
		spriteRenderer.flipX = !spriteRenderer.flipX;
	}


}

