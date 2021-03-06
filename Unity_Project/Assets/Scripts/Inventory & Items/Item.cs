using System;
using UnityEngine;

namespace TimefulDungeon {
	[CreateAssetMenu(fileName = "New Item", menuName = "Interactables/Item")]
	public class Item : ScriptableObject, IEquatable<Item> {
		new public string name;
		public int ID;
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

		public Item(string name, int ID, string description, Sprite sprite, bool stackable, int count, float cooldown, bool autoPickup, string redText = "") {
			this.name = name;
			this.ID = ID;
			this.description = description;
			this.sprite = sprite;
			this.stackable = stackable;
			this.count = count;
			this.cooldown = cooldown;
			this.autoPickup = autoPickup;
		}

		public virtual void Select() { } //Left mouse click
		public virtual void Use() { //Right mouse click
			Debug.Log("Using item " + name);
		}

		public void RemoveFromInventory() => Inventory.instance.Remove(this);

		public override int GetHashCode() => base.GetHashCode();
		public bool Equals(Item other) => !ReferenceEquals(other, null) && this.ID == other.ID;

		public override bool Equals(object other) => other is Item item && this.Equals(item);
		public static bool operator ==(Item a, Item b) => ReferenceEquals(a, b) || a.Equals(b);
		public static bool operator !=(Item a, Item b) => !ReferenceEquals(a, null) && !a.Equals(b);
		public static bool operator !(Item a) => ReferenceEquals(a, null);
		public static bool operator true(Item a) => !ReferenceEquals(a, null);
		public static bool operator false(Item a) => ReferenceEquals(a, null);

		protected Item(Item copy) {
			this.name = copy.name;
			this.ID = copy.ID;
			this.description = copy.description;
			this.redText = copy.redText;
			this.sprite = copy.sprite;
			this.stackable = copy.stackable;
			this.count = copy.count;
			this.cooldown = copy.cooldown;
		}

		public virtual Item Clone() => new Item(this);

		public virtual string GetTooltipText() => $"<size=32>{name}</size>\n{description}\n<color=red>{redText}</color>";
	}
}
