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

		//Coordinates being considered to find the closest path
		SortedSet<Coordinate> open = new SortedSet<Coordinate>(Coordinate.CoordinateFComparer);
		//Coordinates that have already been considered and do not have to be considered again
		HashSet<Coordinate> closed = new HashSet<Coordinate>();

		//Add starting position to closed list
		open.Add(startPos);

		int G = 1;
		Coordinate currPos; //tile to analyze
		do {
			currPos = open.Min; //Get square with lowest F
			open.Remove(currPos);
			closed.Add(currPos);    //Switch square from open to closed list

			if (currPos.Equals(endPos)) {
				//We've added destination to the closed list, found a path
				break;
			}

			//Get valid successors. To be valid must not form a 2x2 box with anything.
			List<Coordinate> successors = currPos.getSuccessors(tilegrid);
			successors.RemoveAll(x => closed.Contains(x));
			foreach (Coordinate suc in successors) {
				//Compute F score of analyzed tile
				suc.F = G + Math.Abs(endPos.x - suc.x) + Math.Abs(endPos.y - suc.y);
				if (tilegrid[suc.x, suc.y] != Floor.TileType.Path) { //try to reuse paths
					suc.F += 7;
				}


				Coordinate existing;
				if (open.TryGetValue(currPos, existing)) {
					if (currPos.F < existing.F) {
						open.Remove(existing);
						open.Add(currPos);
					}
				} else {
					open.Add(currPos);
				}


				//Find place to put new node and place where it already exists
				bool added = false;
				bool found = false;
				LinkedListNode<Coordinate> check = open.First;
				while (check) {
					//Only look at part of list with higher F values
					if (suc.F < check.Value.F) {
						//By being here we have found a spot, add if we haven't already
						if (!added) {
							open.AddBefore(check, suc);
							added = true;
						}

						//If we find our node, remove it as we have a better one
						if (suc.Equals(check.Value)) {
							open.Remove(check);
							found = true;
							break;
						}
					}
					check = check.Next;
				}

				//If we didn't add or find our node, then it should be at the end
				if (!added && !found) {
					open.AddLast(suc);
				}
			}
			G++;
		} while (open.Count != 0);

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


