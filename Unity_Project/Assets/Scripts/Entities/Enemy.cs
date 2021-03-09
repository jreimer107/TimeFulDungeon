using TimefulDungeon;
using TimefulDungeon.Core;
using TimefulDungeon.UI;
using UnityEngine;

public class Enemy : MonoBehaviour {
    public int health;
    public float speed;

    private void OnTriggerEnter2D(Collider2D other) {
        Debug.LogFormat("Collision {0}", other.tag);
        if (!other.CompareTag("PlayerAttack")) return;
        Debug.Log("Taking damage");
        var damage = ((Melee) HoldingPoint.instance.inHand).damage;
        TakeDamage(damage);
        Popup.CreatePopup(damage.ToString(), transform.position, Color.yellow);
    }

    public void TakeDamage(int damage) {
        health -= damage;
        if (health <= 0) Destroy(gameObject);
    }
}