using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple wrapper class for two integers that make up x and y of a coordinate pair.
/// </summary>
public class Coordinate : IComparable<Coordinate>, IEquatable<Coordinate> {
	public readonly int x,
	y;

	/// <summary>
	/// Constructor for Coordinate.
	/// </summary>
	/// <param name="x_pos">x position integer.</param>
	/// <param name="y_pos">y position integer.</param>
	public Coordinate(int x_pos, int y_pos) {
		this.x = x_pos;
		this.y = y_pos;
	}

	public int CompareTo(Coordinate other) {
		if (this.x != other.x) {
			return this.x.CompareTo(other.x);
		}
		return this.y.CompareTo(other.y);
	}

	public bool Equals(Coordinate other) {
		return (this.x == other.x && this.y == other.y);
	}

	public bool Equals(int x, int y) {
		return (this.x == x && this.y == y);
	}

	public override int GetHashCode() {
		int hash = 13;
		hash = (hash * 7) + this.x.GetHashCode();
		hash = (hash * 7) + this.y.GetHashCode();
		return hash;
	}

	public override string ToString() {
		return String.Format("({0},{1})", this.x, this.y);
	}

	public List<Coordinate> getSuccessors() {
		List<Coordinate> successors = new List<Coordinate> {
			new Coordinate(x + 1, y),
			new Coordinate(x, y + 1),
			new Coordinate(x - 1, y),
			new Coordinate(x, y - 1)

		};
		// successors.RemoveAll(x => !x.ValidSuccessor(tiles, this, prev, endpoints));
		return successors;
	}
	
	//The whole point of this is to avoid 2x2 path boxes, which look ugly
	//To avoid a 2x2 box you need to make sure that:
	//	you dont put a tile in a corner
	//	you dont put two tiles alongside another path
	//This includes the current tile, as that would be a tile in this situation
	//So just don't make a corner (including the current tile)
	public bool ValidSuccessor(Floor.TileType[, ] tiles, Coordinate p, Coordinate gp, Room[] endpoints) {
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
			if (!a.IsInBounds() || tiles[a.x, a.y] == Floor.TileType.Room && !a.InRooms(endpoints)) {
				return false;
			}
		}

		//Check which adjacent tiles are taken
		bool[] taken = new bool[8];
		for (int i = 0; i < 8; i++) {
			taken[i] = (!adjs[i].IsInBounds() ||
				tiles[adjs[i].x, adjs[i].y] != Floor.TileType.Wall ||
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

	private bool InRooms(Room[] checkRooms) {
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
		GenConfig gencfg = GameObject.Find("Tilemap").GetComponent<GenConfig>();
		return !(
			x < 0 ||
			x >= gencfg.FloorWidth ||
			y < 0 ||
			y >= gencfg.FloorHeight
		);
	}

	public static bool IsInBounds(int x, int y) {
		GenConfig gencfg = GameObject.Find("Tilemap").GetComponent<GenConfig>();
		return !(
			x < 0 ||
			x >= gencfg.FloorWidth ||
			y < 0 ||
			y >= gencfg.FloorHeight
		);
	}

	// public bool IsNextToPath(Floor.TileType[,] tiles) {
	// 	List<Coordinate> successors = this.getSuccessors(tiles);
	// 	foreach (Coordinate suc in successors) {
	// 		if (tiles[suc.x, suc.y] == Floor.TileType.Path) {
	// 			return true;
	// 		}
	// 	}
	// 	return false;
	// }

	public static int heuristic(Coordinate a, Coordinate b) {
		return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
	}
}