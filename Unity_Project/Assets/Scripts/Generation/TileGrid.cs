using System;
using System.Collections.Generic;



public class TileGrid {
	public enum TileType {
		Void, Room, Path, Wall, Border, Entrance, Exit,
	}

	public static readonly Coordinate[] DIRS = new[] {
		new Coordinate(1, 0),
		new Coordinate(0, -1),
		new Coordinate(-1, 0),
		new Coordinate(0, 1),
	};

	public TileType[,] tiles;
	public int width, height;
	public HashSet<Coordinate> paths = new HashSet<Coordinate>();
	public HashSet<Coordinate> rooms = new HashSet<Coordinate>();

	public TileGrid(int width, int height) {
		this.width = width;
		this.height = height;
		tiles = new TileType[width, height];
	}

	public bool IsInBounds(Coordinate id) {
		return 0 <= id.x && id.x < width && 0 <= id.y && id.y < height;
	}



	public void SetTile(Coordinate pos, TileType type) {
		tiles[pos.x, pos.y] = type;
	}

	public TileType GetTileType(Coordinate pos) {
		return tiles[pos.x, pos.y];
	}

	public bool IsType(Coordinate pos, TileType expected) {
		TileType actual = this.GetTileType(pos);
		if (actual == expected) {
			return true;
		}
		return false;
	}

	public bool IsType(Coordinate pos, String expectedStr) {
		TileType actual = this.GetTileType(pos);
		TileType expected;
		if (expectedStr.ToLower().Equals("void")) {
			expected = TileType.Void;
		}
		return false;
	}

	public TileType[,] GetSurroundings(Coordinate pos, int radius = 1) {
		int side = 2 * radius + 1;

		//Get corner of box
		Coordinate corner = new Coordinate(pos.x - radius, pos.y - radius);
		TileType[,] surroundings = new TileType[side, side];
		for (int row = 0; row < side; row++) {
			for (int col = 0; col < side; col++) {
				surroundings[row, col] = this.tiles[corner.x + row, corner.y + col];
			}
		}
		return surroundings;
	}

}