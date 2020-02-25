using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class Player : MonoBehaviour {
	private CircleCollider2D pickupTrigger;
	private BoxCollider2D collisionCollider;

	public int health;
	public int maxHealth;
	public delegate void OnHealthChanged();
	public OnHealthChanged onHealthChangedCallback;

	public int stamina;
	public int maxStamina;

	private MovementController controller;

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
		health = 10;
		maxHealth = 10;
	}

	// Update is called once per frame
	private void Update() {
		//Get input from player
		controller.SetMoveDirection(
			Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")
		);

		if (Input.GetKeyDown(KeyCode.T)) {
			Damage(1);
		}
		if (Input.GetKeyDown(KeyCode.G)) {
			Heal(1);
		}

	}

	public void Damage(int damage) {
		health = Math.Max(0, health - damage);
		onHealthChangedCallback.Invoke();
		if (health == 0)
			Die();
	}

	public void Heal(int heal) {
		health = Math.Min(maxHealth, health + heal);
		onHealthChangedCallback.Invoke();
	}

	private void Die() {
		Debug.Log("Dead");
	}


	public CircleCollider2D pickupTriggerCollider { get => pickupTrigger; }
	public BoxCollider2D hitbox { get => collisionCollider; }
}