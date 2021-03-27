using UnityEngine;

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

	public static Vector2 MinClamp(this Vector2 v, float floor) {
		return v.SmallerThan(floor) ? v.normalized * floor : v;
	}

	public static Vector2 ClampMagnitude(this Vector2 v, float floor = float.MinValue, float ceiling = float.MaxValue) {
		var sqMag = v.sqrMagnitude;
		floor *= floor;
		ceiling *= ceiling;
		if (sqMag < floor) {
			return v.normalized * floor;
		}
		if (sqMag > ceiling) {
			return v.normalized * ceiling;
		}
		return v;
	}

	public static bool IsZero(this Vector2 vector) => vector == Vector2.zero;

	public static Vector2 Position2D(this Transform transform) => transform.position;

	/// <summary>
	/// Checks to see if two points are further apart than a given distance, but in an efficient way.
	/// </summary>
	/// <param name="here">Start of distance to compare.</param>
	/// <param name="there">End of distance to compare.</param>
	/// <param name="compareDistance">How long the distance between the two vectors should be.</param>
	/// <returns>A float. Negative if distance is smaller than the given, 0 if equal, and positive if larger.</returns>
	public static float LazyDistanceCheck(this Vector2 here, Vector2 there, float compareDistance) {
		return LazyDistanceCheck(there - here, compareDistance);
	}


	/// <summary>
	/// Checks to see if two points are further apart than a given distance, but in an efficient way.
	/// </summary>
	/// <param name="distanceVector">Vector representing the distance between two points.</param>
	/// <param name="compareDistance">How long the distance between the two vectors should be.</param>
	/// <returns>A float. Negative if distance is smaller than the given, 0 if equal, and positive if larger.</returns>
	public static float LazyDistanceCheck(this Vector2 distanceVector, float compareDistance) {
		return distanceVector.sqrMagnitude - Mathf.Pow(compareDistance, 2);
	}

	public static bool LargerThan(this Vector2 distanceVector, float compareDistance) {
		return LazyDistanceCheck(distanceVector, compareDistance) > 0;
	}
	
	/// <summary>
	/// Checks if two Vector2 positions are farther than the given distance.
	/// </summary>
	/// <param name="here">First position.</param>
	/// <param name="there">Second position.</param>
	/// <param name="compareDistance">Distance to compare.</param>
	/// <returns>True if the positions are farther apart than the given distance. False otherwise.</returns>
	public static bool FartherThan(this Vector2 here, Vector2 there, float compareDistance) {
		return LazyDistanceCheck(here, there, compareDistance) > 0;
	}

	public static bool SmallerThan(this Vector2 distanceVector, float compareDistance) {
		return LazyDistanceCheck(distanceVector, compareDistance) < 0;
	}

	/// <summary>
	/// Checks if two Vector2 positions are closer than the given distance.
	/// </summary>
	/// <param name="here">First position.</param>
	/// <param name="there">Second position.</param>
	/// <param name="compareDistance">Distance to compare.</param>
	/// <returns>True if the positions are closer together than the given distance. False otherwise.</returns>
	public static bool CloserThan(this Vector2 here, Vector2 there, float compareDistance) {
		return LazyDistanceCheck(here, there, compareDistance) < 0;
	}
}