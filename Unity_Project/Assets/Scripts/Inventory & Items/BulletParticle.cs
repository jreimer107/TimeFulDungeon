using UnityEngine;

public class BulletParticle : MonoBehaviour
{
    private void OnParticleCollision(GameObject other) {
        other.TryGetComponent(out Enemy enemy);
        enemy?.TakeDamage(1);
        if (enemy) {
            Popup.CreateDamagePopup("1", enemy.transform.position, Color.yellow);
        }
    }
}
