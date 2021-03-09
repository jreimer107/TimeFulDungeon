using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace TimefulDungeon.Core {
    [Serializable]
    public class Backpack {
        [SerializeField] private List<Item> bag = new List<Item>();
        [SerializeField] private UnityEvent<int> onBackpackChanged;
        [SerializeField] public int enabledSlots;
        
        private bool HasSpace => bag.Count < enabledSlots;

        public bool CanAdd(Item newItem) {
            return HasSpace || (newItem.stackable && bag.Any(item => item == newItem));
        }

        //Adds an item to the inventory, if possible.
        public bool Add(Item newItem) {
            //If item is stackable, check if we already have some
            if (newItem.stackable)
                for (var i = 0; i < Count; i++) {
                    if (bag[i] != newItem) continue;
                    bag[i].count += newItem.count;
                    onBackpackChanged.Invoke(i);
                }

            //Couldn't stack and couldn't fit, so can't add to inventory.
            if (!HasSpace) return false;

            //If we have space, add item in new slot
            bag.Add(newItem);
            onBackpackChanged.Invoke(Count - 1);
            return true;
        }

        public bool Remove(Item item) {
            var index = bag.IndexOf(item);
            if (index == -1) return false;
            bag.RemoveAt(index);
            onBackpackChanged.Invoke(index);
            return true;
        }

        public bool RemoveAt(int index) {
            if (index >= Count) return false;
            bag.RemoveAt(index);
            return true;
        }

        //Swaps the position of two items within the inventory.
        public void Swap(int to, int from) {
            if (to >= bag.Count)
                to = bag.Count - 1;
            if (from >= bag.Count)
                from = bag.Count - 1;

            var temp = bag[to];
            bag[to] = bag[from];
            bag[from] = temp;
            onBackpackChanged.Invoke(to);
            onBackpackChanged.Invoke(from);
        }

        //Swaps an item inside the inventory for one outside.
        public void SwapOut(Item newItem, Item oldItem) {
            var index = bag.IndexOf(oldItem);
            bag[index] = newItem;
            onBackpackChanged.Invoke(index);
        }

        public Item GetItem(int index) => index < bag.Count ? bag[index] : null;
        public int Count => bag.Count;
    }
}