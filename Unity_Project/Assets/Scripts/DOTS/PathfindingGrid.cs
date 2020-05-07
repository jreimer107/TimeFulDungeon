using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

public class PathfindingGrid : MonoBehaviour {
	[SerializeField] GenConfig genConfig;

	private WorldGrid<bool> grid;

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
		grid = new WorldGrid<bool>(75, 75, 1.0f);
	}

	// Update is called once per frame
	void Update() {
		if (Input.GetMouseButtonDown(0)) {
			Vector2 mousePos = Utils.GetMouseWorldPosition2D();
			grid.Set(mousePos, !grid.Get(mousePos));
		}
	}

	private void FixedUpdate() {
		for (int x = 0; x < grid.width; x++) {
			for (int y = 0; y < grid.height; y++) {
				// Check for collisions
				Vector2 worldPos = grid.GetWorldPosition(x, y);
				Vector2 middle = new Vector2(worldPos.x + grid.cellSize / 2, worldPos.y + grid.cellSize / 2);
				int layerMask = LayerMask.GetMask("Obstacle", "Player");
				grid.Set(x, y, Physics2D.OverlapBox(middle, new Vector2(grid.cellSize / 2, grid.cellSize / 2), 0f, layerMask) == null);
			}
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

}
