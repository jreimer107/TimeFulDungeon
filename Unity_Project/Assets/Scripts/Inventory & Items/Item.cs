using UnityEngine;
using System;

[CreateAssetMenu(fileName = "New Item", menuName = "Interactables/Item")]
public class Item : ScriptableObject, IEquatable<Item> {
	new public string name;
	public int ID;
	public string description;
	public Sprite sprite;
	public bool stackable;
	public int count;

	public Item(string name, int ID, string description, Sprite sprite, bool stackable, int count) {
		this.name = name;
		this.ID = ID;
		this.description = description;
		this.sprite = sprite;
		this.stackable = stackable;
		this.count = count;
	}

	public virtual void Select() { } //Left mouse click
	public virtual void Use() { //Right mouse click
		Debug.Log("Using item " + name);
	}

	public void RemoveFromInventory() => Inventory.instance.Remove(this);

	public bool Equals(Item other) => this.ID == other.ID;

	protected Item(Item copy) {
		this.name = copy.name;
		this.ID = copy.ID;
		this.description = copy.description;
		this.sprite = copy.sprite;
		this.stackable = copy.stackable;
		this.count = copy.count;
	}

	public virtual Item Clone() => new Item(this);
}
