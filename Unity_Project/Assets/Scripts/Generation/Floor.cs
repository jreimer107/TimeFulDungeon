using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class Floor {
	//Floor constraints
	private readonly int FloorWidth;
	private readonly int FloorHeight;

	//Room generation
	public readonly int RoomSizeMin;
	private readonly int RoomSizeMax;
	private readonly int RoomAttempts;
	private readonly int RoomSizeMean;
	private readonly float RoomSizeDeviation;

	//Pathing
	private readonly int MinPaths;
	private readonly int MaxPaths;


	private List<Room> room_list;
	private List<Path> path_list;
	public TileType[,] tiles;
	private Random rng;
	public Coordinate entrance;
	public Coordinate exit;

	private GenConfig gencfg;


	//Level generation!
	public Floor() {
		//Set up variables
		gencfg = Board.instance.genConfig;

		//Create room list
		room_list = new List<Room>();
		path_list = new List<Path>();
		rng = new Random();
		tiles = new TileType[gencfg.FloorWidth, gencfg.FloorHeight];
	}

	public void Generate() {
		//Create tile grid
		room_list.Clear();
		path_list.Clear();

		for (int row = 0; row < tiles.GetLength(0); row++) {
			for (int col = 0; col < tiles.GetLength(1); col++) {
				tiles[row, col] = TileType.Wall;
			}
		}

		//Attempt to place a room some number of times
		for (int attempt = 0; attempt < gencfg.RoomAttempts; attempt++) {
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
		int numPaths = 0;
		do {
			Room start = room_list[rng.Next(room_list.Count)];
			Room end;
			do {
				end = room_list[rng.Next(room_list.Count)];
			} while (ReferenceEquals(start, end));
			Path newPath = new Path(start, end);

			//Set tiles to be paths unless they are already something else
			foreach (Coordinate coord in newPath.pathCoords) {
				if (IsTileOfType(coord, TileType.Wall)) {
					tiles[coord.x, coord.y] = TileType.Path;
				}
			}

			//Each path depends on other paths so add it immediately
			//Combine path objects if they intersect.
			path_list.RemoveAll(other => newPath.IntersectAndAbsorb(other));

			path_list.Add(newPath);

			numPaths++;
			if (numPaths >= gencfg.MaxPaths) {
				break;
			}

		} while (path_list.Count != 1 || path_list[0].connectedRooms.Count != room_list.Count);


		//Draw outer border of level
		for (int y = 0; y < tiles.GetLength(1); y++) {
			tiles[0, y] = TileType.Border;
		}
		for (int y = 0; y < tiles.GetLength(1); y++) {
			tiles[gencfg.FloorWidth - 1, y] = TileType.Border;
		}
		for (int x = 0; x < tiles.GetLength(0); x++) {
			tiles[x, 0] = TileType.Border;
		}
		for (int x = 0; x < tiles.GetLength(0); x++) {
			tiles[x, gencfg.FloorHeight - 1] = TileType.Border;
		}

		//Place entrance and exit
		//do {
		entrance = room_list[rng.Next(room_list.Count)].GetRandCoordinate();
		//} while (entrance.IsNextToPath(tiles));
		do {
			exit = room_list[rng.Next(room_list.Count)].GetRandCoordinate();
		} while (entrance.Equals(exit));
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
				room.RightSpace <= gencfg.FloorWidth &&
				room.UpperSpace <= gencfg.FloorHeight &&
				room.LowerSpace >= 0);
	}


	//Generates a random room
	public Room RandRoom() {
		//Create randomly placed and sized region
		int room_x = rng.Next(gencfg.FloorWidth - 1);
		int room_y = rng.Next(gencfg.FloorHeight - 1);
		int room_w, room_h;
		do {
			room_w = (int)Gauss(gencfg.RoomSizeMean, gencfg.RoomSizeDeviation);
		} while (room_w < gencfg.RoomSizeMin);
		do {
			room_h = (int)Gauss(gencfg.RoomSizeMean, gencfg.RoomSizeDeviation);
		} while (room_h < gencfg.RoomSizeMin);

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

	public bool IsTileOfType(Coordinate pos, TileType type) {
		return tiles[pos.x, pos.y] == type;
	}

	public bool IsTileTraversable(Coordinate pos) {
		return IsTileOfType(pos, TileType.Path) || IsTileOfType(pos, TileType.Room);
	}

	/// <summary>
	/// A* algorithm to find the shortest path between two Coordinates.
	/// </summary>
	/// <param name="start">The coordinate to start from.</param>
	/// <param name="end">The coordinate we'd like to wind up at.</param>
	/// <param name="GetSuccessorsFunction">Callback function to get possible successor coordinates.</param>
	/// <param name="GetCostFunction">Callback function to get the G value.</param>
	/// <param name="GetHeuristicFunction">Callback function to the the H value.</param>
	/// <returns>Returns a HashSet of Coordinates to path between.</returns>
	public HashSet<Coordinate> GetShortestPath(
		Coordinate start,
		Coordinate end,
		Func<Coordinate, Coordinate, Coordinate[]> GetSuccessorsFunction,
		Func<Coordinate, Coordinate, Coordinate, int, int> GetCostFunction,
		Func<Coordinate, Coordinate, int> GetHeuristicFunction) {

		Dictionary<Coordinate, Coordinate> parents = new Dictionary<Coordinate, Coordinate>();
		Dictionary<Coordinate, int> costs = new Dictionary<Coordinate, int>();

		//Coordinates being considered to find the closest path
		MinHeap<PathNode> open = new MinHeap<PathNode>(300);
		//Coordinates that have already been considered and do not have to be considered again
		HashSet<Coordinate> closed = new HashSet<Coordinate>();

		//Add starting position to closed list
		open.Add(new PathNode(start, GetHeuristicFunction(start, end)));
		parents[start] = null;
		costs[start] = 0;
		Coordinate currPos = null; //tile to analyze
		while (!open.IsEmpty()) {
			currPos = open.Pop().pos;

			//We've added destination to the closed list, found a path
			if (currPos.Equals(end)) {
				break;
			}
			closed.Add(currPos); //Switch square from open to closed list

			foreach (Coordinate suc in GetSuccessorsFunction(currPos, parents[currPos])) {
				if (closed.Contains(suc)) {
					continue;
				}

				int newCost = GetCostFunction(suc, currPos, parents[currPos], costs[currPos]);

				//Only edit dictionaries if node is new or better
				int currentSucCost = 0;
				if (!costs.TryGetValue(suc, out currentSucCost) || newCost < currentSucCost) {
					//Add node to open with F = G + H values as priority
					open.Add(new PathNode(suc, newCost + GetHeuristicFunction(suc, end)));
					costs[suc] = newCost;
					parents[suc] = currPos;
				}
			}
		}

		//Now start at end and work backward through parents
		HashSet<Coordinate> pathCoords = new HashSet<Coordinate>();
		while (currPos != null) {
			pathCoords.Add(currPos);
			currPos = parents[currPos];
		}
		return pathCoords;
	}

	public override string ToString() {
		string str = "";
		for (int i = 0; i < tiles.GetLength(0); i++) {
			for (int j = 0; j < tiles.GetLength(1); j++) {
				switch (tiles[i, j]) {
					case TileType.Border:
						str += "B ";
						break;
					case TileType.Entrance:
						str += "E ";
						break;
					case TileType.Exit:
						str += "e ";
						break;
					case TileType.Path:
						str += "P ";
						break;
					case TileType.Room:
						str += "R ";
						break;
					case TileType.Void:
						str += "_ ";
						break;
					case TileType.Wall:
						str += "W ";
						break;
					default:
						break;
				}
			}
			str += "\n";
		}
		return str;
	}
}


public struct PathNode : IComparable<PathNode> {
	public Coordinate pos { get; }
	public int heuristic { get; }

	public PathNode(Coordinate pos, int heuristic) {
		this.pos = pos;
		this.heuristic = heuristic;
	}

	public int CompareTo(PathNode other) {
		if (this.heuristic != other.heuristic) {
			return this.heuristic.CompareTo(other.heuristic);
		} else {
			return this.pos.CompareTo(other.pos);
		}
	}

	public override string ToString() {
		return string.Format("[{0}, {1}]", this.pos, this.heuristic);
	}
}