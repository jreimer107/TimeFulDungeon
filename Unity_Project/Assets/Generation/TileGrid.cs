using System;
using System.Collections;

public class TileGrid {
	public enum TileType {
		Void, Room, Path, Wall, Border, Entrance, Exit,
	}

	public TileType[,] tiles;
	public int width;
	public int height;

	public TileGrid(int width, int height) {
		tiles = new TileType[width, height];
		// for (int row = 0; row < tiles.Length; row++) {
		// 	tiles[row] = new TileType[height];
		// 	for (int col = 0; col < tiles[0].Length; col++) {
		// 		tiles[row][col] = TileType.Wall;
		// 	}
		// }
	}

	public void SetTile(Coordinate pos, TileType type) {
		tiles[pos.x, pos.y] = type;
	}

	public TileType GetTileType(Coordinate pos) {
		return tiles[pos.x, pos.y];
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