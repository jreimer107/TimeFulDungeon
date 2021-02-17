﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// The point which in-hand items will be attached to.
/// This is a controls object that changes the values in the player object.
/// </summary>
public class HoldingPoint : MonoBehaviour {
	private Player player;
	private EquipmentManager equipmentManager;
	[Range(0, 5)] [SerializeField] private float radius = 1;

	private bool attacking = false;
	private float angle = 0.0f;
	private float startSwingAngle;

	public Equipment inHand = null;
	private EquipType currentWeaponType = EquipType.Melee;
	private bool shieldToggleBuffer = false;

	private SpriteRenderer spriteRenderer;
	private EdgeCollider2D hitbox;
	public Animator animator;
	public AnimatorOverrideController animatorOverrideController;
	public AnimationClip testIdleClip, testAttackClip;
	public AnimationEvent startEvent;
	public AudioSource audioSource;
	public AudioClip soundEffect;

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
		spriteRenderer = GetComponent<SpriteRenderer>();
		hitbox = GetComponent<EdgeCollider2D>();
		hitbox.enabled = false;

		// Setup runtime clip changes
		animator = GetComponent<Animator>();
		animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
		animator.runtimeAnimatorController = animatorOverrideController;
		startEvent = new AnimationEvent();
		audioSource = GetComponent<AudioSource>();
		

		player.onStaminaEmptyCallback += ExhaustedUnshield;
	}

	private void Update() {
		// Check for weapon swapping
		if (Input.GetButtonDown("Swap Weapon")) {
			Debug.Log("Swap inhand");
			ToggleWeapon();
		}

		// If not attacking and shield is buffered, shield and consume the buffer
		shieldToggleBuffer = !player.exhausted && (Input.GetButtonDown("Shield") || Input.GetButtonUp("Shield"));
		if (shieldToggleBuffer && !attacking) {
			ToggleShield();
		}

		// Check for attacking
		if (inHand != null && !attacking && Input.GetButton("Fire1") && !EventSystem.current.IsPointerOverGameObject() && !ClickAndDrag.instance.active) {
			attacking = true;
			animator.SetBool("action", true);
			if (inHand.type == EquipType.Melee) {
				hitbox.enabled = true;
				startSwingAngle = angle;
				angle += (inHand as Melee).arc / 2;
			}
			else if (inHand.type == EquipType.Ranged) {
				Debug.Log("Shooting!");
			}
		}

		if (inHand != null && inHand.type == EquipType.Ranged && attacking && !Input.GetButton("Fire1") && !EventSystem.current.IsPointerOverGameObject() && !ClickAndDrag.instance.active) {
			attacking = false;
			animator.SetBool("action", false);
		}
	}

	// Update is called once per frame
	private void FixedUpdate() {
		// If attacking, swing. Else rotate to mouse.
		if (!attacking) {
			RotateToMouse();
		} else {
			if (!inHand || inHand.type != EquipType.Melee) {
				RotateToMouse();
				SetPosition();
				return;
			}
			angle -= (inHand as Melee).DeltaAngle;
			if (angle <= startSwingAngle - (inHand as Melee).arc / 2) {
				// Debug.Log("Attack ending");
				attacking = false;
				hitbox.enabled = false;
				animator.SetBool("action", false);
				RotateToMouse();
			}
		}

		SetPosition();
	}

	private void SetPosition() {
		//Assign the angle as the rotation of the held object
		transform.localEulerAngles = new Vector3(0, 0, angle);

		// Use the angle to determine the position of the held object
		float xpos = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
		float ypos = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
		transform.localPosition = new Vector3(xpos, ypos, 0);

		// Flip the sprite if on left side, but only we're not melee attacking - this messes up animations
		spriteRenderer.flipY = (inHand.type != EquipType.Melee || !attacking) ? xpos < 0 : false;

	}

	/// <summary>
	/// Rotates the hand object to be between the player and the mouse pointer.
	/// </summary>
	/// <returns></returns>
	private void RotateToMouse() {
		Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition) - player.transform.position;

		//Determine angle of mouse
		angle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg;
		angle = angle < 0 ? angle + 360 : angle;
	}

	/// <summary>
	/// Swaps the currently used weapon in the player's hand.
	/// Unlike shielding, can be done while shielding.
	/// </summary>
	private void ToggleWeapon() {
		currentWeaponType = currentWeaponType == EquipType.Melee ? EquipType.Ranged : EquipType.Melee;
		if (!player.Shielding) {
			inHand = equipmentManager.GetEquipment(currentWeaponType);
			SwapRendered();
		}
	}

	/// <summary>
	/// Swaps the currently used item to the equipped shield.
	/// </summary>
	private void ToggleShield() {
		// TODO: Needs to be passed an input. Exhaustion has a bug otherwise.
		shieldToggleBuffer = false;
		if (player.exhausted) {
			Debug.Log("Player is exhausted!");
			return;
		}

		Equipment shield = equipmentManager.Shield;
		if (shield) {
			player.ToggleShielding();
			inHand = player.Shielding ? shield : equipmentManager.GetEquipment(currentWeaponType);
			SwapRendered();
		} else {
			Debug.Log("No shield equipped!");
		}
	}

	private void ExhaustedUnshield() {
		Debug.Log("Force unshield");
		if (player.Shielding) {
			ToggleShield();

		}
	}

	/// <summary>
	/// Swaps the currently rendered item to the in hand item.
	/// </summary>
	private void SwapRendered() {
		Debug.Log("In hand: " + inHand);
		if (inHand != null) {
			spriteRenderer.sprite = inHand.sprite;
			animatorOverrideController["idle"] = inHand.idleClip;
			// inHand.actionClip.AddEvent(startEvent);
			animatorOverrideController["action"] = inHand.actionClip;
			// soundEffect = inHand.soundEffect;
			//Set animation speed
			inHand.Equip(this.animator, this.hitbox);
		} else {
			spriteRenderer.sprite = null;
			animatorOverrideController["idle"] = null;
			animatorOverrideController["action"] = null;
		}
	}

	private void UpdateRendered() {
		if (inHand != null)
			inHand = equipmentManager.GetEquipment(inHand.type);
		else
			inHand = equipmentManager.GetEquipment(currentWeaponType);
		SwapRendered();
	}
}