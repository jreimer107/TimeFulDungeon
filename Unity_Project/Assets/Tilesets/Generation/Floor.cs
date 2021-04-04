using System;
using System.Collections.Generic;
using System.Linq;
using TimefulDungeon.Misc;
using Pathfinding = TimefulDungeon.Misc.PathFinding<TimefulDungeon.Generation.Coordinate>;

namespace TimefulDungeon.Generation {
    public class Floor {
        private readonly int floorHeight;

        //Floor constraints
        private readonly int floorWidth;
        private readonly int maxPaths;

        //Pathing
        private readonly int minPaths;
        private readonly int roomAttempts;
        private readonly float roomSizeDeviation;
        private readonly int roomSizeMax;
        private readonly int roomSizeMean;

        //Room generation
        public Coordinate entrance;
        public Coordinate exit;

        private readonly GenConfig gencfg;
        private readonly List<Path> pathList;
        private readonly Pathfinding pathfinding;
        private readonly Random rng;


        private readonly List<Room> roomList;
        public readonly TileType[,] tiles;


        //Level generation!
        public Floor() {
            //Set up variables
            gencfg = Board.instance.genConfig;

            //Create room list
            roomList = new List<Room>();
            pathList = new List<Path>();
            rng = new Random();
            tiles = new TileType[gencfg.FloorWidth, gencfg.FloorHeight];
            pathfinding = new Pathfinding();
        }

        public void Generate() {
            //Create tile grid
            roomList.Clear();
            pathList.Clear();

            for (var row = 0; row < tiles.GetLength(0); row++)
            for (var col = 0; col < tiles.GetLength(1); col++)
                tiles[row, col] = TileType.Wall;

            //Attempt to place a room some number of times
            for (var attempt = 0; attempt < gencfg.RoomAttempts; attempt++)
                AddRoom(RandRoom()); //Creates and adds a random room if it fits in the grid

            //Copy rooms into tile grid
            foreach (var room in roomList)
                for (var x = room.LeftBound; x < room.RightBound; x++)
                for (var y = room.LowerBound; y < room.UpperBound; y++)
                    tiles[x, y] = TileType.Room;

            //Create minimum number of paths
            var numPaths = 0;
            do {
                var start = roomList[rng.Next(roomList.Count)];
                Room end;
                do {
                    end = roomList[rng.Next(roomList.Count)];
                } while (ReferenceEquals(start, end));

                var newPath = new Path(start, end);

                //Set tiles to be paths unless they are already something else
                foreach (var coord in newPath.pathCoords.Where(coord => IsTileOfType(coord, TileType.Wall)))
                    tiles[coord.x, coord.y] = TileType.Path;

                //Each path depends on other paths so add it immediately
                //Combine path objects if they intersect.
                pathList.RemoveAll(other => newPath.IntersectAndAbsorb(other));

                pathList.Add(newPath);

                numPaths++;
                if (numPaths >= gencfg.MaxPaths) break;
            } while (pathList.Count != 1 || pathList[0].connectedRooms.Count != roomList.Count);


            //Draw outer border of level
            for (var y = 0; y < tiles.GetLength(1); y++) tiles[0, y] = TileType.Border;
            for (var y = 0; y < tiles.GetLength(1); y++) tiles[gencfg.FloorWidth - 1, y] = TileType.Border;
            for (var x = 0; x < tiles.GetLength(0); x++) tiles[x, 0] = TileType.Border;
            for (var x = 0; x < tiles.GetLength(0); x++) tiles[x, gencfg.FloorHeight - 1] = TileType.Border;

            //Place entrance and exit
            //do {
            entrance = roomList[rng.Next(roomList.Count)].GetRandCoordinate();
            //} while (entrance.IsNextToPath(tiles));
            do {
                exit = roomList[rng.Next(roomList.Count)].GetRandCoordinate();
            } while (entrance.Equals(exit));

            tiles[entrance.x, entrance.y] = TileType.Entrance;
            tiles[exit.x, exit.y] = TileType.Exit;
        }


