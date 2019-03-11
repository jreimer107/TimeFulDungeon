using System;
using System.Collections;
using System.Collections.Generic;

public class Hall {
	public List<Coordinate> pathCoords;
	public List<Room> connectedRooms;


	public enum Direction {
		Right, Up, Left, Down,
	}


	//Trying out this fancy A* pathfinding!
	//So we want to path from one random room to another
	//Avoid other rooms as obstacles to cause more interesting behavior
	public Hall(Room startRoom, Room endRoom, Floor.TileType[,] tilegrid) {
		connectedRooms = new List<Room> { startRoom, endRoom };
		pathCoords = ShortestPath(startRoom, endRoom, tilegrid);
	}


	private static List<Coordinate> ShortestPath(Room startRoom, Room endRoom, Floor.TileType[,] tilegrid) {
		//Get random endpoints outside of given rooms
		//This way all rooms are obstacles
		Coordinate startPos = startRoom.GetRandEntrance();
		Coordinate endPos = endRoom.GetRandEntrance();

		Dictionary<Coordinate, Coordinate> parents = new Dictionary<Coordinate, Coordinate>();
		Dictionary<Coordinate, int> costs = new Dictionary<Coordinate, int>();


		//Coordinates being considered to find the closest path
		SortedSet<Tuple<Coordinate, int>> open = new SortedSet<Tuple<Coordinate, int>>(new Coordinate.CoordinateFComparer());
		//Coordinates that have already been considered and do not have to be considered again
		HashSet<Coordinate> closed = new HashSet<Coordinate>();

		//Add starting position to closed list
		open.Add(Tuple.Create(startPos, 0));
		parents[startPos] = null;
		costs[startPos] = 0;
		Coordinate currPos = null;   //tile to analyze
		while (open.Count > 0) {
			Tuple<Coordinate, int> currTup = open.Min; //Get square with lowest F
			currPos = currTup.Item1;
			open.Remove(currTup);
			closed.Add(currPos);    //Switch square from open to closed list

			if (currPos.Equals(endPos)) {
				//We've added destination to the closed list, found a path
				break;
			}

			//Get valid successors. To be valid must not form a 2x2 box with anything.
			List<Coordinate> successors = currPos.getSuccessors(tilegrid);
			successors.RemoveAll(x => closed.Contains(x));
			foreach (Coordinate suc in successors) {
				//Get G value, adjust to reuse paths
				int newCost = costs[currPos] + ((tilegrid[suc.x, suc.y] != Floor.TileType.Path) ? 7 : 1);

				//Only edit dictionaries if node is new or better
				if (!costs.ContainsKey(suc) || newCost < costs[suc]) {
					//Add node to open with F based on G and H values as priority
					open.Add(Tuple.Create(suc, newCost + Coordinate.heuristic(suc, endPos)));
					costs[suc] = newCost;
					parents[suc] = currPos;
				}
			}
		}

		//Now start at end and work backward through parents
		List<Coordinate> pathCoords = new List<Coordinate>();
		while (currPos != null) {
			pathCoords.Add(currPos);
			currPos = parents[currPos];
		}
		return pathCoords;
	}

	public bool Intersects(Hall other) {
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
}