using System;
using UnityEngine;

/// <summary>
/// Simple Min Heap. Allows a payload and priority per element, sorted by priority.
/// </summary>
/// <typeparam name="T"></typeparam>
public class MinHeap<T> where T : IComparable<T> {
	private T[] elements;
	public int size { get; set; }

	/// <summary>
	/// Constructor. Accepts the initial capacity of the array for the heap.
	/// </summary>
	/// <param name="InitialCapacity"></param>
	public MinHeap(int InitialCapacity) {
		elements = new T[InitialCapacity];
		this.size = 0;
	}

	private int GetLeftChildIndex(int elemIndex) => 2 * elemIndex + 1;
	private int GetRightChildIndex(int elemIndex) => 2 * elemIndex + 2;
	private int GetParentIndex(int elemIndex) => (elemIndex - 1) / 2;

	private bool HasLeftChild(int elemIndex) => GetLeftChildIndex(elemIndex) < size;
	private bool HasRightChild(int elemIndex) => GetRightChildIndex(elemIndex) < size;
	private bool IsRoot(int elemIndex) => elemIndex == 0;

	private int Compare(int i1, int i2) => elements[i1].CompareTo(elements[i2]);

	private void Swap(int firstIndex, int secondIndex) {
		var temp = elements[firstIndex];
		elements[firstIndex] = elements[secondIndex];
		elements[secondIndex] = temp;
	}

	/// <summary>
	/// Returns boolean value based on if heap has content.
	/// </summary>
	/// <returns>True if empty, false otherwise.</returns>
	public bool IsEmpty() => size == 0;
	private void EmptyCheck() {
		if (IsEmpty()) {
			throw new IndexOutOfRangeException();
		}
	}

	/// <summary>
	/// Returns the payload of the minimum element.
	/// </summary>
	/// <returns>The payload of the minimum element</returns>
	public T Peek() {
		EmptyCheck();
		return elements[0];
	}

	/// <summary>
	/// Returns the payload of the minimum element, then removes the minimum element.
	/// </summary>
	/// <returns>The payload of the minimum element.</returns>
	public T Pop() {
		EmptyCheck();
		T result = elements[0];

		elements[0] = elements[size - 1];
		size--;

		ReHeapifyDown();
		return result;
	}

	/// <summary>
	/// Adds a new element to the heap at the proper place given its priority.
	/// </summary>
	/// <param name="newElem">The element to store.</param>
	/// <param name="priority">The priority with which to store the element.</param>
	public void Add(T newElem) {
		if (size >= elements.Length) {
			elements[size - 1] = newElem;
		}
		else {
			elements[size++] = newElem;
		}

		ReHeapifyUp();
	}

	private void ReHeapifyDown() {
		//Start at root
		int index = 0;
		while (HasLeftChild(index)) {
			int lci = GetLeftChildIndex(index);
			int rci = GetRightChildIndex(index);

			//Find which child has smallest priority
			int smallerChild = lci;
			if (HasRightChild(index) && Compare(lci, rci) > 0) {
				smallerChild = rci;
			}

			//If smallest priority child is larger than self's priority, then done
			if (Compare(index, smallerChild) <= 0) {
				return;
			}

			Swap(index, smallerChild);
			index = smallerChild;
		}
	}

	private void ReHeapifyUp() {
		//Start at last node
		int index = this.size - 1;
		//If our priority is smaller than our parent's, need to swap
		while (!IsRoot(index) && Compare(GetParentIndex(index), index) > 0) {
			int parentIndex = GetParentIndex(index);
			Swap(parentIndex, index);
			index = parentIndex;
		}
	}

	public String Verify() {
		for (int i = 0; i < this.size; i++) {
			//If left child does not exist, we are done
			if (2 * i + 1 >= this.size) return null;

			//Check left child
			if (Compare(i, 2 * i + 1) > 0) {
				return String.Format("Heap incorrect: {0} and {1}", elements[i], elements[2 * i + 1]);
			}

			// If right child does not exist, we are done
			if (2 * i + 2 >= this.size) return null;

			//Check right child
			if (Compare(i, 2 * i + 2) > 0) {
				return String.Format("Heap incorrect: {0} and {1}", elements[i], elements[2 * i + 2]);
			}
		}

		return null;
	}

	public override string ToString() {
		string retstr = "";
		for (int i = 0; i < this.size; i++) {
			retstr += string.Format("{0}", elements[i]);
		}
		return retstr;
	}
}