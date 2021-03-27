using System;
using System.Collections.Generic;
using TimefulDungeon.Core;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TimefulDungeon.Generation {
    public class Board : MonoBehaviour {
        [SerializeField] private Tile roomTile;
        [SerializeField] private Tile borderTile;
        [SerializeField] private Tile pathTile;
        [SerializeField] private Tile entranceTile;
        [SerializeField] private Tile exitTile;
        [SerializeField] private RuleTile wallTile;

        [SerializeField] private Tile debugWallTile;

        [SerializeField] private Tilemap groundTilemap, wallTilemap;

        public GenConfig genConfig;
        private Floor floor;
        
        public void Start() {
            floor = new Floor();
            floor.Generate();
            InstantiateGrid(floor.tiles);
            PlacePlayer();
        }

        private void Update() {
            if (!Input.GetKeyDown(KeyCode.M)) return;
            floor.Generate();
            InstantiateGrid(floor.tiles);
        }

        private void InstantiateGrid(TileType[,] grid) {
            for (var x = 0; x < grid.GetLength(0); x++)
            for (var y = 0; y < grid.GetLength(1); y++) {
                var position = new Vector3Int(x, y, 0);
                switch (grid[x, y]) {
                    case TileType.Room:
                        groundTilemap.SetTile(position, roomTile);
                        wallTilemap.SetTile(position, null);
                        break;
                    case TileType.Path:
                        groundTilemap.SetTile(position, pathTile);
                        break;
                    case TileType.Border:
                        groundTilemap.SetTile(position, borderTile);
                        break;
                    case TileType.Entrance:
                        groundTilemap.SetTile(position, entranceTile);
                        break;
                    case TileType.Exit:
                        groundTilemap.SetTile(position, exitTile);
                        break;
                    case TileType.Wall:
                        if (debugWallTile)
                            wallTilemap.SetTile(position, wallTile);
                        else
                            wallTilemap.SetTile(position, debugWallTile);
                        break;
                    case TileType.Void:
                        break;
                    default:
                        wallTilemap.SetTile(position, null);
                        break;
                }
            }
        }

        private void PlacePlayer() {
            Player.instance.transform.position = new Vector3(floor.entrance.x + 0.5f, floor.entrance.y + 0.5f, 0f);
        }

        public bool IsTileOfType(Coordinate pos, TileType type) {
            return floor.IsTileOfType(pos, type);
        }

        public bool IsTileTraversable(Coordinate pos) {
            return floor.IsTileTraversable(pos);
        }

        public HashSet<Coordinate> GetShortestPath(Coordinate start,
            Coordinate end,
            Func<Coordinate, Coordinate, Coordinate[]> getSuccessorsFunction,
            Func<Coordinate, Coordinate, Coordinate, float, float> getCostFunction,
            Func<Coordinate, Coordinate, float> getHeuristicFunction) {
            return floor.GetShortestPath(start, end, getSuccessorsFunction, getCostFunction, getHeuristicFunction);
        }

        #region Singleton

        public static Board instance;

        private void Awake() {
            if (instance != null)
                Debug.LogWarning("Multiple instances of Grid detected.");
            instance = this;
        }

        #endregion
    }

    public enum TileType {
        Void,
        Room,
        Path,
        Wall,
        Border,
        Entrance,
        Exit
    }
}