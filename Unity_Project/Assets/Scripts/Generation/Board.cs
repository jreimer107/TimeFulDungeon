using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour {

	[SerializeField] private Tile roomTile = null;
	[SerializeField] private Tile borderTile = null;
	[SerializeField] private Tile pathTile = null;
	[SerializeField] private Tile entranceTile = null;
	[SerializeField] private Tile exitTile = null;
	[SerializeField] private RuleTile wallTile = null;

	[SerializeField] private Tile debugWallTile = null;

	[SerializeField] private Tilemap groundTilemap = null, wallTilemap = null;
	private Floor floor;

	public GenConfig genConfig;

	#region Singleton
	public static Board instance;
	private void Awake() {
		if (instance != null)
			Debug.LogWarning("Multiple instances of Grid detected.");
		instance = this;
	}
	#endregion


	// Use this for initialization
	public void Start() {
		floor = new Floor();
		floor.Generate();
		InstantiateGrid(floor.tiles);
		PlacePlayer();
	}

	private void Update() {
		if (Input.GetKeyDown(KeyCode.M)) {
			floor.Generate();
			InstantiateGrid(floor.tiles);
		}
	}

	private void InstantiateGrid(TileType[,] grid) {
		for (int x = 0; x < grid.GetLength(0); x++) {
			for (int y = 0; y < grid.GetLength(1); y++) {
				Vector3Int position = new Vector3Int(x, y, 0);
				switch (grid[x, y]) {
					case TileType.Room:
						groundTilemap.SetTile(position, roomTile);
						wallTilemap.SetTile(position, null);
						break;
					case TileType.Path:
						groundTilemap.SetTile(position, pathTile);
						break;
					case TileType.Border:
						groundTilemap.SetTile(position, borderTile);
						break;
					case TileType.Entrance:
						groundTilemap.SetTile(position, entranceTile);
						break;
					case TileType.Exit:
						groundTilemap.SetTile(position, exitTile);
						break;
					case TileType.Wall:
						if (debugWallTile == null) {
							wallTilemap.SetTile(position, wallTile);
						} else {
							wallTilemap.SetTile(position, debugWallTile);
						}
						break;
					default:
						wallTilemap.SetTile(position, null);
						break;
				}
			}
		}
	}

	private void PlacePlayer() {
		Player.instance.transform.position = new Vector3(floor.entrance.x + 0.5f, floor.entrance.y + 0.5f, 0f);
	}

	public bool IsTileOfType(Coordinate pos, TileType type) {
		return floor.IsTileOfType(pos, type);
	}

	public bool IsTileTraversable(Coordinate pos) {
		return floor.IsTileTraversable(pos);
	}

	public HashSet<Coordinate> GetShortestPath(Coordinate start,
		Coordinate end,
		Func<Coordinate, Coordinate, Coordinate[]> GetSuccessorsFunction,
		Func<Coordinate, Coordinate, Coordinate, float, float> GetCostFunction,
		Func<Coordinate, Coordinate, float> GetHeuristicFunction) {
		return floor.GetShortestPath(start, end, GetSuccessorsFunction, GetCostFunction, GetHeuristicFunction);
	}
}

public enum TileType {
	Void, Room, Path, Wall, Border, Entrance, Exit,
}