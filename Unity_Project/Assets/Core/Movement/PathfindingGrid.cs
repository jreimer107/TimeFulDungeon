using System;
using System.Collections.Generic;
using System.Linq;
using TimefulDungeon.Generation;
using UnityEngine;
using Pathfinding = TimefulDungeon.Misc.PathFinding<TimefulDungeon.Generation.Coordinate>;

namespace TimefulDungeon.Core.Movement {
    public class PathfindingGrid : MonoBehaviour {
        private static LayerMask obstacleMask;
        [SerializeField] private bool doPeriodicUpdates;
        [SerializeField] [Range(0f, 2f)] private float updateInterval = 0.2f;
        [SerializeField] private int gridWidth = 100;
        [SerializeField] private int gridHeight = 100;
        [SerializeField] private float cellSize = 0.5f;
        [SerializeField] private bool debug;

        private WorldGrid<bool> _grid;
        private float _intervalCounter;
        private Pathfinding _pathfinding;

        private void Start() {
            _grid = new WorldGrid<bool>(gridWidth, gridHeight, cellSize);
            _pathfinding = new Pathfinding();
            obstacleMask = LayerMask.GetMask("Obstacle");
            UpdateGrid();
            if (doPeriodicUpdates) InvokeRepeating(nameof(UpdateGrid), updateInterval, updateInterval);
        }

        private void Update() {
            if (Input.GetKeyDown(KeyCode.O)) _grid.ShowDebug();
        }

        private void UpdateGrid() {
            var boxSize = Vector2.one * _grid.CellSize / 2;
            for (var x = 0; x < _grid.Width; x++)
            for (var y = 0; y < _grid.Height; y++) {
                // Check for collisions
                var middle = _grid.GetWorldPosition(x, y) + boxSize;
                _grid[x, y] = Physics2D.OverlapBox(middle, boxSize, 0f, obstacleMask);
            }
        }

        private Vector2 GetWorldPosition(Vector2Int xy) {
            return _grid.GetWorldPosition(xy.x, xy.y, true);
        }

        public List<Vector2> RequestPath(Vector2 start, Vector2 end) {
            var startCoord = new Coordinate(_grid.GetXY(start));
            var endCoord = new Coordinate(_grid.GetXY(end));

            static float GetCost(Coordinate successor, Coordinate current, Coordinate parent, float currentCost) {
                return currentCost + Vector2Int.Distance(current, successor);
            }

            static float GetHeuristic(Coordinate a, Coordinate b) {
                return Coordinate.heuristic(a, b);
            }

            Coordinate[] GetSuccessor(Coordinate curr, Coordinate parent) {
                return GetSuccessorsWithJump(curr, endCoord);
            }

            var coordinates = _pathfinding.AStar(startCoord, endCoord, GetSuccessor, GetCost, GetHeuristic);
            var waypoints = Waypointify(coordinates.ConvertAll(a => (Vector2Int) a).ToArray());
            return waypoints;
        }

        private Coordinate[] GetSuccessors(Coordinate curr, Coordinate parent) {
            var x = curr.x;
            var y = curr.y;
            var successors = new List<Coordinate> {
                new Coordinate(x + 1, y),
                new Coordinate(x, y + 1),
                new Coordinate(x - 1, y),
                new Coordinate(x, y - 1)
            };
            if (_grid[x + 1, y] && _grid[x, y + 1]) successors.Add(new Coordinate(x + 1, y + 1));
            if (_grid[x, y + 1] && _grid[x - 1, y]) successors.Add(new Coordinate(x - 1, y + 1));
            if (_grid[x - 1, y] && _grid[x, y - 1]) successors.Add(new Coordinate(x - 1, y - 1));
            if (_grid[x, y - 1] && _grid[x + 1, y]) successors.Add(new Coordinate(x + 1, y - 1));

            successors.RemoveAll(a => !_grid[a.x, a.y]);
            return successors.ToArray();
        }

