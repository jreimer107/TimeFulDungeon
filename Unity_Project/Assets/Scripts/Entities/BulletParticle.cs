using TimefulDungeon.Core;
using UnityEngine;

namespace TimefulDungeon.Entities {
    public class BulletParticle : MonoBehaviour {
        public int damage;

        private void OnParticleCollision(GameObject other) {
            other.GetComponent<IDamageable>()?.Damage(damage);
        }
    }
}