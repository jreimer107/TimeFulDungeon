using UnityEngine;

namespace TimefulDungeon.Items {
    [CreateAssetMenu(fileName = "New Melee Template", menuName = "Interactables/Melee Template")]
    public class MeleeTemplate : WeaponTemplate {
        public float arc;

        public override Item GetInstance() {
            return new Melee.Melee(this);
        }
    }
}