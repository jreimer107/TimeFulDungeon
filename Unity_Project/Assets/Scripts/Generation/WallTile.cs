using System;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace UnityEngine.Tilemaps {
	static class TileCodes {
		public const byte Full = 0;
		public const byte BackWall = 1;
		public const byte BackCorner = 2;
	}

	[Serializable]
	[CreateAssetMenu(filename = "New Wall Tile", menuName = "Tiles/Wall Tile")]
	public class WallTile : TerrainTile {
		private override void UpdateTile(Vector3Int location, ITilemap tilemap, ref TileData tileData) {
			tileData.transform = Matrix4x4.identity;
			tileData.color = Color.white;

			byte mask = TileValue(tileMap, location + new Vector3Int(0, 1, 0)) ? 1 : 0;
			mask |= TileValue(tileMap, location + new Vector3Int(1, 1, 0)) ? 2 : 0;
			mask |= TileValue(tileMap, location + new Vector3Int(1, 0, 0)) ? 4 : 0;
			mask |= TileValue(tileMap, location + new Vector3Int(1, -1, 0)) ? 8 : 0;
			mask |= TileValue(tileMap, location + new Vector3Int(0, -1, 0)) ? 16 : 0;
			mask |= TileValue(tileMap, location + new Vector3Int(-1, -1, 0)) ? 32 : 0;
			mask |= TileValue(tileMap, location + new Vector3Int(-1, 0, 0)) ? 64 : 0;
			mask |= TileValue(tileMap, location + new Vector3Int(-1, 1, 0)) ? 128 : 0;

			int index = GetIndex(mask);
		}

		private override int GetIndex(byte mask) {
			
		}

	}
}