        private Coordinate[] GetSuccessorsWithJump(Coordinate curr, Coordinate end) {
            var successors = new List<Coordinate>(8) {
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
            while (true) {
                var next = new Coordinate(curr.x + dx, curr.y + dy);

                // If blocked, can't jump
                if (!_grid[next.x, next.y]) return null;

                // If goal, return it
                if (next == end) return end;

                // If next has forced neighbors (or, is a jump point), return it
                if (_grid[next.x + dy, next.y + dx] &&
                    !_grid[next.x - dx + dy, next.y - dy + dx] || // 1st forced neighbor
                    _grid[next.x - dy, next.y - dx] && !_grid[next.x - dx - dy, next.y - dy - dx]
                ) // 2nd forced neighbor
                    return next;

                // Diagonal case
                if (dx == 0 || dy == 0) {
                    curr = next;
                    continue;
                }

                if (!!Jump(next, dx, 0, end) || !!Jump(next, 0, dy, end)) return next;

                // Have not found any forced neighbors or walls, check next node in line
                curr = next;
            }
        }

        /// <summary>
        ///     Removes unnecessary positions from A Star result.
        ///     Alternative to Utils version as this uses raycasts instead of the environment matrix.
        /// </summary>
        /// <param name="path">A list of Vector2s representing the path.</param>
        /// <returns>A list of Vector2s representing world coordinates of the simplified path.</returns>
        private List<Vector2> Waypointify(IReadOnlyList<Vector2Int> path) {
            List<Vector2> waypoints;
            if (path.Count < 3) {
                waypoints = path.Select(GetWorldPosition).ToList();
                waypoints.Reverse();
                return waypoints;
            }

            // Set the current position to the path start (end of array)
            var curr = path[path.Count - 1];
            var currWorld = GetWorldPosition(curr);

            // Set the first potential waypoint to the next spot
            var turnIndex = path.Count - 2;
            var turn = path[turnIndex];

            // Create the list and add the start
            waypoints = new List<Vector2> {currWorld};
            var end = path[0];
            while (curr != end && turnIndex > 0) {
                /* Find the first node after a turn
             * The current tile and the next in the path form a line. Iterate through the following points.
             * The first point after to not be on that line is a turn and needs to be raycast-checked.
             */
                Vector2 dir = turn - curr;
                dir.Normalize();
                var inLine = true;
                while (inLine) {
                    if (turnIndex == 0) {
                        curr = end;
                        break;
                    }

                    turn = path[--turnIndex];
                    Vector2 vector = turn - curr;
                    if (Vector2.Dot(dir, vector) != 0) inLine = false;
                }

                var turnWorld = GetWorldPosition(turn);

                // Find the last node after the turn that is still visible from the current waypoint, this is the next waypoint
                do {
                    var hit = Physics2D.Linecast(currWorld, turnWorld, obstacleMask);
                    if (hit.collider) {
                        // Obstacle hit, so spot before this was a waypoint. Move curr to right behind turn, and add to list
                        curr = path[turnIndex + 1];
                        currWorld = GetWorldPosition(curr);
                        waypoints.Add(currWorld);
                        break;
                    }

                    // No obstacle, try next spot. Advance turn.
                    // If turn on last spot, need to ensure index does not underflow.
                    turn = path[Math.Max(--turnIndex, 0)];
                    turnWorld = GetWorldPosition(turn);
                } while (turnIndex > 0);
            }

            // Add the end, as the above loop never reaches it
            waypoints.Add(GetWorldPosition(end));
            if (debug) Debug.Log(string.Join(",", waypoints.ConvertAll(a => a.ToString())));
            return waypoints;
        }

        #region Singleton

        public static PathfindingGrid instance;

        private void Awake() {
            if (instance != null) Debug.LogWarning("Multiple instances of Pathfinding Grid!");
            instance = this;
        }

        #endregion
    }
}