using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Entities;
using System.Collections.Generic;
using VoraUtils;

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
		grid = new WorldGrid<bool>(75, 75, 1.0f);
		entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
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
				new Coordinate(x + 1, y + 1),
				new Coordinate(x, y + 1),
				new Coordinate(x - 1, y + 1),
				new Coordinate(x - 1, y),
				new Coordinate(x - 1, y - 1),
				new Coordinate(x, y - 1),
				new Coordinate(x + 1, y - 1)
			};
			successors.RemoveAll(a => grid[a.x, a.y]);
			return successors.ToArray();
		}
		float GetHeuristic(Coordinate a, Coordinate b) => Coordinate.heuristic(a, b);
		List<Coordinate> coordinates = pathfinding.AStar(startCoord, endCoord, GetSuccessor, GetCost, GetHeuristic);
		List<Vector2> path = new List<Vector2>();
		foreach (Coordinate coordinate in coordinates) {
			path.Add(GetWorldPosition(coordinate));
		}
		return path;
	}

}
