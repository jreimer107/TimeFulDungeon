using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hall {
	public HashSet<Coordinate> pathCoords;
	public HashSet<Room> connectedRooms;

	public Hall(Room startRoom, Room endRoom, Floor.TileType[,] tilegrid) {
		connectedRooms = new HashSet<Room> { startRoom, endRoom };
		pathCoords = ShortestPath(startRoom, endRoom, tilegrid);
	}

	//So we want to path from one random room to another
	//Avoid other rooms as obstacles to cause more interesting behavior
	private static HashSet<Coordinate> ShortestPath(Room startRoom, Room endRoom, Floor.TileType[,] tilegrid) {
		//Get random endpoints outside of given rooms
		//This way all rooms are obstacles
		Room[] endpoints = {startRoom, endRoom};
		Coordinate startPos = startRoom.GetRandEntrance();
		Coordinate endPos = endRoom.GetRandEntrance();
		string debugStr = "";
		debugStr += $"Start: {startPos}; End: {endPos}\n";
		//Debug.Log(debugStr);

		Dictionary<Coordinate, Coordinate> parents = new Dictionary<Coordinate, Coordinate>();
		Dictionary<Coordinate, int> costs = new Dictionary<Coordinate, int>();

		//Coordinates being considered to find the closest path
		MinHeap<PathNode> open = new MinHeap<PathNode>(300);
		//Coordinates that have already been considered and do not have to be considered again
		HashSet<Coordinate> closed = new HashSet<Coordinate>();

		//Add starting position to closed list
		open.Add(new PathNode(startPos, Coordinate.heuristic(startPos, endPos)));
		parents[startPos] = null;
		costs[startPos] = 0;
		Coordinate currPos = null; //tile to analyze
		while (!open.IsEmpty()) {
			//Debug.Log(open);
			currPos = open.Pop().pos;
			closed.Add(currPos); //Switch square from open to closed list

			//We've added destination to the closed list, found a path
			if (currPos.Equals(endPos)) {
				break;
			}

			//Get valid successors. To be valid must not form a 2x2 box with anything.
			// string strsuc = "";
			// foreach (Coordinate suc in successors) {
			// 	strsuc += suc;
			// }
			debugStr += $"Curr: {currPos}\n";
			//Debug.Log(debugStr);
			foreach (Coordinate suc in currPos.getSuccessors()) {
				if (closed.Contains(suc) || !suc.ValidSuccessor(tilegrid, currPos, parents[currPos], endpoints)) {
					continue;
				}

				//Get G value, adjust to reuse paths and to continue in same direction
				int newCost = costs[currPos] + 2;
				if (tilegrid[suc.x, suc.y] != Floor.TileType.Path) {
					newCost += 7;
				}
				if (parents[currPos] != null) {
					if (parents[currPos].x == currPos.x && currPos.x != suc.x ||
						 parents[currPos].y == currPos.y && currPos.y != suc.y) {
						newCost++;
					}
				}

				//Only edit dictionaries if node is new or better
				if (!costs.ContainsKey(suc) || newCost < costs[suc]) {
					//Add node to open with F based on G and H values as priority
					open.Add(new PathNode(suc, newCost + Coordinate.heuristic(suc, endPos)));
					costs[suc] = newCost;
					parents[suc] = currPos;
				}
			}
		}

		if (!currPos.Equals(endPos)) {
			Debug.Log(debugStr);
		}
		//Now start at end and work backward through parents
		HashSet<Coordinate> pathCoords = new HashSet<Coordinate>();
		while (currPos != null) {
			pathCoords.Add(currPos);
			currPos = parents[currPos];
		}
		return pathCoords;
	}

	public bool IntersectAndAbsorb(Hall other) {
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
	internal class PathNode : IComparable<PathNode> {
		public Coordinate pos { get; }
		public int heuristic { get; }

		public PathNode(Coordinate pos, int heuristic) {
			this.pos = pos;
			this.heuristic = heuristic;
		}

		public int CompareTo(PathNode other) {
			if (this.heuristic != other.heuristic) {
				return this.heuristic.CompareTo(other.heuristic);
			} else {
				return this.pos.CompareTo(other.pos);
			}
		}

		public override string ToString() {
			return string.Format("[{0}, {1}]", this.pos, this.heuristic);
		}
	}
}