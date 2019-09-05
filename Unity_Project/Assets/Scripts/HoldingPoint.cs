using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldingPoint : MonoBehaviour {
	[SerializeField] private Player player;
	private Transform player_pos;
	[Range(0, 5)] [SerializeField] private float radius = 1;

	private bool attacking = false;
	private float angle = 0.0f;
	private float startSwingAngle;

	void Start() {
		player_pos = player.transform;
	}

	// Update is called once per frame
	void FixedUpdate() {
		//Get mousebuttondown
		if (Input.GetMouseButtonDown(0) && !attacking) {
			attacking = true;
			startSwingAngle = angle;
			angle += 30.0f;
		}

		if (!attacking) {
			RotateToMouse();
		} else {
			angle -= 2;
			if (angle <= startSwingAngle - 30.0f) {
				angle = startSwingAngle;
				attacking = false;
			}
		}

		//Assign the angle as the rotation of the held object
		transform.localEulerAngles = new Vector3(0, 0, angle);

		// Use the angle to determine the position of the held object
		float xpos = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
		float ypos = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
		transform.localPosition = new Vector3(player_pos.transform.position.x + xpos, player_pos.transform.position.y + ypos, 0);
	}

	private float RotateToMouse() {
		Vector3 mousePos = Input.mousePosition;

		// Convert mouse position to be relative to player position
		mousePos.z = player_pos.transform.position.z - Camera.main.transform.position.z;
		mousePos = Camera.main.ScreenToWorldPoint(mousePos);
		mousePos = mousePos - player_pos.transform.position;

		//Determine angle of mouse
		angle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg;
		if (angle < 0.0f) {
			angle += 360.0f;
		}
		return angle;
	}
}
