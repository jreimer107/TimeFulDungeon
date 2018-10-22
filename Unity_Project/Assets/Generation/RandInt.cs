using Random = UnityEngine.Random;

public class RandRange {

	public int RandInt(int min, int max) {
		return Random.Range(min, max);
	}
	public float RandFloat(float min, float max) {
		return Random.Range(min, max);
	}

}
