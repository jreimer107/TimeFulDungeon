using TimefulDungeon.AI;
using TimefulDungeon.Core;
using TimefulDungeon.UI;
using UnityEngine;

namespace TimefulDungeon.Enemies {
    public abstract class Enemy : Brain, IDamageable {
        public int maxHealth;
        public int damage;
        
        private int _health;

        private Rigidbody2D _rigidbody;

        protected override void Awake() {
            base.Awake();
            _health = maxHealth;
            _rigidbody = GetComponent<Rigidbody2D>();

        }

        public void Damage(int damage) {
            _health -= damage;
            Popup.CreatePopup(damage.ToString(), transform.position, Color.yellow);
            if (_health <= 0) Destroy(gameObject);
        }
    
        private void OnCollisionEnter2D(Collision2D other) {
            var player = other.collider.GetComponent<Player>();
            if (!player) return;
            player.Damage(damage);
            var knockback = (transform.Position2D() - player.transform.Position2D()) * 100 * _rigidbody.mass;
            print($"Applying knockback force: {knockback}");
            _rigidbody.AddForce(knockback, ForceMode2D.Impulse);
            player.ApplyKnockback(knockback * -1);
        }
    }
}