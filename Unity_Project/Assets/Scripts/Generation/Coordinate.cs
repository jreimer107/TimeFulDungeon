using System.Collections.Generic;
using System.Collections;
using System;
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

	public List<Coordinate> getSuccessors(Floor.TileType[,] tiles, Coordinate prev) {

		//Just do it manually
		List<Coordinate> successors = new List<Coordinate> {
			new Coordinate(x + 1, y),
			new Coordinate(x, y + 1),
			new Coordinate(x - 1, y),
			new Coordinate(x, y - 1)

		};
		successors.RemoveAll(x => x.makesBox(tiles, this, prev));
		return successors;
	}

	private static bool isValid(Floor.TileType[] t) {
		if (t[1] == Floor.TileType.Room ||
			t[3] == Floor.TileType.Room ||
			(t[0] == Floor.TileType.Path && t[1] == Floor.TileType.Path && t[2] == Floor.TileType.Path) ||
			(t[2] == Floor.TileType.Path && t[3] == Floor.TileType.Path && t[4] == Floor.TileType.Path)) {
			return false;
		}
		return true;
	}

	//The whole point of this is to avoid 2x2 path boxes, which look ugly
	//To avoid a 2x2 box you need to make sure that:
	//	you dont put a tile in a corner
	//	you dont put two tiles alongside another path
	//This includes the current tile, as that would be a tile in this situation
	//So just don't make a corner (including the current tile)
	private bool makesBox(Floor.TileType[,] tiles, Coordinate p, Coordinate gp) {
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

		//Check if each surrounding tile is available
		bool[] taken = new bool[8];
		for (int i = 0; i < 8; i++) {
			taken[i] = (
				!adjs[i].IsInBounds() ||
				tiles[adjs[i].x, adjs[i].y] != Floor.TileType.Wall ||
				adjs[i].Equals(p) ||
				(gp != null && adjs[i].Equals(gp)));
		}

		//If we are in a corner, return true
		if (taken[0] && taken[1] && taken[2] ||
			taken[2] && taken[3] && taken[4] ||
			taken[4] && taken[5] && taken[6] ||
			taken[6] && taken[7] && taken[0]) {
			// Debug.Log(string.Format("{0} with parents {1}, {2} makes box.", this, p, gp));
			return true;
		}

		// for (int i = 1; i < 8; i += 2) {
		// 	taken[i] = (!adjs[i].IsInBounds() ||
		// 		tiles[adjs[i].x, adjs[i].y] == Floor.TileType.Room);
		// }

		// if (taken[1] ^ taken[3] ||
		// 	taken[3] ^ taken[5] ||
		// 	taken[5] ^ taken[7] ||
		// 	taken[7] ^ taken[1]) {
		// 	return true;
		// }

		return false;
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
