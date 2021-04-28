using TimefulDungeon.AI;
using TimefulDungeon.Core;
using TimefulDungeon.UI;
using UnityEngine;

namespace TimefulDungeon.Enemies {
    public abstract class Enemy : Brain, IDamageable, IPushable {
        private static LayerMask obstacleMask;

        public int maxHealth;
        public int damage;
        public float sightRange = 30f;
        public float searchTime = 10f;
        public Vector2 spawn { get; private set; }
        
        private int _health;
        
        public Transform target { get; private set; }


        private Rigidbody2D _rigidbody;

        protected override void Awake() {
            base.Awake();
            _health = maxHealth;
            _rigidbody = GetComponent<Rigidbody2D>();
            spawn = transform.Position2D();
            if (obstacleMask.value == 0) obstacleMask = LayerMask.GetMask("Obstacle");
        }

        public void Damage(int damage) {
            _health -= damage;
            Popup.CreatePopup(damage.ToString(), transform.position, Color.yellow);
            // if (_health <= 0) Destroy(gameObject);
            if (_health <= 0) gameObject.SetActive(false);
        }
    
        private void OnCollisionEnter2D(Collision2D other) {
            var player = other.collider.GetComponent<Player>();
            if (!player) return;
            player.Damage(damage);
            var knockback = (transform.Position2D() - player.transform.Position2D()) * 100;
            Push(knockback);
            player.Push(-knockback);
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

        public void Push(Vector2 force) {
            _rigidbody.AddForce(force * _rigidbody.mass, ForceMode2D.Impulse);
        }
    }
}