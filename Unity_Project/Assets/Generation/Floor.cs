using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class Floor {
	public enum TileType {
		Void, Room, Cooridor, Wall, Border,
	}
	public enum Direction {
		Up, Down, Left, Right,
	}

	private List<Room> room_list;
    public TileType[][] tiles;
	private Random rng;

	//Level generation!
	public Floor() {
		//Create room list
		room_list = new List<Room>();
		rng = new Random();

		//Create tile grid
		tiles = new TileType[Constants.FLOOR_WIDTH][];
		for (int row = 0; row < tiles.Length; row++) {
			tiles[row] = new TileType[Constants.FLOOR_HEIGHT];
		}

		//Attempt to place a room some number of times
        for (int attempt = 0; attempt < Constants.ROOM_ATTEMPTS; attempt++) {
			AddRoom(RandRoom()); //Creates and adds a random room if it fits in the grid
        }

		//Copy rooms into tile grid
		foreach (Room room in room_list) {
			for (int x = room.LeftBound; x < room.RightBound; x++) {
				for (int y = room.LowerBound; y < room.UpperBound; y++) {
					tiles[x][y] = TileType.Room;
				}
			}
		}

		//Draw outer border of level
		for (int y = 0; y < tiles[0].Length; y++) {
			tiles[0][y] = TileType.Border;
		}
		for (int y = 0; y < tiles[0].Length; y++) {
			tiles[Constants.FLOOR_WIDTH - 1][y] = TileType.Border;
		}
		for (int x = 0; x < tiles.Length; x++) {
			tiles[x][0] = TileType.Border;
		}
		for (int x = 0; x < tiles.Length; x++) {
			tiles[x][Constants.FLOOR_HEIGHT - 1] = TileType.Border;
		}
	}


	public void GeneratePath() {
		//Walk from one random room to another
		//Pick two rooms, make sure they are not the same room
		Room start = room_list[rng.Next(room_list.Count)];
		Room end;
		do {
			end = room_list[rng.Next(room_list.Count)];
		} while (ReferenceEquals(start, end));

		//Pick start and end coordintes
		Coordinate startPos = start.GetRandCoordinate();
		Coordinate endPos = end.GetRandCoordinate();
		Coordinate currPos = new Coordinate(startPos.x, startPos.y);

		//Load dirChangeInterval, will be decremented every tile moved until 0
		//At that point the direction will randomly change
		int dirChangeInterval = rng.Next(Constants.PATH_DIR_CHANGE_INTERVAL_MIN, Constants.PATH_DIR_CHANGE_INTERVAL_MAX);

		//Direction variable to remember which way we are going
		Direction dir = Direction.Left;

		//Distance coordiante that will be used to determine direction change probabilities
		Coordinate distance = startPos.DistanceFrom(endPos);

		//Direction probabilities, have to use ints bc can't compare doubles, range from 100 to 0
		//horizontalDirChance is the proportion of the directional distances (x_dist / y_dist)
		//x/yDirChance are 100% to right direction when far away and 50% when close
		int xDirChance, yDirChance, horizontalDirChance;

		//Randomly path to that point
		while (!currPos.Equals(endPos)) {
			//Update distance
			distance = currPos.DistanceFrom(endPos);

			//The farther in one direction from the endpoint, the more likely the path will take that direction
			//Can change directions every random interval
			if (dirChangeInterval == 0) {
				//Update direction probabilities
				horizontalDirChance = distance.x % distance.y;
				//Get remainder of distance/max. Scale to 50-100. Don't have to worry about mod rollover bc won't happen
				xDirChance = (distance.x % Constants.ROOM_MIN_WIDTH) / 2 + 50;
				yDirChance = (distance.y % Constants.ROOM_MIN_HEIGHT) / 2 + 50;

				//Determine whether going horizontal or vertical
				//horizontalDirChance can be > 1 so need to invert if so and then act accordingly
				if (distance.x > distance.y) { //Higher x chance than y chance
					if (horizontalDirChance > rng.Next(100)) { //x happens

					}

				}
				else { //Higher y chance than x chance
					if (horizontalDirChance > rng.NextDouble) {

					}
				}
			}


		}
	}


	/// <summary>
	/// Adds a new Room to the room list if it fits in the floor grid.
	/// </summary>
	/// <param name="newRoom">The new room to be added.</param>
	/// <returns>True if the room was successfully added, false otherwise.</returns>
	public bool AddRoom(Room newRoom) {
		//Make sure floor is inside the floor border
		if (!IsInsideFloor(newRoom)) {
			return false;
		}

		//Make sure room doesn't overap other rooms
		bool space_taken = false;
		foreach (Room room in room_list) {
			if (OverlapsRoom(newRoom, room)) {
				space_taken = true;
				break;
			}
		}
		if (!space_taken) {
			room_list.Add(newRoom);
			return true;
		}
		
		return false;
	}


	/// <summary>
	/// Checks to see if two rooms are overlapping, including the extra gap between them.
	/// </summary>
	/// <param name="r1">The first room to compare.</param>
	/// <param name="r2">The second room to compare.</param>
	/// <returns>True if the rooms overlap or are within the GAP of each other, false otherwise.</returns>
	public bool OverlapsRoom(Room r1, Room r2) {
		return !(r1.LeftBound >= r2.RightSpace ||
				 r2.LeftBound >= r1.RightSpace ||
				 r1.UpperBound <= r2.LowerSpace ||
				 r2.UpperBound <= r1.LowerSpace);
	}

	/// <summary>
	/// Checks to see if a room is within the floor border.
	/// </summary>
	/// <param name="room">The room to check.</param>
	/// <returns>True if the room is completely within the border, false otherwise.</returns>
	public bool IsInsideFloor(Room room) {
		return (room.LeftSpace >= 0 &&
				room.RightSpace <= Constants.FLOOR_WIDTH &&
				room.UpperSpace <= Constants.FLOOR_HEIGHT &&
				room.LowerSpace >= 0);
	}
	
//Generates a random room
	public Room RandRoom() {
		//Create randomly placed and sized region
		int room_x = rng.Next(Constants.FLOOR_WIDTH - 1);
		int room_y = rng.Next(Constants.FLOOR_HEIGHT - 1);
		int room_w, room_h;
		do {
			room_w = (int)Gauss(Constants.ROOM_SIZE_MEAN, Constants.ROOM_SIZE_DEVIATION);
		} while (room_w < Constants.ROOM_MIN_WIDTH);
		do {
			room_h = (int)Gauss(Constants.ROOM_SIZE_MEAN, Constants.ROOM_SIZE_DEVIATION);
		} while (room_h < Constants.ROOM_MIN_HEIGHT);

		return new Room(room_x, room_y, room_w, room_h);
	}




	//Generates random doubles based on the mean and deviation fed in.
	//Also I have no idea how it works.
	public double Gauss(double mean, double deviation) {
        double x1, x2, w;
		do {
			x1 = rng.NextDouble() * 2 - 1;
			x2 = rng.NextDouble() * 2 - 1;
			w = x1 * x1 + x2 * x2;
		} while (w < 0 || w > 1);

		w = Math.Sqrt(-2 * Math.Log(w) / w);
		return mean + deviation * x1 * w;
    }

	public void Print() {
		string floor_bin = "";
		for (int x = 0; x < tiles.Length; x++) {
			for (int y = 0; y < tiles[0].Length; y++) {
				if (tiles[x][y] == TileType.Room) {
					floor_bin += "1";
				}
				else {
					floor_bin += "0";
				}
			}
			floor_bin += "\n";
		}
		Debug.Log(floor_bin);

		Debug.Log("Done");
	}
}
