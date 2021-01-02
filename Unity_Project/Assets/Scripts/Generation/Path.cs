using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path {
	public HashSet<Coordinate> pathCoords;
	public HashSet<Room> connectedRooms;

	public Path(Room startRoom, Room endRoom) {
		connectedRooms = new HashSet<Room> { startRoom, endRoom };
		//pathCoords = ShortestPath(startRoom, endRoom);
		Generate(startRoom, endRoom);
	}

	public void Generate(Room startRoom, Room endRoom) {
		Room[] endpoints = { startRoom, endRoom };
		Coordinate startPos = startRoom.GetRandCoordinate();
		Coordinate endPos = endRoom.GetRandCoordinate();

		float GetGValue(Coordinate suc, Coordinate curr, Coordinate parent, float currCost) {
			//Get G value, adjust to reuse paths and to continue in same direction
			float newCost = currCost + 2;
			if (!Board.instance.IsTileOfType(suc, TileType.Path)) {
				newCost += 7;
			}
			if (parent != null) {
				if (parent.x == curr.x && curr.x != suc.x ||
					 parent.y == curr.y && curr.y != suc.y) {
					newCost++;
				}
			}
			return newCost;
		}
		Func<Coordinate, Coordinate, Coordinate, float, float> constFunc = GetGValue;
		Func<Coordinate, Coordinate, Coordinate[]> successorsFunction = (Coordinate curr, Coordinate parent) => {
			return Coordinate.GetValidSuccessorsForPathGen(curr, parent, endpoints);
		};

		float time = Time.realtimeSinceStartup;
		this.pathCoords = Board.instance.GetShortestPath(
			startPos, endPos,
			successorsFunction,
			constFunc,
			Coordinate.heuristic
		);
		// Debug.LogFormat("OOP pathing: {0}", (Time.realtimeSinceStartup - time) * 1000f);
	}

	int GetGValue(Coordinate suc, Coordinate curr, Coordinate parent, int currCost) {
		//Get G value, adjust to reuse paths and to continue in same direction
		int newCost = currCost + 2;
		if (!Board.instance.IsTileOfType(suc, TileType.Path)) {
			newCost += 7;
		}
		if (parent != null) {
			if (parent.x == curr.x && curr.x != suc.x ||
				 parent.y == curr.y && curr.y != suc.y) {
				newCost++;
			}
		}
		return newCost;
	}

	public bool IntersectAndAbsorb(Path other) {
		//Create hashset union of both connected rooms
		HashSet<Coordinate> coordsUnion = new HashSet<Coordinate>(this.pathCoords);
		HashSet<Room> roomsUnion = new HashSet<Room>(this.connectedRooms);
		coordsUnion.UnionWith(other.pathCoords);
		roomsUnion.UnionWith(other.connectedRooms);

		//If different count in result, at least one room/tile shared, so paths intersect
		if (coordsUnion.Count != this.pathCoords.Count + other.pathCoords.Count ||
			roomsUnion.Count != this.connectedRooms.Count + other.connectedRooms.Count) {
			this.pathCoords = coordsUnion;
			this.connectedRooms = roomsUnion;
			return true;
		}
		return false;
	}

}