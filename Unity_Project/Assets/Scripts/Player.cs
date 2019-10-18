﻿using UnityEngine;

public class Player : MonoBehaviour {
	[SerializeField] private float speed = 20f;
	[Range(0, .3f)] [SerializeField] private float MovementSmoothing = .05f; //How much to smooth movement
	[SerializeField] private LayerMask CollisionLayers; //Mask determining what the player runs into
	private Rigidbody2D rbody;

	private Vector3 velocity = Vector3.zero;
	private float horizontalMove = 0f;
	private float verticalMove = 0f;

	private bool FacingRight = true;

	#region Singleton
	public static Player instance;
	void Awake() {
		if (instance != null) {
			Debug.LogWarning("More than one instance of Player found.");
		}
		instance = this;
	}
	#endregion

	// Use this for initialization
	void Start() {
		rbody = GetComponent<Rigidbody2D>();
	}

	// Update is called once per frame
	void Update() {
		//Get input from player
		horizontalMove = Input.GetAxisRaw("Horizontal") * speed;
		verticalMove = Input.GetAxisRaw("Vertical") * speed;

		//Check for pickups
		// Collider2D[] collisions = Physics2D.OverlapCircleAll(transform.position, pickup_radius);
		// foreach (Collider2D collision in collisions) {
		//     // Debug.Log("Player collided with things.");
		//     if (collision.gameObject.CompareTag("Pickup")) {
		//         collision.GetComponent<Pickup>().pickup();
		//         Debug.Log("Player picked up object!");
		//     }

		// }
	}

	private void FixedUpdate() {
		//Move character
		Move(horizontalMove * Time.fixedDeltaTime, verticalMove * Time.fixedDeltaTime);
	}

	public void Move(float horizontal, float vertical) {
		//Move player by finding target velocity
		Vector3 targetVelocity = new Vector2(horizontal * 10f, vertical * 10f);
		//And then smoothing it out and applying it to the character
		rbody.velocity = Vector3.SmoothDamp(rbody.velocity, targetVelocity, ref velocity, MovementSmoothing);

		//If input is moving the player right and player is facing left
		if (horizontal > 0 && !FacingRight) {
			Flip();
		} else if (horizontal < 0 && FacingRight) {
			Flip();
		}
	}

	private void Flip() {
		FacingRight = !FacingRight;
		transform.Rotate(0f, 180f, 0f);
	}
}