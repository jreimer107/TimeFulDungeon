using UnityEngine;

namespace TimefulDungeon.Items {
    [System.Serializable]
    public class Item {
        // Randomized fields, configured on construction
        public string name;
        public int count;
        public float cooldown;
        
        
        protected readonly ItemTemplate template;
        
        // Template fields, not changeable
        public int id => template.id;
        public string description => template.description;
        public string redText => template.redText;
        public bool stackable => template.stackable;
        public bool autoPickup => template.autoPickup;
        public Sprite sprite => template.sprite;
        public AnimationClip idleClip => template.idleClip;
        public AnimationClip actionClip => template.actionClip;
        public AudioClip soundEffect => template.soundEffect;
        
        public Item(ItemTemplate template) {
            this.template = template;
            name = template.name;
            count = template.count;
            cooldown = template.cooldown;
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
        
        public bool ToBool() {
            return !ReferenceEquals(this, null);
        }

        public virtual string GetTooltipText() {
            return $"<size=32>{name}</size>\n{description}\n<color=red>{redText}</color>";
        }
    }
}