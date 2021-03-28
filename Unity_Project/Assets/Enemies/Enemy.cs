using TimefulDungeon.AI;
using TimefulDungeon.Core;
using TimefulDungeon.UI;
using UnityEngine;

namespace TimefulDungeon.Enemies {
    public abstract class Enemy : Brain, IDamageable {
        public int maxHealth;
        public int damage;
        
        private int _health;

        protected override void Awake() {
            base.Awake();
            _health = maxHealth;
        }

        public void Damage(int damage) {
            _health -= damage;
            Popup.CreatePopup(damage.ToString(), transform.position, Color.yellow);
            if (_health <= 0) Destroy(gameObject);
        }
    
        private void OnCollisionEnter2D(Collision2D other) {
            other.collider.GetComponent<Player>()?.Damage(damage);
        }
    }
}