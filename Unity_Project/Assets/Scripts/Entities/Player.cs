using UnityEngine;
using System;
using UnityEditor;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class Player : MonoBehaviour {
	private CircleCollider2D pickupTrigger;
	private BoxCollider2D collisionCollider;

	public int health;
	public int maxHealth;
	public delegate void OnHealthChanged();
	public OnHealthChanged onHealthChangedCallback;

	private bool shielding = false;
	public float stamina;
	public float maxStamina;
	public float staminaRegen;
	public bool exhausted = false;
	public delegate void OnMaxStaminaChanged();
	public OnMaxStaminaChanged onMaxStaminaChangedCallback;
	public delegate void OnStaminaEmpty();
	public OnStaminaEmpty onStaminaEmptyCallback;

	private MovementController controller;
	private EquipmentManager equipmentManager;

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
	private void Start() {
		pickupTrigger = GetComponentInChildren<CircleCollider2D>();
		collisionCollider = GetComponent<BoxCollider2D>();
		controller = GetComponent<MovementController>();
		controller.automatedMovement = false;
		equipmentManager = EquipmentManager.instance;
	}

	// Update is called once per frame
	private void Update() {
		//Get input from player
		controller.SetDesiredDirection(
			new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"))
		);

		if (Input.GetKeyDown(KeyCode.G)) {
			Damage(1);
		}
		if (Input.GetKeyDown(KeyCode.H)) {
			Heal(1);
		}

		if (Input.GetKeyDown(KeyCode.T)) {
			ChatBubble.Create(transform, new Vector3(.5f, .5f), "Quack damn you!");
		}

		// Adjust stamina based on shielding or regenning
		if (shielding) {
			stamina = Mathf.Max(
				0,
				stamina - equipmentManager.Shield.staminaUse * Time.deltaTime
			);
		} else if (stamina < maxStamina) {
			stamina = Mathf.Min(
				maxStamina,
				stamina + staminaRegen * Time.deltaTime
			);
		}

		// If stamina runs out, set exhausted so they can't shield until refill
		if (stamina == 0) {
			if (onStaminaEmptyCallback != null) {
				Debug.Log("Invoking empty callback");
				onStaminaEmptyCallback.Invoke();
			}
			exhausted = true;
		}
		if (exhausted && stamina == maxStamina) {
			exhausted = false;
		}
	}

	public void Damage(int damage) {
		health = Math.Max(0, health - damage);
		Popup.CreateDamagePopup(damage.ToString(), transform.position, Color.red);
		onHealthChangedCallback.Invoke();
		if (health == 0)
			Die();
	}

	public void Heal(int heal) {
		health = Math.Min(maxHealth, health + heal);
		onHealthChangedCallback.Invoke();
	}

	private void Die() {
#if UNITY_EDITOR
		EditorApplication.ExitPlaymode();
#endif
		Debug.Log("Dead");
	}

	public bool ToggleShielding() {
		shielding = !shielding;
		return shielding;
	}
	public bool Shielding { get => shielding; }


	public CircleCollider2D pickupTriggerCollider { get => pickupTrigger; }
	public BoxCollider2D hitbox { get => collisionCollider; }

	private void OnCollisionEnter2D(Collision2D collision) {
		if (collision.collider.CompareTag("Enemy")) {
			Damage(1);
		}
	}
}