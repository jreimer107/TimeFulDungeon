using TimefulDungeon;
using TimefulDungeon.Core;
using TimefulDungeon.UI;
using UnityEngine;

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
	}
	
	// Update is called once per frame
	void Update () {
		// if (Input.GetMouseButtonDown(0)) {
		// 	movementController.Travel(Player.instance.transform.position);
		// }
		// movementController.AutomatedMovement(Utils.GetMouseWorldPosition2D());

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
			Popup.CreatePopup(damage.ToString(), transform.position, Color.yellow);
		}
	}

}
