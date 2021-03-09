using System;
using System.Collections.Generic;
using UnityEngine;

namespace TimefulDungeon.Generation {
    public class Path {
        public HashSet<Room> connectedRooms;
        public HashSet<Coordinate> pathCoords;

        public Path(Room startRoom, Room endRoom) {
            connectedRooms = new HashSet<Room> {startRoom, endRoom};
            Generate(startRoom, endRoom);
        }

        private void Generate(Room startRoom, Room endRoom) {
            Room[] endpoints = {startRoom, endRoom};
            var startPos = startRoom.GetRandCoordinate();
            var endPos = endRoom.GetRandCoordinate();

            Coordinate[] SuccessorsFunction(Coordinate curr, Coordinate parent) => Coordinate.GetValidSuccessorsForPathGen(curr, parent, endpoints);

            pathCoords = Board.instance.GetShortestPath(
                startPos, endPos,
                SuccessorsFunction,
                CostFunction,
                Coordinate.heuristic
            );
        }

        private static float CostFunction(Coordinate suc, Coordinate curr, Coordinate parent, float currCost) {
            //Get G value, adjust to reuse paths and to continue in same direction
            var newCost = currCost + 2;
            if (!Board.instance.IsTileOfType(suc, TileType.Path)) newCost += 7;
            if (parent == null) return newCost;
            if (parent.x == curr.x && curr.x != suc.x || parent.y == curr.y && curr.y != suc.y) newCost++;
            return newCost;
        }

        public bool IntersectAndAbsorb(Path other) {
            //Create hashset union of both connected rooms
            var coordsUnion = new HashSet<Coordinate>(pathCoords);
            var roomsUnion = new HashSet<Room>(connectedRooms);
            coordsUnion.UnionWith(other.pathCoords);
            roomsUnion.UnionWith(other.connectedRooms);

            //If different count in result, at least one room/tile shared, so paths intersect
            if (coordsUnion.Count == pathCoords.Count + other.pathCoords.Count &&
                roomsUnion.Count == connectedRooms.Count + other.connectedRooms.Count) return false;
            pathCoords = coordsUnion;
            connectedRooms = roomsUnion;
            return true;

        }
    }
}