using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardHolder : MonoBehaviour {
	public enum TileType {
		Wall, Floor, Corridor, Void,
	}

	public GameObject[] floorTiles;
	public GameObject[] wallTiles;
	public GameObject borderTile;
	public GameObject pathTile;
	public GameObject player;

	private TileType[][] tiles;
	private Room[] rooms;
	private GameObject boardHolder;

	// Use this for initialization
	public void Setup () {
		boardHolder = new GameObject("BoardHolder");
		Floor floor = new Floor();
		InstantiateGrid(floor.tiles);
		//Set
	}
	
	private void InstantiateGrid(Floor.TileType[][] grid) {
		for (int x = 0; x < grid.Length; x++) {
			for (int y = 0; y < grid[0].Length; y++) {
				if (grid[x][y] == Floor.TileType.Room) {
					InstantiateTile(x, y, floorTiles[0]);
				}
				else if (grid[x][y] == Floor.TileType.Path) {
					InstantiateTile(x, y, pathTile);
				}
				else if (grid[x][y] == Floor.TileType.Border) {
					InstantiateTile(x, y, borderTile);
				}
			}
		}
	}

	private void InstantiateTile(int x_pos, int y_pos, GameObject type) {
		Vector3 position = new Vector3(x_pos, y_pos, 0f);
		GameObject tileInstance = Instantiate(type, position, Quaternion.identity) as GameObject;
		//Set tile's parent to boardholder
		tileInstance.transform.parent = boardHolder.transform;
	}



	// Update is called once per frame
	void Update () {
		
	}
}
