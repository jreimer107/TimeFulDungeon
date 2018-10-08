using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomList : MonoBehaviour {
	public List<Room> list;

	public RoomList() {
		list = new List<Room>();
	}


	/// <summary>
	/// Adds a new Room to the room list if it fits in the floor grid.
	/// </summary>
	/// <param name="newRoom">The new room to be added.</param>
	/// <returns>True if the room was successfully added, false otherwise.</returns>
	public bool AddRoom(Room newRoom) {
		bool space_taken = false;
		foreach (Room room_ in list) {
			if (CheckRoomOverlap(newRoom, room_)) {
				space_taken = true;
				break;
			}
		}
		if (!space_taken) {
			list.Add(newRoom);
			return true;
		}
		else return false;
	}

	
	/// <summary>
	/// Checks to see if two rooms are overlapping, including the extra gap between them.
	/// </summary>
	/// <param name="r1">The first room to compare.</param>
	/// <param name="r2">The second room to compare.</param>
	/// <returns>True if the rooms overlap or are within the GAP of each other, false otherwise.</returns>
	public bool CheckRoomOverlap(Room r1, Room r2) {
		return !(r1.LeftBound > r2.RightSpace) &&
			   !(r1.RightBound < r2.LeftSpace) &&
			   !(r1.UpperBound > r2.LowerSpace) &&
			   !(r1.LowerBound < r2.UpperSpace);
		}
	}
}
