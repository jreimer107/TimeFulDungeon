using UnityEngine;
using TMPro;
using System;
using Unity.Mathematics;
using VoraUtils;

public class WorldGrid<T> {
	public int width { private set; get; }
	public int height { private set; get; }
	public float cellSize { private set; get; }
	public bool debug = false;
	private Vector2 originPosition;
	private T[,] gridArray;

	public event EventHandler<OnGridChangedEventArgs> OnGridChange;
	public class OnGridChangedEventArgs : EventArgs {
		public int x;
		public int y;
	}

	public T this[int x, int y] {
		get { return Get(x, y); }
		set { Set(x, y, value); }
	}

	public T this[Vector2Int a] {
		get { return gridArray[a.x, a.y]; }
		set { gridArray[a.x, a.y] = value; }
	}

	public delegate T CreateGridObject(WorldGrid<T> worldGrid, int x, int y);

	public WorldGrid(int width, int height, float cellSize, Vector2 originPosition = default(Vector2), CreateGridObject createObject = null, bool debug = false) {
		this.width = width;
		this.height = height;
		this.cellSize = cellSize;
		this.originPosition = originPosition;
		gridArray = new T[width, height];

		if (createObject != null) {
			for (int x = 0; x < width; x++) {
				for (int y = 0; y < height; y++) {
					gridArray[x, y] = createObject(this, x, y);
				}
			}
		}

		if (debug) {
			ShowDebug();
		}
	}

	public void ShowDebug() {
		Debug.Log("Debug drawing!");
		TextMeshPro[,] debugTextArray = new TextMeshPro[width, height];
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				debugTextArray[x, y] = Utils.CreateWorldText(gridArray[x, y]?.ToString() + 
					$"\n({x}, {y})", null, GetWorldPosition(x, y) + new Vector2(cellSize, cellSize) * 0.5f, 4, Color.white, TextAnchor.MiddleCenter);
				debugTextArray[x, y].fontSize = 2;
				Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.white, 100f);
				Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.white, 100f);
			}
		}
		Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, 100f);
		Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, 100f);
		OnGridChange += (object sender, OnGridChangedEventArgs eventArgs) => {
			debugTextArray[eventArgs.x, eventArgs.y].text = gridArray[eventArgs.x, eventArgs.y]?.ToString();// + $"\n({eventArgs.x}, {eventArgs.y})";
		};
	}

	public T[,] GetGrid() => gridArray;

	public Vector2 GetWorldPosition(int x, int y, bool center = false) {
		Vector2 ret = new Vector2(x, y) * cellSize + originPosition;
		return center ? ret + new Vector2(0.5f, 0.5f) : ret;
	}

	public void GetXY(Vector2 worldPosition, out int x, out int y) {
		x = Mathf.FloorToInt((worldPosition - originPosition).x / cellSize);
		y = Mathf.FloorToInt((worldPosition - originPosition).y / cellSize);
	}

	public Vector2Int GetXY(Vector2 worldPosition) {
		return new Vector2Int(
			Mathf.FloorToInt((worldPosition - originPosition).x / cellSize),
			Mathf.FloorToInt((worldPosition - originPosition).y / cellSize)
		);
	}

	public void Set(int x, int y, T value) {
		if (x >= 0 && y >= 0 && x < width && y < height) {
			gridArray[x, y] = value;
			TriggerGridObjectChanged(x, y);
		}
	}

	public void TriggerGridObjectChanged(int x, int y) {
		if (OnGridChange != null) OnGridChange(this, new OnGridChangedEventArgs { x = x, y = y });
	}

	public void Set(Vector2 worldPositon, T value) {
		int x, y;
		GetXY(worldPositon, out x, out y);
		Set(x, y, value);
	}

	public T Get(int x, int y) {
		if (Utils.PointInBox(x, y, width, height)) {
			return gridArray[x, y];
		}
		return default(T);
	}

	public T Get(Vector2 worldPositon) {
		int x, y;
		GetXY(worldPositon, out x, out y);
		return Get(x, y);
	}

}
