namespace TimefulDungeon.Items.Behaviors {
    public class NoneBehavior : EquippableBehavior {
        private void Awake() {
            Type = EquipType.None;
        }

        protected override void OnEnable() {
            base.OnEnable();

            holdingPoint.SpriteRenderer.sprite = null;
            holdingPoint.SpriteRenderer.enabled = false;
            holdingPoint.AnimatorOverrideController["idle"] = null;
            holdingPoint.AnimatorOverrideController["action"] = null;
        }
    }
}