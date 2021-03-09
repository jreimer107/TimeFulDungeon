using TimefulDungeon.Core;
using UnityEngine;

public class BulletParticle : MonoBehaviour {
    public int damage;

    private void OnParticleCollision(GameObject other) {
        other.TryGetComponent(out IDamageable damageable);
        damageable.Damage(damage);
    }
}