using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAttack : MonoBehaviour {
	public float AttackDelay;
	private float AttackTime;

	public Transform attackPos;
	public LayerMask enemyLayer;
	public float attackRange;
	public int damage;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (AttackTime <= 0) {
			if (Input.GetAxisRaw("Fire2") > 0f) {
				AttackTime = AttackDelay;
				Collider2D[] enemiesHit = Physics2D.OverlapCircleAll(attackPos.position, attackRange, enemyLayer);
				for (int i = 0; i < enemiesHit.Length; i++) {
					enemiesHit[i].GetComponent<Enemy>().TakeDamage(damage);
				}
				//Physics2D.OverlapBoxAll()
			}
		}
		else {
			AttackTime -= Time.fixedDeltaTime;
		}
	}

	private void OnDrawGizmosSelected() {
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(attackPos.position, attackRange);
	}
}
