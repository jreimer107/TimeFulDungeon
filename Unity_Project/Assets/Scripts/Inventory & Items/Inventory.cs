using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour {
	public int maxSlots;
	public int enabledSlots;

	public List<Item> Bag;

	#region Singleton
	public static Inventory instance;
	void Awake() {
		if (instance != null) {
			Debug.LogWarning("More than one instance of inventory found.");
		}
		instance = this;
	}
	#endregion

	//Event to subscribe to for when the inventory contents changes.
	public delegate void OnItemChanged();
	public OnItemChanged onItemChangedCallback;

	void Start() {
		Bag = new List<Item>();
	}

	//Checks if 
	public bool CanAdd(Item newItem) {
		if (HasSpace)
			return true;
		else if (newItem.stackable) {
			foreach (Item item in Bag) {
				if (item == newItem) {
					return true;
				}
			}
		}
		return false;
	}

	//Adds an item to the inventory, if possible.
	public bool Add(Item newItem) {
		//If item is stackable, check if we already have some
		if (newItem.stackable) {
			foreach (Item item in Bag) {
				if (item.ID == newItem.ID) {
					item.count += newItem.count;
					if (onItemChangedCallback != null)
						onItemChangedCallback.Invoke();
					return true;
				}
			}
		}

		//If we have space, add item in new slot
		if (HasSpace) {
			Bag.Add(newItem);
			Debug.Log("Added new item to inventory.");
			if (onItemChangedCallback != null)
				onItemChangedCallback.Invoke();
			return true;
		}

		//Couldn't stack and couldn't fit, so can't add to inventory.
		return false;
	}

	public void Remove(Item item) {
		Bag.Remove(item);
		if (onItemChangedCallback != null)
			onItemChangedCallback.Invoke();
	}

	//Swaps the position of two items within the inventory.
	public void Swap(int to, int from) {
		if (to >= Bag.Count)
			to = Bag.Count - 1;
		if (from >= Bag.Count)
			from = Bag.Count - 1;

		Item temp = Bag[to];
		Bag[to] = Bag[from];
		Bag[from] = temp;
		if (onItemChangedCallback != null)
			onItemChangedCallback.Invoke();
	}

	//Swaps an item inside the inventory for one outside.
	public void SwapOut(Item newItem, Item oldItem) {
		Bag[Bag.IndexOf(oldItem)] = newItem;
		if (onItemChangedCallback != null)
			onItemChangedCallback.Invoke();
	}

	public bool HasSpace {
		get {
			return Bag.Count < enabledSlots;
		}
	}
}