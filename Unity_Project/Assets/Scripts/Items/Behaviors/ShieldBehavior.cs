namespace TimefulDungeon.Items.Behaviors {
    public class ShieldBehavior : EquippableBehavior {
        protected Shield data {
            get => that as Shield;
            set => that = value;
        }

        private void Awake() {
            Type = EquipType.Shield;
        }
    }
}