using UnityEngine;

namespace TimefulDungeon.Items {
    [CreateAssetMenu(fileName = "New Shield", menuName = "Interactables/Shield")]
    public class Shield : Equippable {
        public int staminaUse;
        public float arc;

        public Shield(string name, Sprite sprite, int staminaUse, float arc, float cooldown) : base(name, 3,
            "random shield", sprite, cooldown, EquipType.Shield) {
            this.staminaUse = staminaUse;
            this.arc = arc;
        }
    }
}