using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TimefulDungeon.Generation {
	/// <summary>
	///     Simple wrapper class for two integers that make up x and y of a coordinate pair.
	/// </summary>
	public class Coordinate : IComparable<Coordinate>, IEquatable<Coordinate> {
        public readonly int x, y;

        /// <summary>
        ///     Constructor for Coordinate.
        /// </summary>
        /// <param name="xPos">x position integer.</param>
        /// <param name="yPos">y position integer.</param>
        public Coordinate(int xPos, int yPos) {
            x = xPos;
            y = yPos;
        }

        public Coordinate(Vector2Int a) {
            x = a.x;
            y = a.y;
        }

        public int CompareTo(Coordinate other) {
            return x != other.x ? x.CompareTo(other.x) : y.CompareTo(other.y);
        }

        public bool Equals(Coordinate other) {
            return !ReferenceEquals(other, null) && x == other.x && y == other.y;
        }

        public override bool Equals(object obj) {
            return obj is Coordinate coordinate && Equals(coordinate);
        }

        public static bool operator ==(Coordinate a, Coordinate b) {
            return a is { } && (ReferenceEquals(a, b) || a.Equals(b));
        }

        public static bool operator !=(Coordinate a, Coordinate b) {
            return !ReferenceEquals(a, null) && !a.Equals(b);
        }

        public static bool operator !(Coordinate a) {
            return ReferenceEquals(a, null);
        }

        public static bool operator true(Coordinate a) {
            return !ReferenceEquals(a, null);
        }

        public static bool operator false(Coordinate a) {
            return ReferenceEquals(a, null);
        }

        public static Coordinate operator +(Coordinate a, Coordinate b) {
            return new Coordinate(a.x + b.x, a.y + b.y);
        }

        public static Coordinate operator -(Coordinate a, Coordinate b) {
            return new Coordinate(a.x - b.x, a.y - b.y);
        }

        public static implicit operator Vector2Int(Coordinate a) {
            return new Vector2Int(a.x, a.y);
        }

        public override int GetHashCode() {
            var hash = 13;
            hash = hash * 7 + x.GetHashCode();
            hash = hash * 7 + y.GetHashCode();
            return hash;
        }

        public override string ToString() {
            return $"({x}, {y})";
        }

        public static Coordinate[] GetValidSuccessorsForPathGen(Coordinate c, Coordinate p, Room[] endpoints) {
            var successors = new List<Coordinate> {
                new Coordinate(c.x + 1, c.y),
                new Coordinate(c.x, c.y + 1),
                new Coordinate(c.x - 1, c.y),
                new Coordinate(c.x, c.y - 1)
            };
            successors.RemoveAll(x => !x.ValidSuccessor(c, p, endpoints));
            return successors.ToArray();
        }

        //The whole point of this is to avoid 2x2 path boxes, which look ugly
        //To avoid a 2x2 box you need to make sure that:
        //	you don't put a tile in a corner
        //	you don't put two tiles alongside another path
        //This includes the current tile, as that would be a tile in this situation
        //So just don't make a corner (including the current tile)
        private bool ValidSuccessor(Coordinate p, Coordinate gp, Room[] endpoints) {
            //Check if in endpoint room (don't check for rules anymore)
            if (InRooms(endpoints)) return true;

            //Get surroundings
            var adjacents = new List<Coordinate> {
                new Coordinate(x + 1, y),
                new Coordinate(x + 1, y + 1),
                new Coordinate(x, y + 1),
                new Coordinate(x - 1, y + 1),
                new Coordinate(x - 1, y),
                new Coordinate(x - 1, y - 1),
                new Coordinate(x, y - 1),
                new Coordinate(x + 1, y - 1)
            };

            //Check if next to non-endpoint room (cannot touch)
            if (adjacents.Any(a => !a.IsInBounds() || Board.instance.IsTileOfType(a, TileType.Room) && !a.InRooms(endpoints))) {
                return false;
            }

            //Check which adjacent tiles are taken
            var taken = new bool[8];
            for (var i = 0; i < 8; i++)
                taken[i] = !adjacents[i].IsInBounds() ||
                           !Board.instance.IsTileOfType(adjacents[i], TileType.Wall) ||
                           adjacents[i].Equals(p) ||
                           gp != null && adjacents[i].Equals(gp);

            //If we are in a corner we make a box. Don't want this
            return (!taken[0] || !taken[1] || !taken[2]) && (!taken[2] || !taken[3] || !taken[4]) &&
                   (!taken[4] || !taken[5] || !taken[6]) && (!taken[6] || !taken[7] || !taken[0]);
        }

        private bool InRooms(IEnumerable<Room> checkRooms) {
            return checkRooms.Any(InRoom);
        }

        private bool InRoom(Room room) {
            return x >= room.LeftBound &&
                   x < room.RightBound &&
                   y >= room.LowerBound &&
                   y < room.UpperBound;
        }

        private bool IsInBounds() {
            var gencfg = Board.instance.genConfig;
            return !(
                x < 0 ||
                x >= gencfg.FloorWidth ||
                y < 0 ||
                y >= gencfg.FloorHeight
            );
        }

        public static float heuristic(Coordinate a, Coordinate b) {
            return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
        }
    }
}