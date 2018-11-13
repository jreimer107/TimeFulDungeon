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
				x < room.RightBound &&
				y >= room.LowerBound &&
				y < room.UpperBound);
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


	public bool IsAdjacentToRoom(Floor.TileType[][] tilegrid, Room[] exclusionList, Coordinate prev) {
		//If we're in a terminus room then don't check anything
		if (IsInRooms(exclusionList)) {
			return false;
		}

		//Check if next to non terminus rooms (cannot touch)
		List<Coordinate> adjacents = GetAdjacents(true);
		foreach (Coordinate adj in adjacents) {
			if (tilegrid[adj.x][adj.y] == Floor.TileType.Room && !adj.IsInRooms(exclusionList)) {
				return true;
			}
		}


		//Check for terminus rooms (can enter, but cannot run alongside)
		List<Coordinate> adjs = GetAdjacents(true, false);


		//Get tile statuses
		bool east =		 adjs[0].IsInBounds() && (tilegrid[adjs[0].x][adjs[0].y] != Floor.TileType.Void || adjs[0].Equals(prev));
		bool northeast = adjs[1].IsInBounds() && (tilegrid[adjs[1].x][adjs[1].y] != Floor.TileType.Void || adjs[1].Equals(prev));
		bool north =	 adjs[2].IsInBounds() && (tilegrid[adjs[2].x][adjs[2].y] != Floor.TileType.Void || adjs[2].Equals(prev));
		bool northwest = adjs[3].IsInBounds() && (tilegrid[adjs[3].x][adjs[3].y] != Floor.TileType.Void || adjs[3].Equals(prev));
		bool west =		 adjs[4].IsInBounds() && (tilegrid[adjs[4].x][adjs[4].y] != Floor.TileType.Void || adjs[4].Equals(prev));
		bool southwest = adjs[5].IsInBounds() && (tilegrid[adjs[5].x][adjs[5].y] != Floor.TileType.Void || adjs[5].Equals(prev));
		bool south =	 adjs[6].IsInBounds() && (tilegrid[adjs[6].x][adjs[6].y] != Floor.TileType.Void || adjs[6].Equals(prev));
		bool southeast = adjs[7].IsInBounds() && (tilegrid[adjs[7].x][adjs[7].y] != Floor.TileType.Void || adjs[7].Equals(prev));

		//If we are in a corner, return true
		if ((east && northeast && north) ||
			(north && northwest && west) ||
			(west && southwest && south) ||
			(south && southeast && east)) {
			return true;
		}

		return false;
	}
	
	//The whole point of this is to avoid 2x2 path boxes, which look ugly
	//To avoid a 2x2 box you need to make sure that:
	//	you dont put a tile in a corner
	//	you dont put two tiles alongside another path
	//This includes the current tile, as that would be a tile in this situation
	//So just don't make a corner (including the current tile)
	public bool IsAdjacentToPath(Floor.TileType[][] tilegrid, Coordinate prev) {
		List<Coordinate> adjs = GetAdjacents(true, false);
		

		//Get tile statuses
		bool east =		 adjs[0].IsInBounds() && (tilegrid[adjs[0].x][adjs[0].y] == Floor.TileType.Path || adjs[0].Equals(prev));
		bool northeast = adjs[1].IsInBounds() && (tilegrid[adjs[1].x][adjs[1].y] == Floor.TileType.Path || adjs[1].Equals(prev));
		bool north =	 adjs[2].IsInBounds() && (tilegrid[adjs[2].x][adjs[2].y] == Floor.TileType.Path || adjs[2].Equals(prev));
		bool northwest = adjs[3].IsInBounds() && (tilegrid[adjs[3].x][adjs[3].y] == Floor.TileType.Path || adjs[3].Equals(prev));
		bool west =		 adjs[4].IsInBounds() && (tilegrid[adjs[4].x][adjs[4].y] == Floor.TileType.Path || adjs[4].Equals(prev));
		bool southwest = adjs[5].IsInBounds() && (tilegrid[adjs[5].x][adjs[5].y] == Floor.TileType.Path || adjs[5].Equals(prev));
		bool south =	 adjs[6].IsInBounds() && (tilegrid[adjs[6].x][adjs[6].y] == Floor.TileType.Path || adjs[6].Equals(prev));
		bool southeast = adjs[7].IsInBounds() && (tilegrid[adjs[7].x][adjs[7].y] == Floor.TileType.Path || adjs[7].Equals(prev));

		//If we are in a corner, return true
		if ((east && northeast && north) ||
			(north && northwest && west) ||
			(west && southwest && south) ||
			(south && southeast && east)) {
			return true;
		}

		return false;
	}


	private bool[] GetTileStatuses(Floor.TileType type, Floor.TileType[][] tilegrid, Coordinate prev) {
		Coordinate[] adjs = GetAdjacents(true, false).ToArray();


		//Get tile statuses
		//Counter clockwise from east
		bool[] statuses = new bool[adjs.Length];
		for (int i = 0; i < adjs.Length; i++) {
			statuses[i] = adjs[i].IsInBounds() && (tilegrid[adjs[i].x][adjs[i].y] == type || adjs[i].Equals(prev));
		}


		return statuses;
		
	}

}
