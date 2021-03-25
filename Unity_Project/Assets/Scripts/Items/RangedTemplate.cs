using UnityEngine;

namespace TimefulDungeon.Items {
    [CreateAssetMenu(fileName = "New Ranged Template", menuName = "Interactables/Ranged Template")]
    public class RangedTemplate : WeaponTemplate {
        public Sprite projectile;
        public float speed;
        public int penetrate;
        public float spread;
    }
}