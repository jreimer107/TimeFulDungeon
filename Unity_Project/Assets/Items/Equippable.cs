using System;
using System.Reflection;
using TimefulDungeon.Core;
using TimefulDungeon.Misc;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TimefulDungeon.Items {
    public abstract class Equippable : Item {
        private readonly Inventory _inventory;

        protected readonly HoldingPoint holdingPoint;
        protected readonly int level;
        protected float levelScale => Mathf.Pow(1.1f, level);
        protected Prefix prefix;

        protected Equippable(EquippableTemplate template) : base(template) {
            var player = Player.instance;
            holdingPoint = player.HoldingPoint;
            _inventory = player.Inventory;

            level = Random.Range(1, 6);
        }

        public EquipType type => ((EquippableTemplate) template).type;

        public bool Activated { get; private set; }

        public override void Select() { }

        public override void Use() {
            base.Use();
            _inventory.Equip(this);
        }

        public virtual void Update() { }

        public virtual void OnEnable() {
            if (idleClip && actionClip) {
                holdingPoint.AnimatorOverrideController["idle"] = idleClip;
                holdingPoint.AnimatorOverrideController["action"] = actionClip;
                holdingPoint.Animator.enabled = true;
            }
            else {
                holdingPoint.Animator.enabled = false;
                holdingPoint.SpriteRenderer.sprite = sprite;
            }

            holdingPoint.SpriteRenderer.enabled = true;
        }

        public virtual void OnDisable() { }

        public virtual void Activate() {
            Activated = true;
        }

        public virtual void Deactivate() {
            Activated = false;
        }

        public virtual void OnActionLoop() { }

        public virtual void OnCollision(Collider2D other) { }

        protected string GetNameLevelDescription() {
            return
                $"<size=32>{prefix.translatedValue} {Translations.Get(name)}</size>\n" +
                $"Lv. {level}\n" +
                (description != "" ? $"{Translations.Get(description)}\n" : "") +
                GetFormattedRedText();
        }
        
        public T GetValue<T>(string fieldName) {
            var fieldInfo = GetFieldInfo(fieldName);
            if (fieldInfo != null && fieldInfo.FieldType == typeof(T)) {
                return (T) fieldInfo.GetValue(this);
            }
            throw new ArgumentException("Requested field is not given type or is not a field.");
        }

        public void SetValue<T>(string fieldName, T newValue) {
            var fieldInfo = GetFieldInfo(fieldName);
            if (fieldInfo.FieldType == typeof(T)) {
                fieldInfo.SetValue(this, newValue);
            }
            throw new ArgumentException("Requested field is not given type or is not a field.");
        }

        public void AdjustModifier(string fieldName, float modifier, ModifierMode mode) {
            var fieldInfo = GetModifierInfo(fieldName);
            if (fieldInfo == null)
                throw new ArgumentException("Requested field is not a valid modifier.");
            var modifiedValue = mode switch {
                ModifierMode.Add => (float) fieldInfo.GetValue(this) + modifier,
                ModifierMode.Subtract => (float) fieldInfo.GetValue(this) - modifier,
                ModifierMode.Multiply => (float) fieldInfo.GetValue(this) * modifier,
                ModifierMode.Divide => (float) fieldInfo.GetValue(this) / modifier,
                _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
            };
            fieldInfo.SetValue(this, modifiedValue);
        }

        private FieldInfo GetFieldInfo(string fieldName) => GetType().GetField(fieldName);

        private FieldInfo GetModifierInfo(string modifierName) => GetType().GetField( modifierName + "Mod");
    }
}