﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BoardHolder : MonoBehaviour {

	public Tile roomTile;
	public Tile wallTile;
	public Tile borderTile;
	public Tile pathTile;
	public GameObject player;

	public Tilemap tilemap;


	// Use this for initialization
	public void Awake () {
		Floor floor = new Floor();
		InstantiateGrid(floor.tiles);
		//Set
	}
	
	private void InstantiateGrid(Floor.TileType[][] grid) {
		for (int x = 0; x < grid.Length; x++) {
			for (int y = 0; y < grid[0].Length; y++) {
				Vector3Int position = new Vector3Int(x, y, 0);
				if (grid[x][y] == Floor.TileType.Room) {
					tilemap.SetTile(position, roomTile);
				}
				else if (grid[x][y] == Floor.TileType.Path) {
					tilemap.SetTile(position, pathTile);
				}
				else if (grid[x][y] == Floor.TileType.Border) {
					tilemap.SetTile(position, borderTile);
				}
				
			}
		}
	}
}