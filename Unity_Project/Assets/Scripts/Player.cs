using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
	[SerializeField] private float speed = 20f;
	[Range(0, .3f)] [SerializeField] private float MovementSmoothing = .05f; //How much to smooth movement
	[SerializeField] private LayerMask CollisionLayers; //Mask determining what the player runs into
	[SerializeField] private Rigidbody2D rbody;

	private Vector3 velocity = Vector3.zero;
	private float horizontalMove = 0f;
	private float verticalMove = 0f;

	private bool FacingRight = true;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		//Get input from player
		horizontalMove = Input.GetAxisRaw("Horizontal") * speed;
		verticalMove = Input.GetAxisRaw("Vertical") * speed;
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
		}
		else if (horizontal < 0 && FacingRight) {
			Flip();
		}
	}

	private void Flip() {
		FacingRight = ! FacingRight;
		transform.Rotate(0f, 180f, 0f);
	}
}