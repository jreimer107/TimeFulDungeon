using System;
using System.Collections.Generic;
using Random = System.Random;

public class Floor {
	public enum TileType {
		Void, Room, Path, Wall, Border, Entrance, Exit,
	}

	private List<Room> room_list;
	private List<Hall> path_list;
	public TileType[,] tiles;
	private Random rng;
	public Coordinate entrance;
	public Coordinate exit;

	//Level generation!
	public Floor() {
		//Create room list
		room_list = new List<Room>();
		path_list = new List<Hall>();
		rng = new Random();

		//Create tile grid
		tiles = new TileType[Constants.FLOOR_WIDTH, Constants.FLOOR_HEIGHT];
		for (int row = 0; row < tiles.GetLength(0); row++) {
			for (int col = 0; col < tiles.GetLength(1); col++) {
				tiles[row, col] = TileType.Wall;
			}
		}

		//Attempt to place a room some number of times
		for (int attempt = 0; attempt < Constants.ROOM_ATTEMPTS; attempt++) {
			AddRoom(RandRoom()); //Creates and adds a random room if it fits in the grid
		}

		//Copy rooms into tile grid
		foreach (Room room in room_list) {
			for (int x = room.LeftBound; x < room.RightBound; x++) {
				for (int y = room.LowerBound; y < room.UpperBound; y++) {
					tiles[x, y] = TileType.Room;
				}
			}
		}

		//Create minimum number of paths
		do {
			Room start = room_list[rng.Next(room_list.Count)];
			Room end;
			do {
				end = room_list[rng.Next(room_list.Count)];
			} while (ReferenceEquals(start, end));
			Hall newPath = new Hall(start, end, tiles);

			//Set tiles to be paths unless they are already something else
			foreach (Coordinate coord in newPath.pathCoords) {
				if (tiles[coord.x, coord.y] == TileType.Wall) {
					tiles[coord.x, coord.y] = TileType.Path;
				}
			}

			//Each path depends on other paths so add it immediately
			//Idea: combine path objects if they intersect.
			List<Hall> absorbed = new List<Hall>(); //Build list of paths to be absorbed
			foreach (Hall other in path_list) {
				if (newPath.Intersects(other) || newPath.ShareEndpoint(other)) { //If they intersect
					absorbed.Add(other);
				}
			}
			//Absorb and remove each path found
			foreach (Hall other in absorbed) {
				newPath.Absorb(other);
				path_list.Remove(other);
			}
			path_list.Add(newPath);

		} while (path_list.Count != 1 || path_list[0].connectedRooms.Count != room_list.Count);


		//Draw outer border of level
		for (int y = 0; y < tiles.GetLength(1); y++) {
			tiles[0, y] = TileType.Border;
		}
		for (int y = 0; y < tiles.GetLength(1); y++) {
			tiles[Constants.FLOOR_WIDTH - 1, y] = TileType.Border;
		}
		for (int x = 0; x < tiles.GetLength(0); x++) {
			tiles[x, 0] = TileType.Border;
		}
		for (int x = 0; x < tiles.GetLength(0); x++) {
			tiles[x, Constants.FLOOR_HEIGHT - 1] = TileType.Border;
		}

		//Place entrance and exit
		do {
			entrance = room_list[rng.Next(room_list.Count)].GetRandCoordinate();
		} while (entrance.IsNextToPath(tiles));
		do {
			exit = room_list[rng.Next(room_list.Count)].GetRandCoordinate();
		} while (exit.IsNextToPath(tiles) || entrance.Equals(exit));
		tiles[entrance.x, entrance.y] = TileType.Entrance;
		tiles[exit.x, exit.y] = TileType.Exit;
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

	//Interaction with coordinate

}
