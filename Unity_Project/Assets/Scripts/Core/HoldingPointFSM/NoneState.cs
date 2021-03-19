using TimefulDungeon.Items;

namespace TimefulDungeon.Core.HoldingPointFSM {
    public class NoneState : EquippableState {
        public NoneState(HoldingPoint fsm) : base(fsm) {
            Name = EquipType.None;
            transitions.Add(EquipType.Melee, ToMelee);
            transitions.Add(EquipType.Ranged, ToRanged);
            transitions.Add(EquipType.Shield, ToShield);
        }
    }
}