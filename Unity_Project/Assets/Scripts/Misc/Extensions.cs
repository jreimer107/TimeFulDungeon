using UnityEngine;
using  Unity.Mathematics;

public static class Extensions {
	public static int Clamp(this int value, int inclusiveMin,
		int exclusiveMax) {
		if (inclusiveMin == exclusiveMax) {
			return inclusiveMin;
		} else if (value < inclusiveMin) {
			return inclusiveMin;
		} else if (value >= exclusiveMax) {
			return exclusiveMax - 1;
		} else {
			return value;
		}
	}

	public static int Min(this int x, int y) {
		return (x > y) ? y : x;
	}

	public static int Max(this int x, int y) {
		return (x > y) ? x : y;
	}

	/// <summary>
	/// Projects the given vector onto this vector.
	/// </summary>
	/// <param name="target">The vector that will be projected onto.</param>
	/// <param name="source">The vector that will be projected onto this vector.</param>
	/// <returns>A projected vector.</returns>
	public static Vector2 Project(this Vector2 target, Vector2 source) {
		return target * source.magnitude * Vector2.Angle(target, source);
	}

	public static Vector2 Rotate(this Vector2 v, float delta) {
        return new Vector2(
            v.x * Mathf.Cos(delta) - v.y * Mathf.Sin(delta),
            v.x * Mathf.Sin(delta) + v.y * Mathf.Cos(delta)
        );
    }
}

public static class ExtensionMethods {
	
}