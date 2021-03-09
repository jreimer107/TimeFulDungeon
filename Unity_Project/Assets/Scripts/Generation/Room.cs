using UnityEngine;

namespace TimefulDungeon.Generation {
    public class Room {
        private readonly int height;
        private readonly int width;
        
        public Room(int x, int y, int w, int h) {
            LeftBound = x;
            LowerBound = y;
            width = w;
            height = h;
        }

        //Bounds are tiles taken up by the room
        public int UpperBound => LowerBound + height;

        public int LowerBound { get; }

        public int LeftBound { get; }

        public int RightBound => LeftBound + width;

        //Spaces are the areas where another room cannot be (too close to this one)
        public int UpperSpace => UpperBound + Board.instance.genConfig.RoomGap;

        public int LowerSpace => LowerBound - Board.instance.genConfig.RoomGap;

        public int LeftSpace => LeftBound - Board.instance.genConfig.RoomGap;

        public int RightSpace => RightBound + Board.instance.genConfig.RoomGap;


        public Coordinate GetRandCoordinate() {
            var randX = GetRandXPos();
            var randY = GetRandYPos();
            return new Coordinate(randX, randY);
        }

        private int GetRandXPos() {
            return Random.Range(LeftBound, RightBound);
        }

        private int GetRandYPos() {
            return Random.Range(LowerBound, UpperBound);
        }
    }
}