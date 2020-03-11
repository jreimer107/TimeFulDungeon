using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {
	public int health;
	public float speed;

	private Animator anim;

	private DamagePopupManager damagePopupManager;

	// Use this for initialization
	void Start () {
		//anim = GetComponent<Animator>();
		//anim.SetBool("isRunning", true);
		damagePopupManager = DamagePopupManager.instance;
	}
	
	// Update is called once per frame
	void Update () {
		//transform.Translate(Vector2.left * speed * Time.deltaTime);
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
			damagePopupManager.CreateDamagePopup(damage.ToString(), transform.position, Color.yellow);
		}
	}

}
