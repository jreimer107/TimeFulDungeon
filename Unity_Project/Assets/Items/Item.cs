using System;
using TimefulDungeon.Misc;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TimefulDungeon.Items {
    [Serializable]
    public class Item {
        private const float RandomizationRange = 0.25f;
        protected const string FLOAT_FORMAT = "F1";

        // Randomized fields, configured on construction
        public string name;
        public int count;
        public float cooldown;
        
        protected readonly ItemTemplate template;

        private string _toolTipText;

        public Item(ItemTemplate template) {
            this.template = template;
            name = template.name;
            count = template.count;
            cooldown = template.cooldown;
        }

        // Template fields, not changeable
        public int ID => template.id;
        public string Description => template.description;
        public string RedText => template.redText;
        public bool Stackable => template.stackable;
        public bool AutoPickup => template.autoPickup;
        public Sprite Sprite => template.sprite;
        public AnimationClip IdleClip => template.idleClip;
        public AnimationClip ActionClip => template.actionClip;
        public AudioClip SoundEffect => template.soundEffect;

        protected static string FormatFloat(float value) {
            return value.ToString(FLOAT_FORMAT);
        }

        protected static float GetModifier() {
            return 1 + Random.Range(-RandomizationRange, RandomizationRange);
        }

        public bool Equals(Item other) {
            return !ReferenceEquals(other, null) && ID == other.ID;
        }

        public virtual void Select() { } //Left mouse click

        public virtual void Use() {
            //Right mouse click
            Debug.Log("Using item " + name);
        }

        public override int GetHashCode() {
            return name.GetHashCode();
        }

        public override bool Equals(object other) {
            return other is Item item && Equals(item);
        }

        public static bool operator ==(Item a, Item b) {
            return a is { } && (ReferenceEquals(a, b) || a.Equals(b));
        }

        public static bool operator !=(Item a, Item b) {
            return !ReferenceEquals(a, null) && !a.Equals(b);
        }

        public static bool operator !(Item a) {
            return ReferenceEquals(a, null);
        }

        public static bool operator true(Item a) {
            return !ReferenceEquals(a, null);
        }

        public static bool operator false(Item a) {
            return ReferenceEquals(a, null);
        }

        public bool ToBool() {
            return !ReferenceEquals(this, null);
        }

        protected virtual string CalculateTooltipText() {
            return $"<size=32>{name}</size>\n{Description}\n<color=red>{RedText}</color>";
        }

        public string GetTooltipText() {
            return _toolTipText ??= CalculateTooltipText();
        }

        protected string GetFormattedRedText() {
            return RedText != "" ? $"<color=red>{Translations.Get(RedText)}</color>\n" : "";
        }
    }
}