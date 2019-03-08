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
	public Hall(Room startRoom, Room endRoom, Floor.TileType[][] tilegrid) {
		connectedRooms = new List<Room> { startRoom, endRoom };
		pathCoords = ShortestPath(startRoom, endRoom, tilegrid);
	}


	private static List<Coordinate> ShortestPath(Room startRoom, Room endRoom, Floor.TileType[][] tilegrid) {
		//Get random endpoints outside of given rooms
		//This way all rooms are obstacles
		Coordinate startPos = startRoom.GetRandEntrance();
		Coordinate endPos = endRoom.GetRandEntrance();


		//Coordinates being considered to find the closest path
		List<Coordinate> open = new List<Coordinate>();
		//Coordinates that have already been considered and do not have to be considered again
		List<Coordinate> closed = new List<Coordinate>();

		//Add starting position to closed list
		open.Add(startPos);

		int G = 1;
		Coordinate currPos; //tile to analyze
		do {
			currPos = open[0];
			open.RemoveAt(0); //Get square with lowest F
			closed.Add(currPos);    //Switch square from open to closed list

			if (currPos.Equals(endPos)) {
				//We've added destination to the closed list, found a path
				break;
			}

			//Get valid successors. To be valid must not form a box with a path or room.
			List<Coordinate> successors = currPos.GetValidSuccessors(tilegrid);
			foreach (Coordinate suc in successors) {
				//Compute F score of analyzed tile
				suc.F = G + Math.Abs(endPos.x - suc.x) + Math.Abs(endPos.y - suc.y);
				if (tilegrid[suc.x][suc.y] != Floor.TileType.Path) { //try to reuse paths
					suc.F += 7;
				}
				suc.parent = currPos;


				//Redo a* in one iteration through open
				//Keep track of positions to insert new node and remove old one
				int insertPos = -1;
				int removePos = -1;

				//Check if open already contains successor
				for (int i = 0; i < open.Count; i++) {
					//Only consider section of list with higher F values
					if (suc.F < open[i].F) {
						//By being here we've found a spot to insert
						if (insertPos == -1) {
							insertPos = i;
						}

						//If our node already in list, need to remove it as we have a better path
						//Due to parent condition, it is not possible to remove a better node
						if (suc.Equals(open[i])) {
							removePos = i;
							break;
						}
					}
				}

				//Edit list based on found positions
				if (insertPos != -1) {
					//If we're not inserting then we're not removing
					if (removePos != -1) {
						open.RemoveAt(removePos);
					}
					//Insert second so as not to mess up removePos
					open.Insert(insertPos, suc);
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


