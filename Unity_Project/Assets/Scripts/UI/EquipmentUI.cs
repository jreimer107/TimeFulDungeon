using System;
using TimefulDungeon.Items;
using UnityEngine;

namespace TimefulDungeon.UI {
    public class EquipmentUI : MonoBehaviour {
        private EquipmentSlot[] slots;

        private void Start() {
            slots = GetComponentsInChildren<EquipmentSlot>();
            Array.Sort(slots, (x, y) => x.type.CompareTo(y.type));
        }

        public void UpdateUI(EquipType type) {
            slots?[(int) type]?.Refresh();
        }
    }
}