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

	public override int GetHashCode() {
		int hash = 13;
		hash = (hash * 7) + this.x.GetHashCode();
		hash = (hash * 7) + this.y.GetHashCode();
		return hash;
	}

	public override string ToString() {
		return String.Format("({0},{1})", this.x, this.y);
	}

	public List<Coordinate> getSuccessors(Floor.TileType[,] tiles) {
		List<Coordinate> successors = new List<Coordinate> {
			new Coordinate(x + 1, y),
			new Coordinate(x, y + 1),
			new Coordinate(x - 1, y),
			new Coordinate(x, y - 1)
		};
		successors.RemoveAll(x => !x.IsInBounds());

		//Build surrounding area grid to see which tiles are good/bad
		bool[,] inUse = new bool[5, 5];
		for (int row = 0; row < 5; row++) {
			for (int col = 0; col < 5; col++) {
				int realX = this.x - 2 + row;
				int realY = this.y - 2 + col;
				if (!Coordinate.IsInBounds(realX, realY) ||         //Not in bounds
					tiles[realX, realY] != Floor.TileType.Wall ||   //Is taken
					(realX == this.x && realY == this.y)) {         //Is the current square
					inUse[row, col] = true;
				} else {
					inUse[row, col] = false;
				}
			}
		}

		//Remove all successors that make a 2x2box
		successors.RemoveAll(suc => suc.makesBox(inUse, suc.x - this.x + 2, suc.y - this.y + 2));
		Debug.Log(String.Format("Curr: {0}, Valid successors: {1}", this.ToString(), string.Join(", ", successors)));
		return successors;
	}

	private bool makesBox(bool[,] inUse, int x_pos, int y_pos) {
		bool e = inUse[x_pos + 1, y_pos];
		bool ne = inUse[x_pos + 1, y_pos + 1];
		bool n = inUse[x_pos, y_pos + 1];
		bool nw = inUse[x_pos - 1, y_pos + 1];
		bool w = inUse[x_pos - 1, y_pos];
		bool sw = inUse[x_pos - 1, y_pos - 1];
		bool s = inUse[x_pos, y_pos - 1];
		bool se = inUse[x_pos + 1, y_pos - 1];

		if ((e && ne && n) ||
			(n && nw && w) ||
			(w && sw && s) ||
			(s && s && se)) {
			//Debug.Log(String.Format("Taken: x_pos: {0}, y_pos:{1}", x_pos, y_pos));
			return true;
		}
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

	public bool IsNextToPath(Floor.TileType[,] tiles) {
		List<Coordinate> successors = this.getSuccessors(tiles);
		foreach (Coordinate suc in successors) {
			if (tiles[suc.x, suc.y] == Floor.TileType.Path) {
				return true;
			}
		}
		return false;
	}

	public static int heuristic(Coordinate a, Coordinate b) {
		return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
	}
}
