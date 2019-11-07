using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BoardHolder : MonoBehaviour {

	[SerializeField] private Tile roomTile;
	[SerializeField] private Tile borderTile;
	[SerializeField] private Tile pathTile;
	[SerializeField] private Tile entranceTile;
	[SerializeField] private Tile exitTile;
	[SerializeField] private RuleTile wallTile;

	[SerializeField] private Tilemap groundTilemap, wallTilemap;
	private Floor floor;

	public GenConfig genConfig;

	#region Singleton
	public static BoardHolder instance;
	private void Awake() {
		if (instance != null)
			Debug.LogWarning("Multiple instances of Boardholder detected.");
		instance = this;
	}
	#endregion


	// Use this for initialization
	public void Start() {
		floor = new Floor();
		InstantiateGrid(floor.tiles);
		PlacePlayer();
	}

	private void InstantiateGrid(Floor.TileType[,] grid) {
		for (int x = 0; x < grid.GetLength(0); x++) {
			for (int y = 0; y < grid.GetLength(1); y++) {
				Vector3Int position = new Vector3Int(x, y, 0);
				if (grid[x, y] == Floor.TileType.Room) {
					groundTilemap.SetTile(position, roomTile);
				} else if (grid[x, y] == Floor.TileType.Path) {
					groundTilemap.SetTile(position, pathTile);
				} else if (grid[x, y] == Floor.TileType.Border) {
					groundTilemap.SetTile(position, borderTile);
				} else if (grid[x, y] == Floor.TileType.Entrance) {
					groundTilemap.SetTile(position, entranceTile);
				} else if (grid[x, y] == Floor.TileType.Exit) {
					groundTilemap.SetTile(position, exitTile);
				} else {
					wallTilemap.SetTile(position, wallTile);
				}
			}
		}
	}

	private void PlacePlayer() {
		Player.instance.transform.position = new Vector3(floor.entrance.x + 0.5f, floor.entrance.y + 0.5f, 0f);
	}
}
