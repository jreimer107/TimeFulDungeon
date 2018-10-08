using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorCreator : MonoBehaviour {
	//Tile types
	public enum TileType {
		Wall, Floor,
	}

	public GameObject[] floorTiles;
	public GameObject[] wallTiles;
	public GameObject player;

	private TileType[][] tiles;
	private Room[] rooms;
	private GameObject boardHolder;

	// Use this for initialization
	private void Start () {
		boardHolder = new GameObject("BoardHolder");

		Set
	}
	
	void SetupTilesArray() {
		//Set the tiles jagged array to correct width
		tiles = new TileType[Constants.FLOOR_HEIGHT][];
		//Go through all tile arrays
		for (int i = 0; i < tiles.Length; i++) {
			tiles[i] = new TileType[rows];
		}
	}


	// Update is called once per frame
	void Update () {
		
	}
}
