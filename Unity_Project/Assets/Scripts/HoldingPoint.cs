using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoldingPoint : MonoBehaviour {
	private Player player;
	[Range(0, 5)] [SerializeField] private float radius = 1;

	private bool attacking = false;
	private float angle = 0.0f;
	private float startSwingAngle;

	private Equipment equipped;
	private SpriteRenderer spriteRenderer;

	#region Singleton
	public static HoldingPoint instance;
	private void Awake() {
		if (instance != null) {
			Debug.LogWarning("More than one instance of holding point detected.");
		}
		instance = this;
	}
	#endregion

	private void Start() {
		player = Player.instance;
		spriteRenderer = GetComponentsInChildren<SpriteRenderer>()[1];
	}

	private void Update() {
		if (equipped != null && !attacking && Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject()) {
			attacking = true;
			startSwingAngle = angle;
			angle += (equipped as Melee).arc / 2;
		}
	}

	// Update is called once per frame
	private void FixedUpdate() {
		if (!attacking) {
			RotateToMouse();
		} else {
			angle -= (equipped as Melee).speed;
			if (angle <= startSwingAngle - (equipped as Melee).arc / 2) {
				attacking = false;
				RotateToMouse();
			}
		}

		//Assign the angle as the rotation of the held object
		transform.localEulerAngles = new Vector3(0, 0, angle);

		// Use the angle to determine the position of the held object
		float xpos = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
		float ypos = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
		transform.localPosition = new Vector3(xpos, ypos, 0);
	}

	private float RotateToMouse() {
		Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition) - player.transform.position;

		//Determine angle of mouse
		angle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg;
		angle = angle < 0 ? angle + 360 : angle;
		return angle;
	}
}
