using System;
using System.Collections.Generic;
using System.Linq;
using TimefulDungeon.Generation;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using VoraUtils;

namespace TimefulDungeon.AI {
	public class PathfindingGrid : MonoBehaviour {
		[SerializeField] private GenConfig genConfig;
		[SerializeField] [Range(0f, 2f)] private float UpdateInterval = 0.2f;
		[SerializeField] private int gridWidth = 100;
		[SerializeField] private int gridHeight = 100;
		[SerializeField] private float cellSize = 0.5f;

		private float intervalCounter = 0;
		private Pathfinding<Coordinate> pathfinding;

		public WorldGrid<bool> grid;

		public int width { get { return grid.width; } }
		public int height { get { return grid.height; } }

		#region Singleton
		public static PathfindingGrid Instance;
		private void Awake() {
			if (Instance != null) {
				Debug.LogWarning("Multiple instances of Pathfinding Grid!");
			}
			Instance = this;
		}
		#endregion


		// Start is called before the first frame update
		void Start() {
			grid = new WorldGrid<bool>(gridWidth, gridHeight, cellSize);
			pathfinding = new Pathfinding<Coordinate>();
		}

		private void Update() {
			if (Input.GetKeyDown(KeyCode.L)) {
				JumpPointTest();
			}
			if (Input.GetKeyDown(KeyCode.O)) {
				grid.ShowDebug();
			}
		}

		private void FixedUpdate() {
			if (intervalCounter >= UpdateInterval) {
				for (int x = 0; x < grid.width; x++) {
					for (int y = 0; y < grid.height; y++) {
						// Check for collisions
						Vector2 worldPos = grid.GetWorldPosition(x, y);
						Vector2 middle = new Vector2(worldPos.x + grid.cellSize / 2, worldPos.y + grid.cellSize / 2);
						int layerMask = LayerMask.GetMask("Obstacle");
						grid.Set(x, y, Physics2D.OverlapBox(middle, new Vector2(grid.cellSize / 2, grid.cellSize / 2), 0f, layerMask) == null);
					}
				}
				intervalCounter = 0;
			} else {
				intervalCounter += Time.fixedDeltaTime;
			}
		}

		private class JumpPointSquare {
			private int[] values;
			public JumpPointSquare(int[] values) {
				this.values = values;
			}
			public override string ToString() {
				return $"{values[0]}  {values[1]}  {values[2]}\n{values[3]}     {values[4]}\n{values[5]}  {values[6]}  {values[7]}";
				// return $"  {value & 4}  \n{value & 8}   {value & 2}\n  {value & 1}  ";
				// return value.ToString();
			}
		}

		private WorldGrid<JumpPointSquare> jumpPoints;
		public void JumpPointTest() {
			Debug.Log("Creating new worldgrid");
			jumpPoints = new WorldGrid<JumpPointSquare>(width, height, cellSize);
			JPSPlus jps = new JPSPlus(grid);
			jps.CalculateJumpPointMap();
			jps.CalculateDistanceMap();
			for (int i = 0; i < width; i++) {
				for (int j = 0; j < height; j++) {
					jumpPoints[i, j] = new JumpPointSquare(jps.distanceMap[i, j]);
				}
			}
			jumpPoints.ShowDebug();
		}

		public NativeArray<bool> GetNativeArray(Allocator allocator) {
			NativeArray<bool> nativeArray = new NativeArray<bool>(width * height, allocator);
			for (int x = 0; x < grid.width; x++) {
				for (int y = 0; y < grid.height; y++) {
					nativeArray[x * width + y] = grid.Get(x, y);
				}
			}
			return nativeArray;
		}

		public Vector2Int GetXY(Vector2 worldPosition) => grid.GetXY(worldPosition);
		public void GetXY(Vector2 worldPosition, out int x, out int y) => grid.GetXY(worldPosition, out x, out y);
		public Vector2 GetWorldPosition(Vector2Int xy) => grid.GetWorldPosition(xy.x, xy.y, true);
		public Vector2 GetWorldPosition(int2 xy) => grid.GetWorldPosition(xy.x, xy.y, true);

		public List<Vector2> RequestPath(Vector2 start, Vector2 end) {
			Coordinate startCoord = new Coordinate(GetXY(start));
			Coordinate endCoord = new Coordinate(GetXY(end));
			float GetCost(Coordinate successor, Coordinate current, Coordinate parent, float currentCost) => currentCost + Vector2Int.Distance(current, successor);
			float GetHeuristic(Coordinate a, Coordinate b) => Coordinate.heuristic(a, b);
			Coordinate[] GetSuccessor(Coordinate curr, Coordinate parent) => GetSuccessorsWithJump(curr, endCoord);
		
			List<Coordinate> coordinates = pathfinding.AStar(startCoord, endCoord, GetSuccessor, GetCost, GetHeuristic);
			// List<Vector2> waypoints = Utils.Waypointify(coordinates.ConvertAll(a => (Vector2Int)a).ToArray(), grid).ConvertAll(a => GetWorldPosition(a));
			List<Vector2> waypoints = Waypointify(coordinates.ConvertAll(a => (Vector2Int)a).ToArray());
			// List<Vector2> waypoints = WaypointifyJPS(coordinates.ConvertAll(a => (Vector2Int)a).ToArray());
			// List<Vector2> waypoints = coordinates.ConvertAll(a => GetWorldPosition(a));
			return waypoints;
		}

