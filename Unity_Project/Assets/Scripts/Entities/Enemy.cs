﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class Enemy : MonoBehaviour {
	public int health;
	public float speed;

	private Animator anim;

	private MovementController movementController;

	private Transform player;

	// Use this for initialization
	void Start () {
		player = Player.instance.transform;
		//anim = GetComponent<Animator>();
		//anim.SetBool("isRunning", true);
		movementController = GetComponent<MovementController>();
		movementController.CreateEntityForPathfinding();
	}
	
	// Update is called once per frame
	void Update () {
		// if (Input.GetMouseButtonDown(0)) {
		// 	movementController.Travel(Player.instance.transform.position);
		// }
		movementController.Seek(Utils.GetMouseWorldPosition2D());

		// movementController.UpdateWaypoint();
	}

	public void TakeDamage(int damage) {
		health -= damage;
		if (health <= 0) {
			Destroy(gameObject);
		}
	}

	private void OnTriggerEnter2D(Collider2D other) {
		Debug.LogFormat("Collision {0}",other.tag);
		if (other.CompareTag("PlayerAttack")) {
			Debug.Log("Taking damage");;
			int damage = (HoldingPoint.instance.inHand as Melee).damage;
			TakeDamage(damage);
			Popup.CreateDamagePopup(damage.ToString(), transform.position, Color.yellow);
		}
	}

}
