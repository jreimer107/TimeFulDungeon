﻿using System.Collections.Generic;

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
	
	public List<Coordinate> GetAdjacents(int distance = 1, bool diagonals = false) {
		List<Coordinate> adjacents = new List<Coordinate> {
			new Coordinate(x + distance, y),
			new Coordinate(x - distance, y),
			new Coordinate(x, y + distance),
			new Coordinate(x, y - distance),
		};

		if (diagonals) {
			Coordinate[] diags = {
				new Coordinate(x + distance, y + distance),
				new Coordinate(x + distance, y - distance),
				new Coordinate(x - distance, y + distance),
				new Coordinate(x - distance, y - distance)
			};
			adjacents.AddRange(diags);
		}

		adjacents.RemoveAll(x => !x.IsInBounds());

		return adjacents;
	}


	public bool IsAdjacentToRoom(Floor.TileType[][] tilegrid, Room[] exclusionList = null, int distance = 1) {
		List<Coordinate> adjacents = GetAdjacents(distance, true);

		foreach (Coordinate adj in adjacents) {
			if (tilegrid[adj.x][adj.y] == Floor.TileType.Room && !adj.IsInRooms(exclusionList)) {
				return true;
			}
		}
		return false;
	}
}
