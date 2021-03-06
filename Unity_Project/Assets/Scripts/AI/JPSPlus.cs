using System.Collections.Generic;
using VoraUtils;

namespace TimefulDungeon.AI {
	public class JPSPlus {
		public WorldGrid<bool> grid;
		public byte[,] map;
		public int[,][] distanceMap;

		private byte[][] ValidDirLookupTable = new byte[8][];

		private static class Dir {
			public const byte Northwest = 0;
			public const byte North = 1;
			public const byte Northeast = 2;
			public const byte West = 3;
			public const byte East = 4;
			public const byte Southwest = 5;
			public const byte South = 6;
			public const byte Southeast = 7;
		}

		private const byte MovingDown = 1 << 0;
		private const byte MovingRight = 1 << 1;
		private const byte MovingUp = 1 << 2;
		private const byte MovingLeft = 1 << 3;

		public JPSPlus(WorldGrid<bool> grid) {
			this.grid = grid;

			ValidDirLookupTable[Dir.South] = new byte[] {Dir.West, Dir.Southwest, Dir.South, Dir.Southeast, Dir.East};
			ValidDirLookupTable[Dir.Southeast] = new byte[] {Dir.South, Dir.Southeast, Dir.East};
			ValidDirLookupTable[Dir.East] = new byte[] {Dir.South, Dir.Southeast, Dir.East, Dir.Northeast, Dir.North};
			ValidDirLookupTable[Dir.Northeast] = new byte[] {Dir.East, Dir.Northeast, Dir.North};
			ValidDirLookupTable[Dir.North] = new byte[] {Dir.East, Dir.Northeast, Dir.North, Dir.Northwest, Dir.West};
			ValidDirLookupTable[Dir.Northwest] = new byte[] {Dir.North, Dir.Northwest, Dir.West};
			ValidDirLookupTable[Dir.West] = new byte[] {Dir.North, Dir.Northwest, Dir.West, Dir.Southwest, Dir.South};
			ValidDirLookupTable[Dir.Southwest] = new byte[] {Dir.West, Dir.Southwest, Dir.South};
		}

		public void CalculateJumpPointMap() {
			map = new byte[grid.width, grid.height];
			for (int x = 0; x < grid.width; x++) {
				for (int y = 0; y < grid.height; y++) {
					if (IsJumpPoint(x, y, 1, 0)) {
						map[x, y] |= MovingLeft;
					}
					if (IsJumpPoint(x, y, -1, 0)) {
						map[x, y] |= MovingRight;
					}
					if (IsJumpPoint(x, y, 0, 1)) {
						map[x, y] |= MovingDown;
					}
					if (IsJumpPoint(x, y, 0, -1)) {
						map[x, y] |= MovingUp;
					}
				}
			}
		}

		private bool IsJumpPoint(int x, int y, int xdir, int ydir) {
			return
				grid[x - xdir, y - ydir] && // Parent not a wall
				((grid[x + ydir, y + xdir] && !grid[x - xdir + ydir, y - ydir + xdir]) || // 1st forced neighbor
				 (grid[x - ydir, y - xdir] && !grid[x - xdir - ydir, y - ydir - xdir])); // 2nd forced neighbor
		}



	


