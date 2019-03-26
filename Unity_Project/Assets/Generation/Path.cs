using System;
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
	public Hall(Room startRoom, Room endRoom, Floor.TileType[][] tilegrid) {
		connectedRooms = new List<Room> { startRoom, endRoom };
		pathCoords = ShortestPath(startRoom, endRoom, tilegrid);
	}

 
	private static List<Coordinate> ShortestPath(Room startRoom, Room endRoom, Floor.TileType[][] tilegrid) {
		//Get random start points
		Coordinate startPos = startRoom.GetRandCoordinate();
		Coordinate endPos = endRoom.GetRandCoordinate();


		//Coordinates being considered to find the closest path
		List<Coordinate> openList = new List<Coordinate>();
		//Coordinates that have already been considered and do not have to be considered again
		List<Coordinate> closedList = new List<Coordinate>();

		//Add starting position to closed list
		openList.Add(startPos);

		int G = 1;
		Coordinate currPos; //tile to analyze
		do {
			currPos = Coordinate.FindSmallestF(openList); //Get square with lowest F
			closedList.Add(currPos);	//Switch square from open to closed list
			openList.Remove(currPos);

			if (endPos.IsInList(closedList)) {
				//We've added destination to the closed list, found a path
				break;
			}

			//Get adjacent walkable squares
			Room[] termini = { startRoom, endRoom }; //array of start and end rooms for IsInRooms.

			//Adjacent tiles that may be considered. To be considered they must not be next to a non-terminus room.
			List<Coordinate> adjacents = currPos.GetAdjacents();
			adjacents.RemoveAll(x => x.IsAdjacentToRoom(tilegrid, termini, currPos) || x.IsAdjacentToPath(tilegrid, currPos) || x.IsInList(closedList));
			foreach (Coordinate adj in adjacents) {
				int newF = G + Math.Abs(endPos.x - adj.x) + Math.Abs(endPos.y - adj.y);
				if (tilegrid[adj.x][adj.y] != Floor.TileType.Path) { //try to reuse paths
					newF += 7;
				}
				if (!adj.IsInList(openList)) { //not in open list
					//Compute its F score, set its parent, and add it to the list
					adj.F = newF;
					adj.parent = currPos;
					openList.Add(adj);
				}
				else { //In open list
					//check if current G score makes score lower, if yes update bc found better path
					if (newF < adj.F) {
						adj.F = newF;
						adj.parent = currPos;
					}
				}
			}

			G++;
		} while (openList.Count != 0);

		//Now start at end and work backward through parents
		List<Coordinate> pathCoords = new List<Coordinate>();
		while (currPos != null) {
			pathCoords.Add(currPos);
			currPos = currPos.parent;
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
			if (!c.IsInList(pathCoords)) {
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


