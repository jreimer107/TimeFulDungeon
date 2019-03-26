using System;

/// <summary>
/// Simple Min Heap. Allows a payload and priority per element, sorted by priority.
/// </summary>
/// <typeparam name="T"></typeparam>
public class MinHeap<T> where T : IComparable<T> {
	private Tuple<T, int>[] elements;
	public int size { get; set; }

	/// <summary>
	/// Constructor. Accepts the initial capacity of the array for the heap.
	/// </summary>
	/// <param name="InitialCapacity"></param>
	public MinHeap(int InitialCapacity) {
		elements = new Tuple<T, int>[InitialCapacity];
		this.size = 0;
	}

	private int GetLeftChildIndex(int elemIndex) => 2 * elemIndex;
	private int GetRightChildIndex(int elemIndex) => 2 * elemIndex + 1;
	private int GetParentIndex(int elemIndex) => elemIndex / 2;

	private bool HasLeftChild(int elemIndex) => GetLeftChildIndex(elemIndex) < size;
	private bool HasRightChild(int elemIndex) => GetRightChildIndex(elemIndex) < size;
	private bool IsRoot(int elemIndex) => elemIndex == 0;

	private T GetLeftChild(int elemIndex) => elements[GetLeftChildIndex(elemIndex)].Item1;
	private T GetRightChild(int elemIndex) => elements[GetRightChildIndex(elemIndex)].Item1;
	private T GetParent(int elemIndex) => elements[GetParentIndex(elemIndex)].Item1;

	private int GetLeftChildPriority(int elemIndex) => elements[GetLeftChildIndex(elemIndex)].Item2;
	private int GetRightChildPriority(int elemIndex) => elements[GetRightChildIndex(elemIndex)].Item2;
	private int GetParentPriority(int elemIndex) => elements[GetParentIndex(elemIndex)].Item2;
	private int GetSelfPriority(int elemIndex) => elements[elemIndex].Item2;

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
		return elements[0].Item1;
	}

	/// <summary>
	/// Returns the payload of the minimum element, then removes the minimum element.
	/// </summary>
	/// <returns>The payload of the minimum element.</returns>
	public T Pop() {
		EmptyCheck();
		T result = elements[0].Item1;

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
	public void Add(T newElem, int priority) {
		if (size >= elements.Length) {
			//throw new IndexOutOfRangeException();
			return;
		}
		elements[size] = Tuple.Create(newElem, priority);
		size++;

		ReHeapifyUp();
	}

	private void ReHeapifyDown() {
		//Start at root
		int index = 0;
		while (HasLeftChild(index)) {
			//Find which child has smallest priority
			int smallerIndex = GetLeftChildIndex(index);
			if (HasRightChild(index) && GetRightChildPriority(index) < (GetLeftChildPriority(index))) {
				smallerIndex = GetRightChildIndex(index);
			}

			//If smallest priority child is larger than self's priority, then done
			if (GetSelfPriority(smallerIndex) >= GetSelfPriority(index)) {
				return;
			}

			//Else self's priority is larger than a child's, swap those elements.
			Swap(smallerIndex, index);
			index = smallerIndex;
		}
	}

	private void ReHeapifyUp() {
		//Start at last node
		int index = this.size - 1;
		//If our priority is smaller than our parent's, need to swap
		while (!IsRoot(index) && GetSelfPriority(index) < GetParentPriority(index)) {
			int parentIndex = GetParentIndex(index);
			Swap(parentIndex, index);
			index = parentIndex;
		}
	}
}