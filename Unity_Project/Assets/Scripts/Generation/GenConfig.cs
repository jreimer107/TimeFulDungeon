using UnityEngine;

namespace TimefulDungeon.Generation {
	public class GenConfig : MonoBehaviour {
		//Floor size and tile size
		public int FloorWidth;
		public int FloorHeight;

		//Room generation
		public int RoomSizeMin;
		public int RoomSizeMax;
		public int RoomAttempts;
		public int RoomSizeMean;
		public float RoomSizeDeviation;
		public int RoomGap;

		//Pathing
		public int MaxPaths;
	}
}