        /// <summary>
        ///     Adds a new Room to the room list if it fits in the floor grid.
        /// </summary>
        /// <param name="newRoom">The new room to be added.</param>
        private void AddRoom(Room newRoom) {
            //Make sure floor is inside the floor border
            if (!IsInsideFloor(newRoom)) return;

            //Make sure room doesn't overap other rooms
            var spaceTaken = roomList.Any(room => OverlapsRoom(newRoom, room));
            if (spaceTaken) return;
            roomList.Add(newRoom);
        }


        /// <summary>
        ///     Checks to see if two rooms are overlapping, including the extra gap between them.
        /// </summary>
        /// <param name="r1">The first room to compare.</param>
        /// <param name="r2">The second room to compare.</param>
        /// <returns>True if the rooms overlap or are within the GAP of each other, false otherwise.</returns>
        private static bool OverlapsRoom(Room r1, Room r2) {
            return !(r1.LeftBound >= r2.RightSpace ||
                     r2.LeftBound >= r1.RightSpace ||
                     r1.UpperBound <= r2.LowerSpace ||
                     r2.UpperBound <= r1.LowerSpace);
        }

        /// <summary>
        ///     Checks to see if a room is within the floor border.
        /// </summary>
        /// <param name="room">The room to check.</param>
        /// <returns>True if the room is completely within the border, false otherwise.</returns>
        private bool IsInsideFloor(Room room) {
            return room.LeftSpace >= 0 &&
                   room.RightSpace <= gencfg.FloorWidth &&
                   room.UpperSpace <= gencfg.FloorHeight &&
                   room.LowerSpace >= 0;
        }


        //Generates a random room
        private Room RandRoom() {
            //Create randomly placed and sized region
            var roomX = rng.Next(gencfg.FloorWidth - 1);
            var roomY = rng.Next(gencfg.FloorHeight - 1);
            int roomW, roomH;
            do {
                roomW = (int) Utils.Gauss(gencfg.RoomSizeMean, gencfg.RoomSizeDeviation);
            } while (roomW < gencfg.RoomSizeMin);

            do {
                roomH = (int) Utils.Gauss(gencfg.RoomSizeMean, gencfg.RoomSizeDeviation);
            } while (roomH < gencfg.RoomSizeMin);

            return new Room(roomX, roomY, roomW, roomH);
        }

        public bool IsTileOfType(Coordinate pos, TileType type) {
            return tiles[pos.x, pos.y] == type;
        }

        public bool IsTileTraversable(Coordinate pos) {
            return IsTileOfType(pos, TileType.Path) || IsTileOfType(pos, TileType.Room);
        }

        /// <summary>
        ///     A* algorithm to find the shortest path between two Coordinates.
        /// </summary>
        /// <param name="start">The coordinate to start from.</param>
        /// <param name="end">The coordinate we'd like to wind up at.</param>
        /// <param name="getSuccessorsFunction">Callback function to get possible successor coordinates.</param>
        /// <param name="getCostFunction">Callback function to get the G value.</param>
        /// <param name="getHeuristicFunction">Callback function to the the H value.</param>
        /// <returns>Returns a HashSet of Coordinates to path between.</returns>
        public HashSet<Coordinate> GetShortestPath(
            Coordinate start,
            Coordinate end,
            Func<Coordinate, Coordinate, Coordinate[]> getSuccessorsFunction,
            Func<Coordinate, Coordinate, Coordinate, float, float> getCostFunction,
            Func<Coordinate, Coordinate, float> getHeuristicFunction) {
            return new HashSet<Coordinate>(pathfinding.AStar(start, end, getSuccessorsFunction, getCostFunction,
                getHeuristicFunction));
        }

        public override string ToString() {
            var str = "";
            for (var i = 0; i < tiles.GetLength(0); i++) {
                for (var j = 0; j < tiles.GetLength(1); j++)
                    str += tiles[i, j] switch {
                        TileType.Border => "B ",
                        TileType.Entrance => "E ",
                        TileType.Exit => "e ",
                        TileType.Path => "P ",
                        TileType.Room => "R ",
                        TileType.Void => "_ ",
                        TileType.Wall => "W ",
                        _ => throw new ArgumentOutOfRangeException()
                    };

                str += "\n";
            }

            return str;
        }
    }
}