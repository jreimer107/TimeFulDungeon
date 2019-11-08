﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// The point which in-hand items will be attached to.
/// </summary>
public class HoldingPoint : MonoBehaviour {
	private Player player;
	private EquipmentManager equipmentManager;
	[Range(0, 5)] [SerializeField] private float radius = 1;

	private bool attacking = false;
	private float angle = 0.0f;
	private float startSwingAngle;

	private Equipment inHand;
	private int inHandIndex;
	private bool shielding;
	private GameObject child;
	private SpriteRenderer spriteRenderer;
	private PolygonCollider2D hitbox;

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
		equipmentManager = EquipmentManager.instance;
		equipmentManager.onEquipmentChangedCallback += UpdateRendered;
		child = transform.GetChild(0).gameObject;
		spriteRenderer = GetComponentsInChildren<SpriteRenderer>()[1];
	}

	private void Update() {
		// Check for weapon swapping
		if (Input.GetButtonDown("Swap Weapon")) {
			Debug.Log("Swap inhand");
			SwapInHand();
		}

		// Check for shielding
		if (Input.GetButtonDown("Shield")) {
			Debug.Log("Shield");
			SwapShield();
		} else if (Input.GetButtonUp("Shield")) {
			Debug.Log("Unshield");
			SwapShield();
		}

		// Check for attacking
		if (inHand != null && !attacking && Input.GetButton("Fire1") && !EventSystem.current.IsPointerOverGameObject()) {
			attacking = true;
			startSwingAngle = angle;
			angle += (inHand as Melee).arc / 2;
		}
	}

	// Update is called once per frame
	private void FixedUpdate() {
		// If attacking, swing. Else rotate to mouse.
		if (!attacking) {
			RotateToMouse();
		} else {
			angle -= (inHand as Melee).speed;
			if (angle <= startSwingAngle - (inHand as Melee).arc / 2) {
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

	/// <summary>
	/// Rotates the hand object to be between the player and the mouse pointer.
	/// </summary>
	/// <returns></returns>
	private float RotateToMouse() {
		Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition) - player.transform.position;

		//Determine angle of mouse
		angle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg;
		angle = angle < 0 ? angle + 360 : angle;
		return angle;
	}

	/// <summary>
	/// Swaps the currently used weapon in the player's hand.
	/// </summary>
	private void SwapInHand() {
		int meleeIndex = (int)EquipType.Melee;
		int rangedIndex = (int)EquipType.Ranged;
		inHandIndex = inHandIndex == meleeIndex ? rangedIndex : meleeIndex;
		inHand = equipmentManager.currentEquipment[inHandIndex];
		SwapRendered();
	}

	/// <summary>
	/// Swaps the currently used item to the equipped shield.
	/// </summary>
	/// <param name="onOff">Whether to equip or unequip the shield.</param>
	private void SwapShield() {
		int shieldIndex = (int)EquipType.Shield;
		shielding = !shielding;
		inHand = shielding ? equipmentManager.currentEquipment[shieldIndex] :
			equipmentManager.currentEquipment[inHandIndex];
		SwapRendered();
	}

	/// <summary>
	/// Swaps the currently rendered item to the in hand item.
	/// </summary>
	private void SwapRendered() {
		if (hitbox != null) {
			Destroy(hitbox);
		}
		if (inHand != null) {
			spriteRenderer.sprite = inHand.sprite;
			hitbox = child.AddComponent<PolygonCollider2D>();
			hitbox.enabled = false;
		}
	}

	private void UpdateRendered() {
		int shieldIndex = (int)EquipType.Shield;
		inHand = shielding ? equipmentManager.currentEquipment[shieldIndex] :
			equipmentManager.currentEquipment[inHandIndex];
		SwapRendered();
	}
}
