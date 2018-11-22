using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour {
	[Range(0, .3f)] [SerializeField] private float MovementSmoothing = .05f; //How much to smooth movement
	[SerializeField] private LayerMask CollisionLayers; //Mask determining what the player runs into

	private Rigidbody2D rbody;
	private bool FacingRight = true; 
	private Vector3 velocity = Vector3.zero;

	private void Awake() {
		rbody = GetComponent<Rigidbody2D>();
	}

	private void FixedUpdate() {
		
	}

	private void Move(float move) {
		//Move player by finding target velocity
		Vector3 targetVelocity = new Vector2(move * 10f, rbody.velocity.y);
		//And then smoothing it out and applying it to the character
		rbody.velocity = Vector3.SmoothDamp(rbody.velocity, targetVelocity, ref velocity, MovementSmoothing);

		//If input is moving the player right and player is facing left
		if (move > 0 && !FacingRight) {
			Flip();
		}
		else if (move < 0 && FacingRight) {
			Flip();
		}
	}

	private void Flip() {
		// Switch the way the player is labelled as facing.
		FacingRight = !FacingRight;

		// Multiply the player's x local scale by -1.
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}
}
