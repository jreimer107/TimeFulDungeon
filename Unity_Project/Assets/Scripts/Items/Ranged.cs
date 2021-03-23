using UnityEngine;

namespace TimefulDungeon.Items {
    [CreateAssetMenu(fileName = "New Ranged", menuName = "Interactables/Ranged")]
    public class Ranged : Weapon {
        public Sprite projectile;
        public float speed;
        public int penetrate;
        public float spread;
    }
}