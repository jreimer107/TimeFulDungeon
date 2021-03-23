using UnityEngine;

namespace TimefulDungeon.Items {
    [CreateAssetMenu(fileName = "New Melee", menuName = "Interactables/Melee")]
    public class Melee : Weapon {
        public float arc;
        
        public override string GetTooltipText() {
            return
                $"<size=32>{name}</size>\n" +
                (description != "" ? $"{description}\n" : "") +
                $"{damage} dmg\n" +
                $"{arc}\u00b0 arc\n" +
                $"{rate}\u00b0/sec\n" +
                $"{range}m range\n" +
                $"{cooldown}s cooldown\n" +
                (redText != "" ? $"<color=red>{redText}</color>" : "");
        }
    }
}