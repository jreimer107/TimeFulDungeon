using System;
using TMPro;
using UnityEngine;
using VoraUtils;

namespace TimefulDungeon.Core {
    public class WorldGrid<T> {
        public delegate T CreateGridObject(WorldGrid<T> worldGrid, int x, int y);

        public bool debug = false;
        private readonly T[,] gridArray;
        private readonly Vector2 originPosition;

        public WorldGrid(int width, int height, float cellSize, Vector2 originPosition = default,
            CreateGridObject createObject = null, bool debug = false) {
            Width = width;
            Height = height;
            this.CellSize = cellSize;
            this.originPosition = originPosition;
            gridArray = new T[width, height];

            if (createObject != null)
                for (var x = 0; x < width; x++)
                for (var y = 0; y < height; y++)
                    gridArray[x, y] = createObject(this, x, y);

            if (debug) ShowDebug();
        }

        public int Width { get; }
        public int Height { get; }
        public float CellSize { get; }

        public T this[int x, int y] {
            get => Get(x, y);
            set => Set(x, y, value);
        }

        public T this[Vector2Int a] {
            get => gridArray[a.x, a.y];
            set => gridArray[a.x, a.y] = value;
        }

        public event EventHandler<OnGridChangedEventArgs> OnGridChange;

        public void ShowDebug() {
            Debug.Log("Debug drawing!");
            var debugTextArray = new TextMeshPro[Width, Height];
            for (var x = 0; x < Width; x++)
            for (var y = 0; y < Height; y++) {
                debugTextArray[x, y] = Utils.CreateWorldText(gridArray[x, y] +
                                                             $"\n({x}, {y})", null,
                    GetWorldPosition(x, y) + new Vector2(CellSize, CellSize) * 0.5f, 4, Color.white,
                    TextAnchor.MiddleCenter);
                debugTextArray[x, y].fontSize = 2;
                Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.white, 100f);
                Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.white, 100f);
            }

            Debug.DrawLine(GetWorldPosition(0, Height), GetWorldPosition(Width, Height), Color.white, 100f);
            Debug.DrawLine(GetWorldPosition(Width, 0), GetWorldPosition(Width, Height), Color.white, 100f);
            OnGridChange += (sender, eventArgs) => {
                debugTextArray[eventArgs.x, eventArgs.y].text =
                    gridArray[eventArgs.x, eventArgs.y]?.ToString();
            };
        }

        public Vector2 GetWorldPosition(int x, int y, bool center = false) {
            var ret = new Vector2(x, y) * CellSize + originPosition;
            return center ? ret + Vector2.one * CellSize / 2 : ret;
        }

        public void GetXY(Vector2 worldPosition, out int x, out int y) {
            x = Mathf.FloorToInt((worldPosition - originPosition).x / CellSize);
            y = Mathf.FloorToInt((worldPosition - originPosition).y / CellSize);
        }

        public Vector2Int GetXY(Vector2 worldPosition) {
            return new Vector2Int(
                Mathf.FloorToInt((worldPosition - originPosition).x / CellSize),
                Mathf.FloorToInt((worldPosition - originPosition).y / CellSize)
            );
        }

        public void Set(int x, int y, T value) {
            if (x < 0 || y < 0 || x >= Width || y >= Height) return;
            gridArray[x, y] = value;
            TriggerGridObjectChanged(x, y);
        }

        private void TriggerGridObjectChanged(int x, int y) {
            OnGridChange?.Invoke(this, new OnGridChangedEventArgs {x = x, y = y});
        }

        public void Set(Vector2 worldPositon, T value) {
            GetXY(worldPositon, out var x, out var y);
            Set(x, y, value);
        }

        public T Get(int x, int y) {
            return Utils.PointInBox(x, y, Width, Height) ? gridArray[x, y] : default;
        }

        public T Get(Vector2 worldPositon) {
            GetXY(worldPositon, out var x, out var y);
            return Get(x, y);
        }

        public class OnGridChangedEventArgs : EventArgs {
            public int x;
            public int y;
        }
    }
}