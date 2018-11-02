using System;
using System.Collections.Generic;
using Random = System.Random;
using UnityEngine;

public class Path {
	public List<Coordinate> pathCoords;
	public Room startRoom;
	public Room endRoom;
	public Coordinate startPos;
	public Coordinate endPos;
	public List<Room> connectedRooms;


	public enum Direction {
		Right, Up, Left, Down,
	}


	//Trying out this fancy A* pathfinding!
	//So we want to path from one random room to another
	//Avoid other rooms as obstacles to cause more interesting behavior
	//Idea - avoid all rooms
	public Path(Room startRoom, Room endRoom, Floor.TileType[][] tilegrid) {
		pathCoords = new List<Coordinate>();
		connectedRooms = new List<Room>();
		pathCoords = ShortestPath(startRoom, endRoom, tilegrid);
	}

 
	private static List<Coordinate> ShortestPath(Room startRoom, Room endRoom, Floor.TileType[][] tilegrid) {
		List<Coordinate> pathCoords = new List<Coordinate>();
		Coordinate currPos;

		//Get random start points
		Coordinate startPos = startRoom.GetRandCoordinate();
		Coordinate endPos = endRoom.GetRandCoordinate();
		Debug.Log(string.Format("Travelling from ({0},{1}) to ({2},{3})", startPos.x, startPos.y, endPos.x, endPos.y));


		//Coordinates being considered to find the closest path
		List<Coordinate> openList = new List<Coordinate>();
		//Coordinates that have already been considered and do not have to be considered again
		List<Coordinate> closedList = new List<Coordinate>();

		//Add starting position to closed list
		openList.Add(startPos);

		
		int G = 1;
		do {
			currPos = Coordinate.FindSmallestF(openList); //Get square with lowest F
			closedList.Add(currPos);	//Switch square from open to closed list
			openList.Remove(currPos);
			Debug.Log(string.Format("Checking tile ({0},{1}), with F: {2}", currPos.x, currPos.y, currPos.F));

			if (endPos.IsInList(closedList)) {
				//We've added destination to the closed list, found a path
				Debug.Log("Found endPos in closed list.");
				break;
			}

			//Get adjacent walkable squares
			List<Coordinate> adjacents = new List<Coordinate>();
			Room[] termini = { startRoom, endRoom }; //array of start and end rooms for IsInRooms.

			if (tilegrid[currPos.x][currPos.y] == Floor.TileType.Room && !currPos.IsInRooms(termini)) {
				Debug.Log("ERROR: IN NON-TERMINUS ROOM");
			}

			//Adjacent tiles that may be considered. To be considered they must not be next to a room.
			Coordinate[] one_offs = { currPos.Clone(), currPos.Clone(), currPos.Clone(), currPos.Clone() };
			one_offs[0].x += 1; //East
			one_offs[1].x -= 1; //West
			one_offs[2].y += 1; //North
			one_offs[3].y -= 1; //South

			//Tiles that are one tile more in the direction of the corresponding adjacent tile.
			Coordinate[] two_offs = { currPos.Clone(), currPos.Clone(), currPos.Clone(), currPos.Clone() };
			two_offs[0].x += 2; //East
			two_offs[1].x -= 2; //West
			two_offs[2].y += 2; //North
			two_offs[3].y -= 2; //South

			//To consider a tile, tile must not be next to a room tile or that room tile must be one of the termini.
			//So the tile two out in a given direction must not be in a room, or that room must be one of the termini.
			for (int i = 0; i < two_offs.Length; i++) {
				if (two_offs[i].IsInBounds()) {
					if (tilegrid[two_offs[i].x][two_offs[i].y] != Floor.TileType.Room || two_offs[i].IsInRooms(termini)) {
						if (two_offs[i].IsInRooms(termini)) {
							Debug.Log("Two-off is in terminus room.");
						}
						Debug.Log(string.Format("Adding ({0},{1}) to adjacents.", one_offs[i].x, one_offs[i].y));
						adjacents.Add(one_offs[i]);
					}
				}
				
			}

			//Now we have the tiles adjancent to the current tile that are not next to a room
			foreach (Coordinate adjCoord in adjacents) {
				if (adjCoord.IsInList(closedList)) { //If adj tile is already considered, ignore it
					continue;
				}
				int newF = G + (Math.Abs(endPos.x - adjCoord.x) + Math.Abs(endPos.y - adjCoord.y));
				//Debug.Log(string.Format("New F for ({0},{1}): {2}", adjCoord.x, adjCoord.y, newF));
				if (!adjCoord.IsInList(openList)) { //not in open list
					//Debug.Log(string.Format("({0},{1}) not found in openList, adding", adjCoord.x, adjCoord.y));
					//Compute its F score, set its parent, and add it to the list
					adjCoord.F = newF;
					adjCoord.parent = currPos;
					openList.Add(adjCoord);
				}
				else { //In open list
					//check if current G score makes score lower, if yes update bc found better path
					if (newF < adjCoord.F) {
						adjCoord.F = newF;
						adjCoord.parent = currPos;
					}
				}
			}
			adjacents.Clear();

			G++;
			//break;
			//if (G > 200) break;
		} while (openList.Count != 0);
		//Debug.Log("Found path.");

		//Now start at end and work backward through parents
		//currPos = endPos;
		while(currPos != null) {
			pathCoords.Add(currPos.Clone());
			//Debug.Log(string.Format("Added ({0},{1}) to path coords.", currPos.x, currPos.y));
			currPos = currPos.parent;
		}
		return pathCoords;
	}
}


