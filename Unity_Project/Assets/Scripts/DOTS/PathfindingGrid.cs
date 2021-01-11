using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Entities;
using System.Collections.Generic;
using VoraUtils;
using System.Linq;
using System;

public class PathfindingGrid : MonoBehaviour {
	[SerializeField] GenConfig genConfig;
	[SerializeField] [Range(0f, 2f)] float UpdateInterval = 0.2f;
	private float intervalCounter = 0;
	private Pathfinding<Coordinate> pathfinding;

	public WorldGrid<bool> grid;

	public int width { get { return grid.width; } }
	public int height { get { return grid.height; } }

	private EntityManager entityManager;

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
		grid = new WorldGrid<bool>(75, 75, 0.5f);
		entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
		pathfinding = new Pathfinding<Coordinate>();
	}

	// Update is called once per frame
	// void Update() {
	// 	if (Input.GetMouseButtonDown(0)) {
	// 		Vector2 mousePos = Utils.GetMouseWorldPosition2D();
	// 		grid.Set(mousePos, !grid.Get(mousePos));
	// 	}
	// }

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

	public void RequestPath(Entity entity, Vector2 start, Vector2 end) {
		if (entity == null) {
			Debug.LogWarning("Pathfinding Grid RequestPath: Null entity!");
			return;
		}
		Vector2Int startInt = GetXY(start);
		Vector2Int endInt = GetXY(end);
		entityManager.AddComponentData(entity, new PathfindingParams
		{
			start = new int2(startInt.x, startInt.y),
			end = new int2(endInt.x, endInt.y)
		});
	}

	public List<Vector2> RequestPath(Vector2 start, Vector2 end) {
		Coordinate startCoord = new Coordinate(GetXY(start));
		Coordinate endCoord = new Coordinate(GetXY(end));
		float GetCost(Coordinate successor, Coordinate current, Coordinate parent, float currentCost) => currentCost + Vector2Int.Distance(current, successor);
		Coordinate[] GetSuccessor(Coordinate curr, Coordinate parent) {
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
		float GetHeuristic(Coordinate a, Coordinate b) => Coordinate.heuristic(a, b);
		
		List<Coordinate> coordinates = pathfinding.AStar(startCoord, endCoord, GetSuccessor, GetCost, GetHeuristic);
		// List<Vector2> waypoints = Utils.Waypointify(coordinates.ConvertAll(a => (Vector2Int)a).ToArray(), grid).ConvertAll(a => GetWorldPosition(a));
		List<Vector2> waypoints = Waypointify(coordinates.ConvertAll(a => (Vector2Int)a).ToArray());
		return waypoints;
	}

	/// <summary>
	/// Removes unnecessary positions from A Star result.
	/// Alternative to Utils version as this uses raycasts instead of the environment matrix.
	/// </summary>
	/// <param name="path">A list of Vector2s representing the path.</param>
	/// <returns>A list of Vector2s representing world coordinates of the simplified path.</returns>
	private List<Vector2> Waypointify(Vector2Int[] path) {
		// Set the current position to the path start (end of array)
		Vector2Int curr = path[path.Length - 1];
		Vector2 currWorld = GetWorldPosition(curr);

		// Set the first potential waypoint to the next spot
		int turnIndex = path.Length - 2;
		Vector2Int turn = path[turnIndex];
		Vector2 turnWorld = GetWorldPosition(turn);

		// Create the list and add the start
		List<Vector2> waypoints = new List<Vector2>{currWorld};
		Vector2Int end = path[0];
		while (curr != end && turnIndex > 0) {
			// Find the first node after a turn
			while (curr.x == turn.x || curr.y == turn.y || math.abs(turn.x - curr.x) == math.abs(turn.y - curr.y)) {
				if (turnIndex == 0) {
					curr = end;
					break;
				}
				turn = path[--turnIndex];
			}

			// If we got through the turn finder, there's a turn on the last spot.
			// Raycast won't add it, so do it here.
			if (turnIndex == 0) {
				waypoints.Add(GetWorldPosition(path[turnIndex + 1]));
			}

			// Find the last node after the turn that is still visible from the current waypoint, this is the next waypoint
			while (turnIndex > 0) {
				RaycastHit2D hit = Physics2D.Raycast(currWorld, turnWorld - currWorld, Vector2.Distance(turnWorld, currWorld), LayerMask.GetMask("Obstacle"));
				if (hit.collider) {
					// Obstacle hit, so spot before this was a waypoint. Move curr to right behind turn, and add to list
					curr =  path[turnIndex + 1];
					currWorld = GetWorldPosition(curr);
					waypoints.Add(currWorld);
					break;
				}
				else {
					// No obstacle, try next spot. Advance turn.
					turn = path[--turnIndex];
					turnWorld = GetWorldPosition(turn);
				}
			}
		}

		// Add the end, as the above loop never reaches it
		waypoints.Add(GetWorldPosition(end));
		// Debug.Log(string.Join(",", waypoints.ConvertAll(a => a.ToString())));
		return waypoints;
	}

}
