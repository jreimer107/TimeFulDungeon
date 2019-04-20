using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hall {
	public HashSet<Coordinate> pathCoords;
	public HashSet<Room> connectedRooms;

	public Hall(Room startRoom, Room endRoom, Floor.TileType[, ] tilegrid) {
		connectedRooms = new HashSet<Room> { startRoom, endRoom };
		pathCoords = ShortestPath(startRoom, endRoom, tilegrid);
	}

	//So we want to path from one random room to another
	//Avoid other rooms as obstacles to cause more interesting behavior
	private static HashSet<Coordinate> ShortestPath(Room startRoom, Room endRoom, Floor.TileType[, ] tilegrid) {
		//Get random endpoints outside of given rooms
		//This way all rooms are obstacles
		Coordinate startPos = startRoom.GetRandEntrance();
		Coordinate endPos = endRoom.GetRandEntrance();

		Dictionary<Coordinate, Coordinate> parents = new Dictionary<Coordinate, Coordinate>();
		Dictionary<Coordinate, int> costs = new Dictionary<Coordinate, int>();

		//Coordinates being considered to find the closest path
		MinHeap<PathNode> open = new MinHeap<PathNode>(100);
		//Coordinates that have already been considered and do not have to be considered again
		HashSet<Coordinate> closed = new HashSet<Coordinate>();

		//Add starting position to closed list
		open.Add(new PathNode(startPos, Coordinate.heuristic(startPos, endPos)));
		parents[startPos] = null;
		costs[startPos] = 0;
		Coordinate currPos = null; //tile to analyze
		while (!open.IsEmpty()) {
			currPos = open.Pop().pos;
			closed.Add(currPos); //Switch square from open to closed list

			//We've added destination to the closed list, found a path
			if (currPos.Equals(endPos)) {
				break;
			}

			//Get valid successors. To be valid must not form a 2x2 box with anything.
			List<Coordinate> successors = currPos.getSuccessors(tilegrid);
			foreach (Coordinate suc in successors) {
				if (closed.Contains(suc)) {
					continue;
				}

				//Get G value, adjust to reuse paths
				int newCost = costs[currPos] + ((tilegrid[suc.x, suc.y] != Floor.TileType.Path) ? 7 : 1);

				//Only edit dictionaries if node is new or better
				if (!costs.ContainsKey(suc) || newCost < costs[suc]) {
					//Add node to open with F based on G and H values as priority
					open.Add(new PathNode(suc, newCost + Coordinate.heuristic(suc, endPos)));
					costs[suc] = newCost;
					parents[suc] = currPos;
				}
			}
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

		//If different count in result, at least one room shared, so paths intersect
		if (coordsUnion.Count != this.pathCoords.Count + other.pathCoords.Count ||
			roomsUnion.Count != this.connectedRooms.Count + other.connectedRooms.Count) {
			this.pathCoords = coordsUnion;
			this.connectedRooms = roomsUnion;
			return true;
		}
		return false;
	}

	public bool Intersects(Hall other) {
		// HashSet<Coordinate> temp = new HashSet<Coordinate>(this.pathCoords);
		// temp.IntersectWith(other.pathCoords);
		// if (temp.Count > 0) {
		// 	return true;
		// }
		foreach (Coordinate c in pathCoords) {
			if (other.pathCoords.Contains(c)) {
				return true;
			}
		}
		return false;
	}

	public void Absorb(Hall other) {
		//Coalesce coordinates into one list
		foreach (Coordinate c in other.pathCoords) {
			if (!pathCoords.Contains(c)) {
				pathCoords.Add(c);
			}
		}

		//Coalesce connected rooms (termini) into one list
		foreach (Room r in other.connectedRooms) {
			if (!connectedRooms.Contains(r)) {
				connectedRooms.Add(r);
			}
		}
	}

	public bool ShareEndpoint(Hall other) {
		foreach (Room r in connectedRooms) {
			if (other.connectedRooms.Contains(r)) {
				return true;
			}
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
			}
			else {
				return this.pos.CompareTo(other.pos);
			}
		}

		public override string ToString() {
			return string.Format("[{0}, {1}]", this.pos, this.heuristic);
		}
	}
}