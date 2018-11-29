using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
	public CharacterController controller;

	[SerializeField] private float speed = 20f;

	private Vector2 direction;
	private float horizontalMove = 0f;
	private float verticalMove = 0f;

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
		controller.Move(horizontalMove * Time.fixedDeltaTime, verticalMove * Time.fixedDeltaTime);
	}
}
