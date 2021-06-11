using System;
using System.IO;
using System.Runtime.Serialization;
using TimefulDungeon.Misc;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TimefulDungeon.Items {
    [Serializable]
    public class Item : ISerializationSurrogate {
        private const float RandomizationRange = 0.25f;
        protected const string FLOAT_FORMAT = "F1";
        
        public int count;
        
        [SerializeField] protected ItemTemplate template;

        private string _toolTipText;

        public Item(ItemTemplate template) {
            this.template = template;
            count = template.count;
        }

        // Template fields, not changeable
        public string Name => template.name;
        public float Cooldown => template.cooldown;
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
            Debug.Log("Using item " + Name);
        }

        public override int GetHashCode() {
            return Name.GetHashCode();
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
            return $"<size=32>{Name}</size>\n{Description}\n<color=red>{RedText}</color>";
        }

        public string GetTooltipText() {
            return _toolTipText ??= CalculateTooltipText();
        }

        protected string GetFormattedRedText() {
            return RedText != "" ? $"<color=red>{Translations.Get(RedText)}</color>\n" : "";
        }

        public void Serialize() {
            var path = Application.persistentDataPath + "/" + Name + ".item";
            File.WriteAllText(path, JsonUtility.ToJson(this));
        }
        
        public static Item Deserialize(string json) {
            var dummyItem = JsonUtility.FromJson<Item>(json);
            var realItem = dummyItem.template.GetInstance();

            return realItem;
            // var path = Application.persistentDataPath + "/" + Name + ".item";
            // Debug.Log("Wrote file to " + path);
            // JsonUtility.FromJsonOverwrite(File.ReadAllText(path), this);
        }

        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context) { }

        public virtual object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector) {
            var dummyItem = (Item) obj;
            var realItem = dummyItem.template.GetInstance();
            realItem.count = (int) info.GetValue("count", typeof(int));
            return realItem;
        }
    }
}