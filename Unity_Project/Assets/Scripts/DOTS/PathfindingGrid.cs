using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Entities;

public class PathfindingGrid : MonoBehaviour {
	[SerializeField] GenConfig genConfig;
	[SerializeField] [Range(0f, 2f)] float UpdateInterval = 0.2f;
	private float intervalCounter = 0;

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

	public int2 GetXY(Vector2 worldPosition) => grid.GetXY(worldPosition);
	public void GetXY(Vector2 worldPosition, out int x, out int y) => grid.GetXY(worldPosition, out x, out y);
	public Vector2 GetWorldPosition(int2 xy) => grid.GetWorldPosition(xy.x, xy.y, true);

	public void RequestPath(Entity entity, Vector2 start, Vector2 end) {
		if (entity == null) {
			return;
		}
		entityManager.AddComponentData(entity, new PathfindingParams
		{
			start = GetXY(start),
			end = GetXY(end)
		});
	}

	// private class GridPos {
	// 	private int x, y;

	// 	public
	// }

}
