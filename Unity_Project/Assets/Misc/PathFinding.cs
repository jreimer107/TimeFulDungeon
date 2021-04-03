using System;
using System.Collections.Generic;
using VoraUtils;

namespace TimefulDungeon.Misc {
    public class PathFinding<T> where T : class, IComparable<T> {
        /// <summary>
        ///     A* algorithm to find the shortest path between two generic points.
        /// </summary>
        /// <param name="start">The place to start from.</param>
        /// <param name="end">The place we'd like to wind up at.</param>
        /// <param name="getSuccessorsFunction">
        ///     Callback function to get possible successor coordinates. Passes self, parent, and
        ///     grandparent.
        /// </param>
        /// <param name="getCostFunction">
        ///     Callback function to get the G value. Passes successor, self, parent, and the currently
        ///     stored cost.
        /// </param>
        /// <param name="getHeuristicFunction">Callback function to the the H value. Passes self and end.</param>
        /// <returns>Returns a List of T to path between.</returns>
          public List<T> AStar(
            T start,
            T end,
            Func<T, T, T[]> getSuccessorsFunction,
            Func<T, T, T, float, float> getCostFunction,
            Func<T, T, float> getHeuristicFunction
        )  {
            var parents = new Dictionary<T, T>();
            var costs = new Dictionary<T, float>();

            //Ts being considered to find the closest path
            var open = new MinHeap<PathNode<T>>(300);
            //Ts that have already been considered and do not have to be considered again
            var closed = new HashSet<T>();

            //Add starting position to closed list
            open.Add(new PathNode<T>(start, getHeuristicFunction(start, end)));
            parents[start] = null;
            costs[start] = 0;
            T currPos = null; //tile to analyze
            while (!open.IsEmpty()) {
                currPos = open.Pop().pos;

                //We've added destination to the closed list, found a path
                if (currPos.Equals(end)) break;
                closed.Add(currPos); //Switch square from open to closed list

                foreach (var suc in getSuccessorsFunction(currPos, parents[currPos])) {
                    if (closed.Contains(suc)) continue;

                    var newCost = getCostFunction(suc, currPos, parents[currPos], costs[currPos]);

                    //Only edit dictionaries if node is new or better
                    if (costs.TryGetValue(suc, out var currentSucCost) && !(newCost < currentSucCost)) continue;
                    
                    //Add node to open with F = G + H values as priority
                    open.Add(new PathNode<T>(suc, newCost + getHeuristicFunction(suc, end)));
                    costs[suc] = newCost;
                    parents[suc] = currPos;
                }
            }

            //Now start at end and work backward through parents
            var pathCoords = new List<T>();
            while (currPos != null) {
                pathCoords.Add(currPos);
                currPos = parents[currPos];
            }

            return pathCoords;
        }
    
        private readonly struct PathNode<E> : IComparable<PathNode<E>> where E : IComparable<E> {
            public E pos { get; }
            private float heuristic { get; }

            public PathNode(E pos, float heuristic) {
                this.pos = pos;
                this.heuristic = heuristic;
            }

            public int CompareTo(PathNode<E> other) {
                return Math.Abs(heuristic - other.heuristic) > 0.01 ? heuristic.CompareTo(other.heuristic) : pos.CompareTo(other.pos);
            }

            public override string ToString() {
                return $"[{pos}, {heuristic}]";
            }
        }
    }
}