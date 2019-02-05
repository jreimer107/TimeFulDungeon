using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldingPoint : MonoBehaviour {
	[SerializeField] private Transform player;
	[Range(1,5)] [SerializeField] private float radius = 1;

    // Update is called once per frame
    void FixedUpdate() {
		// Find the mouse position
		Vector3 mousePos = Input.mousePosition;

		// Convert mouse position to be relative to player position
		mousePos.z = player.transform.position.z - Camera.main.transform.position.z;
		mousePos = Camera.main.ScreenToWorldPoint(mousePos);
		mousePos = mousePos - player.transform.position;

		//Determine angle of mouse
		float angle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg;
		if (angle < 0.0f) {
			angle += 360.0f;
		}
		
		//Assign the angle as the rotation of the held object
		transform.localEulerAngles = new Vector3(0,0, angle);

		// Use the angle to determine the position of the held object
		float xpos = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
		float ypos = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
		transform.localPosition = new Vector3(player.transform.position.x + xpos, player.transform.position.y + ypos, 0);
    }
}
