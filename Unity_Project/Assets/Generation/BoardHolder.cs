using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardHolder : MonoBehaviour {
	public enum TileType {
		Wall, Floor, Corridor, Void,
	}

	public GameObject[] floorTiles;
	public GameObject[] wallTiles;
	public GameObject player;

	private TileType[][] tiles;
	private Room[] rooms;
	private GameObject boardHolder;

	// Use this for initialization
	public void Setup () {
		boardHolder = new GameObject("BoardHolder");
		Floor floor = new Floor();
		InstantiateTiles(floor.tiles);
		//Set
	}
	
	private void InstantiateTiles(Floor.TileType[][] grid) {
		for (int x = 0; x < grid.Length; x++) {
			for (int y = 0; y < grid[0].Length; y++) {
				if (grid[x][y] == Floor.TileType.Room) {
					Vector3 position = new Vector3(x, y, 0f);
					GameObject tileInstance = Instantiate(floorTiles[0], position, Quaternion.identity) as GameObject;
					//Set tile's parent to boardholder
					tileInstance.transform.parent = boardHolder.transform;
				}
			}
		}
	}


	// Update is called once per frame
	void Update () {
		
	}
}