		private Coordinate[] GetSuccessors(Coordinate curr, Coordinate parent) {
			int x = curr.x;
			int y = curr.y;
			List<Coordinate> successors = new List<Coordinate> {
				new Coordinate(x + 1, y),
				new Coordinate(x, y + 1),
				new Coordinate(x - 1, y),
				new Coordinate(x, y - 1)
			};
			if (grid[x + 1, y] && grid[x, y + 1]) {
				successors.Add(new Coordinate(x + 1, y + 1));
			}
			if (grid[x, y + 1] && grid[x - 1, y]) {
				successors.Add(new Coordinate(x - 1, y + 1));
			}
			if (grid[x - 1, y] && grid[x, y - 1]) {
				successors.Add(new Coordinate(x - 1, y - 1));
			}
			if (grid[x, y - 1] && grid[x + 1, y]) {
				successors.Add(new Coordinate(x + 1, y - 1));
			}

			successors.RemoveAll(a => !grid[a.x, a.y]);
			return successors.ToArray();
		}

		private Coordinate[] GetSuccessorsWithJump(Coordinate curr, Coordinate end) {
			List<Coordinate> successors = new List<Coordinate>(8) {
				Jump(curr, 1, 0, end),
				Jump(curr, 1, 1, end),
				Jump(curr, 0, 1, end),
				Jump(curr, -1, 1, end),
				Jump(curr, -1, 0, end),
				Jump(curr, -1, -1, end),
				Jump(curr, 0, -1, end),
				Jump(curr, 1, -1, end)
			};
	
			successors.RemoveAll(x => !x);
			return successors.ToArray();
		}

		private Coordinate Jump(Coordinate curr, int dx, int dy, Coordinate end) {
			Coordinate next = new Coordinate(curr.x + dx, curr.y + dy);

			// If blocked, can't jump
			if (!grid[next.x, next.y]) {
				return null;
			}

			// If goal, return it
			if (next == end) {
				return end;
			}

			// If next has forced neighbors (or, is a jump point), return it
			if ((grid[next.x + dy, next.y + dx] && !grid[next.x - dx + dy, next.y - dy + dx]) || // 1st forced neighbor
			    (grid[next.x - dy, next.y - dx] && !grid[next.x - dx - dy, next.y - dy - dx]))  { // 2nd forced neighbor
				return next;
			}

			// Diagonal case
			if (dx != 0 && dy != 0) {
				if (!!Jump(next, dx, 0, end) || !!Jump(next, 0, dy, end)) {
					return next;
				}
			}

			// Have not found any forced neighbors or walls, check next node in line
			return Jump(next, dx, dy, end);
		}

		/// <summary>
		/// Removes unnecessary positions from A Star result.
		/// Alternative to Utils version as this uses raycasts instead of the environment matrix.
		/// </summary>
		/// <param name="path">A list of Vector2s representing the path.</param>
		/// <returns>A list of Vector2s representing world coordinates of the simplified path.</returns>
		private List<Vector2> Waypointify(Vector2Int[] path) {
			List<Vector2> waypoints;
			if (path.Length < 3) {
				waypoints = path.Select(x => GetWorldPosition(x)).ToList<Vector2>();
				waypoints.Reverse();
				return waypoints;
			}

			// Set the current position to the path start (end of array)
			Vector2Int curr = path[path.Length - 1];
			Vector2 currWorld = GetWorldPosition(curr);

			// Set the first potential waypoint to the next spot
			int turnIndex = path.Length - 2;
			Vector2Int turn = path[turnIndex];
			Vector2 turnWorld = GetWorldPosition(turn);

			// Create the list and add the start
			waypoints = new List<Vector2>{currWorld};
			Vector2Int end = path[0];
			int layerMask = LayerMask.GetMask("Obstacle");
			while (curr != end && turnIndex > 0) {
				/* Find the first node after a turn
			 * The current tile and the next in the path form a line. Iterate through the following points.
			 * The first point after to not be on that line is a turn and needs to be raycast-checked.
			 */
				Vector2 dir = turn - curr;
				float length = dir.magnitude;
				dir.Normalize();
				bool inLine = true;
				while(inLine) {
					if (turnIndex == 0) {
						curr = end;
						break;
					}
					turn = path[--turnIndex];
					Vector2 vector = turn - curr;
					if (Vector2.Dot(dir, vector) != 0) {
						inLine = false;
					}

				}
				turnWorld = GetWorldPosition(turn);

				// Find the last node after the turn that is still visible from the current waypoint, this is the next waypoint
				do {
					RaycastHit2D hit = Physics2D.Raycast(currWorld, turnWorld - currWorld, Vector2.Distance(turnWorld, currWorld), layerMask);
					if (hit.collider) {
						// Obstacle hit, so spot before this was a waypoint. Move curr to right behind turn, and add to list
						curr =  path[turnIndex + 1];
						currWorld = GetWorldPosition(curr);
						waypoints.Add(currWorld);
						break;
					}
					else {
						// No obstacle, try next spot. Advance turn.
						// If turn on last spot, need to ensure index does not underflow.
						turn = path[Math.Max(--turnIndex, 0)];
						turnWorld = GetWorldPosition(turn);
					}
				} while (turnIndex > 0);
			}

			// Add the end, as the above loop never reaches it
			waypoints.Add(GetWorldPosition(end));
			// Debug.Log(string.Join(",", waypoints.ConvertAll(a => a.ToString())));
			return waypoints;
		}
	}
}
