using UnityEngine;

namespace TimefulDungeon.Items {
    [CreateAssetMenu(fileName = "New Shield", menuName = "Interactables/Shield")]
    public class Shield : Equippable {
        public int staminaUse;
        public float arc;
    }
}