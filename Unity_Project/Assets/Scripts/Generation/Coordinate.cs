using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple wrapper class for two integers that make up x and y of a coordinate pair.
/// </summary>
public class Coordinate : IComparable<Coordinate>, IEquatable<Coordinate> {
	public readonly int x, y;

	/// <summary>
	/// Constructor for Coordinate.
	/// </summary>
	/// <param name="x_pos">x position integer.</param>
	/// <param name="y_pos">y position integer.</param>
	public Coordinate(int x_pos, int y_pos) {
		this.x = x_pos;
		this.y = y_pos;
	}

	public Coordinate(Vector2Int a) {
		this.x = a.x;
		this.y = a.y;
	}

	public int CompareTo(Coordinate other) {
		if (this.x != other.x) {
			return this.x.CompareTo(other.x);
		}
		return this.y.CompareTo(other.y);
	}

	public bool Equals(Coordinate other) {
		return !ReferenceEquals(other, null) && (this.x == other.x && this.y == other.y);
	}

	public bool Equals(int x, int y) {
		return (this.x == x && this.y == y);
	}

	public override bool Equals(object obj) => obj is Coordinate coordinate && this.Equals(coordinate);

	public static bool operator ==(Coordinate a, Coordinate b) => !ReferenceEquals(a, null) && a.Equals(b);
	public static bool operator !=(Coordinate a, Coordinate b) => !ReferenceEquals(a, null) && !a.Equals(b);

	public static implicit operator Vector2Int(Coordinate a) => new Vector2Int(a.x, a.y);

	public override int GetHashCode() {
		int hash = 13;
		hash = (hash * 7) + this.x.GetHashCode();
		hash = (hash * 7) + this.y.GetHashCode();
		return hash;
	}

	public static float Distance(Coordinate a, Coordinate b) => Vector2Int.Distance(a, b);

	public override string ToString() => $"({this.x}, {this.y})"; 

	public Coordinate[] GetValidSuccessorsForPathfinding() {
		List<Coordinate> successors = new List<Coordinate> {
			new Coordinate(x + 1, y),
			new Coordinate(x + 1, y + 1),
			new Coordinate(x, y + 1),
			new Coordinate(x - 1, y + 1),
			new Coordinate(x - 1, y),
			new Coordinate(x - 1, y - 1),
			new Coordinate(x, y - 1),
			new Coordinate(x + 1, y - 1)
		};
		successors.RemoveAll(x => !Board.instance.IsTileTraversable(x));
		return successors.ToArray();
	}

	public static Coordinate[] GetValidSuccessorsForPathGen(Coordinate c, Coordinate p, Room[] endpoints) {
		List<Coordinate> successors = new List<Coordinate> {
			new Coordinate(c.x + 1, c.y),
			new Coordinate(c.x, c.y + 1),
			new Coordinate(c.x - 1, c.y),
			new Coordinate(c.x, c.y - 1)
		};
		successors.RemoveAll(x => !x.ValidSuccessor(c, p, endpoints));
		return successors.ToArray();
	}

	//The whole point of this is to avoid 2x2 path boxes, which look ugly
	//To avoid a 2x2 box you need to make sure that:
	//	you dont put a tile in a corner
	//	you dont put two tiles alongside another path
	//This includes the current tile, as that would be a tile in this situation
	//So just don't make a corner (including the current tile)
	public bool ValidSuccessor(Coordinate p, Coordinate gp, Room[] endpoints) {
		//Check if in endpoint room (don't check for rules anymore)
		if (this.InRooms(endpoints)) {
			return true;
		}

		//Get surroundings
		List<Coordinate> adjs = new List<Coordinate> {
			new Coordinate(x + 1, y),
			new Coordinate(x + 1, y + 1),
			new Coordinate(x, y + 1),
			new Coordinate(x - 1, y + 1),
			new Coordinate(x - 1, y),
			new Coordinate(x - 1, y - 1),
			new Coordinate(x, y - 1),
			new Coordinate(x + 1, y - 1),
		};

		//Check if next to non-endpoint room (cannot touch)
		foreach (Coordinate a in adjs) {
			if (!a.IsInBounds() || Board.instance.IsTileOfType(a, TileType.Room) && !a.InRooms(endpoints)) {
				return false;
			}
		}

		//Check which adjacent tiles are taken
		bool[] taken = new bool[8];
		for (int i = 0; i < 8; i++) {
			taken[i] = (!adjs[i].IsInBounds() ||
				!Board.instance.IsTileOfType(adjs[i], TileType.Wall) ||
				adjs[i].Equals(p) ||
				(gp != null && adjs[i].Equals(gp)));
		}

		//If we are in a corner we make a box. Don't want this
		if (taken[0] && taken[1] && taken[2] ||
			taken[2] && taken[3] && taken[4] ||
			taken[4] && taken[5] && taken[6] ||
			taken[6] && taken[7] && taken[0]) {
			return false;
		}

		return true;
	}

	public bool InRooms(Room[] checkRooms) {
		for (int i = 0; i < checkRooms.Length; i++) {
			if (InRoom(checkRooms[i])) {
				return true;
			}
		}
		return false;
	}

	private bool InRoom(Room room) {
		return (x >= room.LeftBound &&
			x < room.RightBound &&
			y >= room.LowerBound &&
			y < room.UpperBound);
	}

	public Coordinate Clone() {
		return new Coordinate(x, y);
	}

	public bool IsInBounds() {
		GenConfig gencfg = Board.instance.genConfig;
		return !(
			x < 0 ||
			x >= gencfg.FloorWidth ||
			y < 0 ||
			y >= gencfg.FloorHeight
		);
	}

	public static bool IsInBounds(int x, int y) {
		GenConfig gencfg = Board.instance.genConfig;
		return !(
			x < 0 ||
			x >= gencfg.FloorWidth ||
			y < 0 ||
			y >= gencfg.FloorHeight
		);
	}

	public static float heuristic(Coordinate a, Coordinate b) {
		return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
	}
}