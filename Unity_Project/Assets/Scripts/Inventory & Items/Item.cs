using System;
using UnityEngine;

namespace TimefulDungeon {
    [CreateAssetMenu(fileName = "New Item", menuName = "Interactables/Item")]
    public class Item : ScriptableObject, IEquatable<Item> {
        public new string name;
        public int id;
        public string description;
        public string redText;
        public Sprite sprite;
        public bool stackable;
        public int count;
        public float cooldown;
        public bool autoPickup;
        public AnimationClip idleClip;
        public AnimationClip actionClip;
        public AudioClip soundEffect;

        public Item(string name, int id, string description, Sprite sprite, bool stackable, int count, float cooldown,
            bool autoPickup, string redText = "") {
            this.name = name;
            this.id = id;
            this.description = description;
            this.sprite = sprite;
            this.stackable = stackable;
            this.count = count;
            this.cooldown = cooldown;
            this.autoPickup = autoPickup;
            this.redText = redText;
        }

        protected Item(Item copy) {
            name = copy.name;
            id = copy.id;
            description = copy.description;
            redText = copy.redText;
            sprite = copy.sprite;
            stackable = copy.stackable;
            count = copy.count;
            cooldown = copy.cooldown;
        }

        public bool Equals(Item other) {
            return !ReferenceEquals(other, null) && id == other.id;
        }

        public virtual void Select() { } //Left mouse click

        public virtual void Use() {
            //Right mouse click
            Debug.Log("Using item " + name);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
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

        public virtual string GetTooltipText() {
            return $"<size=32>{name}</size>\n{description}\n<color=red>{redText}</color>";
        }
    }
}