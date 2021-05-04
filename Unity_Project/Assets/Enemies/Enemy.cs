using TimefulDungeon.AI;
using TimefulDungeon.Core;
using TimefulDungeon.UI;
using UnityEngine;

namespace TimefulDungeon.Enemies {
    public abstract class Enemy : Brain, IDamageable, IPushable, IDamaging {
        private static LayerMask obstacleMask;

        public int maxHealth;
        public int damage;
        public float sightRange = 30f;
        public float searchTime = 10f;
        public Vector2 spawn { get; private set; }
        
        private int _health;
        
        public Transform target { get; private set; }


        protected new Rigidbody2D rigidbody;

        protected override void Awake() {
            base.Awake();
            _health = maxHealth;
            rigidbody = GetComponent<Rigidbody2D>();
            spawn = transform.Position2D();
            if (obstacleMask.value == 0) obstacleMask = LayerMask.GetMask("Obstacle");
        }

        public void Damage(int damage) {
            _health -= damage;
            Popup.CreatePopup(damage.ToString(), transform.position, Color.yellow);
            if (_health <= 0) Die();
        }

        public virtual void Die() {
            gameObject.SetActive(false);
            // Destroy(gameObject);
        }

        private void OnCollisionEnter2D(Collision2D other) {
            var player = other.collider.GetComponent<Player>();
            if (!player) return;
            player.Damage(damage);
            Push(player.Position2D());
            player.Push(transform.Position2D());
        }
        
        public void SetTarget(Transform newTarget) {
            target = newTarget;
            // targetInterest = new ContextSteering.Interest(target.Position2D());
        }
        
        public bool CanSeeTarget() {
            if (!target) return false;
            var ourPosition = transform.Position2D();
            var targetPosition = target.Position2D();
            var inRange = ourPosition.CloserThan(targetPosition, sightRange);
            if (!inRange) return false;
            var hit = Physics2D.Linecast(ourPosition, targetPosition, obstacleMask);
            return !hit.collider;
        }

        public int GetDamage() => damage;

        public void Push(Vector2 point, float magnitude = 1) {
            rigidbody.AddForce((transform.Position2D() - point).normalized * magnitude * rigidbody.mass * 100, ForceMode2D.Impulse);
        }
    }
}