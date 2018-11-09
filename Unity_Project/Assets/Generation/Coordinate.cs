using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple wrapper class for two integers that make up x and y of a coordinate pair.
/// </summary>
public class Coordinate {
	public int x;
	public int y;
	public int F; //Weight/score for pathfinding.
	public Coordinate parent;

	/// <summary>
	/// Constructor for Coordinate.
	/// </summary>
	/// <param name="x_pos">x position integer.</param>
	/// <param name="y_pos">y position integer.</param>
	public Coordinate(int x_pos, int y_pos, int F = 0, Coordinate parent = null) {
		this.x = x_pos;
		this.y = y_pos;
		this.F = F;
		this.parent = parent;
	}

	public bool Equals(Coordinate other) {
		return (this.x == other.x && this.y == other.y);
	}

	public Coordinate DistanceFrom(Coordinate other) {
		int x_distance = this.x - other.x;
		int y_distance = this.y - other.y;
		return new Coordinate(x_distance, y_distance);
	}


	public bool IsInRooms(Room[] checkRooms) {
		for (int i = 0; i < checkRooms.Length; i++) {
			if (IsInsideRoom(checkRooms[i])) {
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Checks to see if a coordinate point is inside a room object.
	/// </summary>
	/// <param name="pos">Coordinate object to check.</param>
	/// <param name="room">Room object to compare against.</param>
	/// <returns>True if point is inside room, false otherwise.</returns>
	public bool IsInsideRoom(Room room) {
		return (x >= room.LeftBound &&
				x <= room.RightBound &&
				y >= room.LowerBound &&
				y <= room.UpperBound);
	}

	/// <summary>
	/// Checks if the coordinate is in a given list of coordinates.
	/// </summary>
	/// <param name="coord_list">The list of coordinates to parse.</param>
	/// <returns>True if the coordinate is in the list, false otherwise.</returns>
	public bool IsInList(List<Coordinate> coord_list) {
		foreach (Coordinate coord in coord_list) {
			if (coord.x == x && coord.y == y) {
				return true;
			}
		}
		return false;
	}

	public static Coordinate FindSmallestF(List<Coordinate> coord_list) {
		Coordinate smallestF = coord_list[0];
		foreach (Coordinate coord in coord_list) {
			if (coord.F < smallestF.F) {
				smallestF = coord;
			}
		}
		return smallestF;
	}

	public Coordinate Clone() {
		return new Coordinate(x, y, F, parent);
	}

	public bool IsInBounds() {
		return !(
			x < 0 ||
			x >= Constants.FLOOR_WIDTH ||
			y < 0 ||
			y >= Constants.FLOOR_HEIGHT
		);
	}

	public static bool IsInBounds(int x, int y) {
		return !(
			x < 0 ||
			x >= Constants.FLOOR_WIDTH ||
			y < 0 ||
			y >= Constants.FLOOR_HEIGHT
		);
	}
	
	public List<Coordinate> GetAdjacents(bool diagonals = false, bool boundsCheck = true) {
		List<Coordinate> adjacents;


		if (diagonals) {
			adjacents = new List<Coordinate> {
				new Coordinate(x + 1, y),
				new Coordinate(x + 1, y + 1),
				new Coordinate(x, y + 1),
				new Coordinate(x - 1, y + 1),
				new Coordinate(x - 1, y),
				new Coordinate(x - 1, y - 1),
				new Coordinate(x, y - 1),
				new Coordinate(x + 1, y - 1),
			};
		}
		else {
			adjacents = new List<Coordinate> {
				new Coordinate(x + 1, y),
				new Coordinate(x, y + 1),
				new Coordinate(x - 1, y),
				new Coordinate(x, y - 1)
			};
		}

		if (boundsCheck) {
			adjacents.RemoveAll(x => !x.IsInBounds());
		}
		

		return adjacents;
	}


	public bool IsAdjacentToRoom(Floor.TileType[][] tilegrid, Room[] exclusionList = null) {
		List<Coordinate> adjacents = GetAdjacents(true);

		foreach (Coordinate adj in adjacents) {
			if (tilegrid[adj.x][adj.y] == Floor.TileType.Room && !adj.IsInRooms(exclusionList)) {
				return true;
			}
		}
		return false;
	}


	//The whole point of this is to avoid 2x2 path boxes, which look ugly
	//To avoid a 2x2 box you need to make sure that:
	//	you dont put a tile in a corner
	//	you dont put two tiles alongside another path
	//This includes the current tile, as that would be a tile in this situation
	public bool IsAdjacentToPath(Coordinate current, Floor.TileType[][] tilegrid) {
		List<Coordinate> adjacents = GetAdjacents(true, false);

		bool horizontalOrientation = current.x != x;

		//Get tile statuses
		bool east = adjacents[0].IsInBounds() && (tilegrid[x + 1][y] == Floor.TileType.Path || adjacents[0].Equals(current));
		bool northeast = adjacents[1].IsInBounds() && (tilegrid[x + 1][y + 1] == Floor.TileType.Path || adjacents[1].Equals(current));
		bool north = adjacents[2].IsInBounds() && (tilegrid[x][y + 1] == Floor.TileType.Path || adjacents[2].Equals(current));
		bool northwest = adjacents[3].IsInBounds() && (tilegrid[x - 1][y + 1] == Floor.TileType.Path || adjacents[3].Equals(current));
		bool west = adjacents[4].IsInBounds() && (tilegrid[x - 1][y] == Floor.TileType.Path || adjacents[4].Equals(current));
		bool southwest = adjacents[5].IsInBounds() && (tilegrid[x - 1][y - 1] == Floor.TileType.Path || adjacents[5].Equals(current));
		bool south = adjacents[6].IsInBounds() && (tilegrid[x][y - 1] == Floor.TileType.Path || adjacents[6].Equals(current));
		bool southeast = adjacents[7].IsInBounds() && (tilegrid[x + 1][y - 1] == Floor.TileType.Path || adjacents[7].Equals(current));
		
		//If we are in a corner, return true
		if ((east && northeast && north) ||
			(north && northwest && west) ||
			(west && southwest && south) ||
			(south && southeast && east)) {
			return true;
		}

		//If we are alongside a path, return true
		if (horizontalOrientation) { //Curr is same horizontal as us
			if ((northwest && north && northeast) ||
				(southwest && south && southeast)) {
				return true;
			}
		}
		else { //Curr is same vertical as us
			if ((northwest && west && southwest) ||
				(northeast && east && southeast)) {
				return true;
			}
		}
		//Debug.Log(string.Format("({0},{1}) is ok.", x, y));

		return false;
	}

}
