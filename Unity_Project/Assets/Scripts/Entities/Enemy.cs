using TimefulDungeon.Core;
using TimefulDungeon.UI;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable {
    public int health;
    public float speed;

    public void Damage(int damage) {
        health -= damage;
        Popup.CreatePopup(damage.ToString(), transform.position, Color.yellow);
        if (health <= 0) Destroy(gameObject);
    }
}