﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoldingPoint : MonoBehaviour {
	private Player player;
	[Range(0, 5)] [SerializeField] private float radius = 1;

	private bool attacking = false;
	private float angle = 0.0f;
	private float startSwingAngle;

	void Start() {
		player = Player.instance;
	}

	// Update is called once per frame
	void FixedUpdate() {
		//Get mousebuttondown
		if (Input.GetMouseButtonDown(0) && !attacking && !EventSystem.current.IsPointerOverGameObject()) {
			attacking = true;
			startSwingAngle = angle;
			angle += 30.0f;
		}

		if (!attacking) {
			RotateToMouse();
		} else {
			angle -= 2;
			if (angle <= startSwingAngle - 45.0f) {
				angle = startSwingAngle;
				attacking = false;
			}
		}

		//Assign the angle as the rotation of the held object
		transform.localEulerAngles = new Vector3(0, 0, angle);

		// Use the angle to determine the position of the held object
		float xpos = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
		float ypos = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
		transform.localPosition = new Vector3(player.transform.position.x + xpos, player.transform.position.y + ypos, 0);
	}

	private float RotateToMouse() {
		Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition) - player.transform.position;

		//Determine angle of mouse
		angle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg;
		if (angle < 0.0f) {
			angle += 360.0f;
		}
		return angle;
	}
}