		/// <summary>
		/// Uses algorithm from here https://github.com/SteveRabin/JPSPlusWithGoalBounding/blob/master/JPSPlusGoalBounding/PrecomputeMap.cpp
		/// </summary>
		public void CalculateDistanceMap() {
			// Create array
			distanceMap = new int[grid.width, grid.height][];
			for (int x = 0; x < grid.width; x++) {
				for (int y = 0; y < grid.height; y++) {
					distanceMap[x, y] = new int[8];
				}
			}

			// Horizontal pass
			for (int y = 0; y < grid.height; y++) {
				// West/left
				int leftCount = -1;
				bool leftJumpPointLastSeen = false;
				for (int x = 0; x < grid.width; x++) {
					if (!grid[x,y]) {
						leftCount = -1;
						leftJumpPointLastSeen = false;
						distanceMap[x, y][Dir.West] = 0;
						continue;
					}
					leftCount++;

					distanceMap[x, y][Dir.West] = leftJumpPointLastSeen ? leftCount : -leftCount;

					if ((map[x, y] & MovingLeft) != 0) {
						leftCount = 0;
						leftJumpPointLastSeen = true;
					}
				}

				// East/right
				int rightCount = -1;
				bool rightJumpPointLastSeen = false;
				for (int x = grid.width - 1; x >= 0; x--) {
					if (!grid[x,y]) {
						rightCount = -1;
						rightJumpPointLastSeen = false;
						distanceMap[x, y][Dir.East] = 0;
						continue;
					}
					rightCount++;

					distanceMap[x, y][Dir.East] = rightJumpPointLastSeen ? rightCount : -rightCount;

					if ((map[x, y] & MovingRight) != 0) {
						rightCount = 0;
						rightJumpPointLastSeen = true;
					}
				}	
			}
		
			// Vertical pass
			for (int x = 0; x < grid.width; x++) {
				// North/up
				int upCount = -1;
				bool upJumpPointLastSeen = false;
				for (int y = 0; y < grid.height; y++) {
					if(!grid[x,y]) {
						upCount = -1;
						upJumpPointLastSeen = false;
						distanceMap[x, y][Dir.North] = 0;
						continue;	
					}
					upCount++;
					distanceMap[x, y][Dir.North] = upJumpPointLastSeen ? upCount : -upCount;
					if ((map[x, y] & MovingUp) != 0) {
						upCount = 0;
						upJumpPointLastSeen = true;
					}
				}

				// South/down
				int downCount = -1;
				bool downJumpPointLastSeen = false;
				for (int y = grid.height - 1; y >= 0; y--) {
					if(!grid[x,y]) {
						downCount = -1;
						downJumpPointLastSeen = false;
						distanceMap[x, y][Dir.South] = 0;
						continue;	
					}
					downCount++;
					distanceMap[x, y][Dir.South] = downJumpPointLastSeen ? downCount : -downCount;
					if ((map[x, y] & MovingDown) != 0) {
						downCount = 0;
						downJumpPointLastSeen = true;
					}
				}
			}

			// Diagonals - Northwest and northeast
			for (int x = 0; x < grid.width; x++) {
				for (int y = 0; y < grid.height; y++) {
					if (!grid[x, y]) {
						//Northwest
						if (x == 0 || y == 0 || grid[x - 1, y] || grid[x, y - 1] || grid[x - 1, y - 1]) {
							distanceMap[x, y][Dir.Northwest] = 0;
						}
						else if (!grid[x - 1, y] && !grid[x, y - 1] && 
						         (distanceMap[x - 1, y - 1][Dir.North] != 0 || 
						          distanceMap[x - 1, y - 1][Dir.West] != 0)) {
							distanceMap[x, y][Dir.Northwest] = 1;
						}
						else {
							int jumpDistance = distanceMap[x - 1, y - 1][Dir.Northwest];
							distanceMap[x, y][Dir.Northwest] = jumpDistance + (jumpDistance > 0 ? 1 : -1);
						}

						// Northeast
						if (x == 0 || y == grid.height - 1 || !grid[x - 1, y] || !grid[x, y + 1] || !grid[x - 1, y + 1]) {
							distanceMap[x, y][Dir.Northeast] = 0;
						}
						else if (!grid[x - 1, y] && !grid[x, y + 1] && 
						         (distanceMap[x - 1, y + 1][Dir.North] != 0 || 
						          distanceMap[x - 1, y + 1][Dir.East] != 0)) {
							distanceMap[x, y][Dir.Northeast] = 1;
						}
						else {
							int jumpDistance = distanceMap[x - 1, y + 1][Dir.Northeast];
							distanceMap[x, y][Dir.Northeast] = jumpDistance + (jumpDistance > 0 ? 1 : -1);
						}
					}
				}
			}

			// Diagonals - Southwest and southeast
			for (int x = grid.width - 1; x >= 0; x--) {
				for (int y = 0; y < grid.height; y++) {
					if (!grid[x, y]) {
						//Southwest
						if (x == grid.width - 1 || y == 0 || grid[x + 1, y] || grid[x, y - 1] || grid[x + 1, y - 1]) {
							distanceMap[x, y][Dir.Southwest] = 0;
						}
						else if (!grid[x + 1, y] && !grid[x, y - 1] && 
						         (distanceMap[x + 1, y - 1][Dir.South] != 0 || 
						          distanceMap[x + 1, y - 1][Dir.West] != 0)) {
							distanceMap[x, y][Dir.Southwest] = 1;
						}
						else {
							int jumpDistance = distanceMap[x + 1, y - 1][Dir.Southwest];
							distanceMap[x, y][Dir.Southwest] = jumpDistance + (jumpDistance > 0 ? 1 : -1);
						}

						// Southeast
						if (x == 0 || y == grid.height - 1 || !grid[x + 1, y] || !grid[x, y + 1] || !grid[x + 1, y + 1]) {
							distanceMap[x, y][Dir.Southeast] = 0;
						}
						else if (!grid[x + 1, y] && !grid[x, y + 1] && 
						         (distanceMap[x + 1, y - 1][Dir.South] != 0 || 
						          distanceMap[x + 1, y - 1][Dir.East] != 0)) {
							distanceMap[x, y][Dir.Southeast] = 1;
						}
						else {
							int jumpDistance = distanceMap[x + 1, y + 1][Dir.Southeast];
							distanceMap[x, y][Dir.Southeast] = jumpDistance + (jumpDistance > 0 ? 1 : -1);
						}
					}
				}
			}
		}
		
		// public void GetPath(Coordinate start, Coordinate end) {
		// 	Dictionary<Coordinate, Coordinate> parents = new Dictionary<Coordinate, Coordinate>();
		// 	Dictionary<Coordinate, float> costs = new Dictionary<Coordinate, float>();
		//
		// 	MinHeap<PathNode<Coordinate>> open = new MinHeap<PathNode<Coordinate>>(32);
		// 	HashSet<Coordinate> closed = new HashSet<Coordinate>();
		//
		// 	open.Add(new PathNode<Coordinate>(start, Coordinate.heuristic(start, end)));
		// 	parents[start] = null;
		// 	costs[start] = 0;
		//
		// 	Coordinate curr = null;
		// 	while(!open.IsEmpty()) {
		// 		curr = open.Pop().pos;
		//
		// 		if (curr == end) {
		// 			break;
		// 		}
		// 		closed.Add(curr);
		//
		// 		// foreach (int direction in ValidDirLookupTable[])
		// 	}
		// }
	}
}