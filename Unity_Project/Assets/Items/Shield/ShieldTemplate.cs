using UnityEngine;

namespace TimefulDungeon.Items.Shield {
    [CreateAssetMenu(fileName = "New Shield Template", menuName = "Interactables/Shield Template")]
    public class ShieldTemplate : EquippableTemplate {
        public int armor;
        public int staminaUse;
        public float arc;
        public float knockback;

        public override Item GetInstance() {
            return new Shield(this);
        }
    }
}