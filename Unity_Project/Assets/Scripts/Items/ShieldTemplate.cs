using UnityEngine;

namespace TimefulDungeon.Items {
    [CreateAssetMenu(fileName = "New Shield Template", menuName = "Interactables/Shield Template")]
    public class ShieldTemplate : EquippableTemplate {
        public int staminaUse;
        public float arc;
    }
}