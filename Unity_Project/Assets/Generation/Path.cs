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

	private static Random rng = new Random();

	public enum Direction {
		Right, Up, Left, Down,
	}


	public Path(List<Room> room_list, Room start = null, Room end = null) {
		pathCoords = new List<Coordinate>();
		connectedRooms = new List<Room>();

		//Walk from one random room to another
		//Pick two rooms, make sure they are not the same room
		//Caller can supply start and end
		if (start == null) {
			startRoom = room_list[rng.Next(room_list.Count)];
		}
		else {
			startRoom = start;
		}
		if (end == null) {
			do {
				endRoom = room_list[rng.Next(room_list.Count)];
			} while (ReferenceEquals(startRoom, endRoom));
		}
		else {
			endRoom = end;
		}


		//Pick start and end coordintes
		startPos = startRoom.GetRandCoordinate();
		endPos = endRoom.GetRandCoordinate();
		
		

	}

	//Trying out this fancy A* pathfinding!
	//So we want to path from one random room to another
	//Avoid other rooms as obstacles to cause more interesting behavior
	//Idea - avoid all rooms 
	private static List<Coordinate> ShortestPath(Room startRoom, Room endRoom, Floor.TileType[][] tilegrid) {
		List<Coordinate> pathCoords = new List<Coordinate>();
		Coordinate currPos;

		//Get random start points
		Coordinate startPos = startRoom.GetRandCoordinate();
		Coordinate endPos = endRoom.GetRandCoordinate();


		//Coordinates being considered to find the closest path
		List<Coordinate> openList = new List<Coordinate>();
		//Coordinates that do not have to be considered again
		List<Coordinate> closedList = new List<Coordinate>();
		//Add starting position to closed list
		openList.Add(startPos);

		
		int G = 1;
		do {
			currPos = Coordinate.FindSmallestF(openList); //Get square with lowest F
			closedList.Add(currPos);	//Switch square from open to closed list
			openList.Remove(currPos);

			if (endPos.IsInList(closedList)) {
				//We've added destination to the closed list, found a path
				break;
			}

			//Get adjacent walkable squares
			List<Coordinate> adjacents = new List<Coordinate>();
			Room[] termini = { startRoom, endRoom };
			Coordinate[] two_offs = { currPos.Clone(), currPos.Clone(), currPos.Clone(), currPos.Clone() };
			//two_offs[0]

			//To consider a tile, tile must not be next to a room tile or that room tile must be one of the termini.
			//The "nextTile" is the tile one more in the same direction as the adjacent is from the current. So two out.

			//East tile
			Coordinate nextTile = new Coordinate(currPos.x + 2, currPos.y);
			if (tilegrid[nextTile.x][nextTile.y] != Floor.TileType.Room || nextTile.IsInRooms(termini)) {
				adjacents.Add(new Coordinate(currPos.x + 1, currPos.y, currPos.F));
			}

			//West tile
			nextTile.x = currPos.x - 2;
			if (tilegrid[nextTile.x][nextTile.y] != Floor.TileType.Room || nextTile.IsInRooms(termini)) {
				adjacents.Add(new Coordinate(currPos.x - 1, currPos.y, currPos.F));
			}

			//North tile
			nextTile.x = currPos.x;
			nextTile.y = currPos.y + 2;
			if (tilegrid[nextTile.x][nextTile.y] != Floor.TileType.Room || nextTile.IsInRooms(termini)) {
				adjacents.Add(new Coordinate(currPos.x, currPos.y + 1, currPos.F));
			}

			//South tile
			nextTile.y = currPos.y - 2;
			if (tilegrid[nextTile.x][nextTile.y] != Floor.TileType.Room || nextTile.IsInRooms(termini)) {
				adjacents.Add(new Coordinate(currPos.x, currPos.y - 1, currPos.F));
			}

			foreach (Coordinate adjCoord in adjacents) {
				if (adjCoord.IsInList(closedList)) { //If adj tile is already considered, ignore it
					continue;
				}
				int newF = G + (Math.Abs(endPos.x - adjCoord.x) + Math.Abs(endPos.y - adjCoord.y));
				if (!adjCoord.IsInList(openList)) { //not in open list
					//Compute its F score, set its parent, and add it to the list
					adjCoord.F = newF;
					adjCoord.parent = currPos;
				}
				else { //In open list
					//check if current G score makes score lower, if yes update bc found better path
					if (newF < adjCoord.F) {
						adjCoord.F = newF;
						adjCoord.parent = currPos;
					}
				}
			}

			G++;
		} while (openList.Count != 0);

		//Now start at end and work backward through parents
		//Here, currPos should be endPos
		while(currPos.parent != null) {
			pathCoords.Add(currPos.Clone());
		}
		return pathCoords;
	}
}


