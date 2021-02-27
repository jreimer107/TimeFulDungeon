using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using TMPro;

namespace VoraUtils {
	public static class Utils {
		public static Vector2 GetMouseWorldPosition2D() {
			return Camera.main.ScreenToWorldPoint(Input.mousePosition);
		}

		public static Vector3 GetMouseWorldPosition3D() {
			return Camera.main.ScreenToWorldPoint(Input.mousePosition);
		}

		public static TextMeshPro CreateWorldText(string text, Transform parent = null, Vector2 localPosition = default(Vector2), int fontSize = 40, Color? color = null, TextAnchor textAnchor = TextAnchor.UpperLeft, TextAlignmentOptions textAlignment = TextAlignmentOptions.Center, int sortingOrder = 5000) {
			if (color == null) color = Color.white;
			return CreateWorldText(parent, text, localPosition, fontSize, (Color)color, textAnchor, textAlignment, sortingOrder);
		}

		public static TextMeshPro CreateWorldText(Transform parent, string text, Vector2 localPosition, int fontSize, Color color, TextAnchor textAnchor, TextAlignmentOptions textAlignment, int sortingOrder = 5000) {
			GameObject gameObject = new GameObject("World_Text", typeof(TextMeshPro));
			Transform transform = gameObject.transform;
			transform.SetParent(parent, false);
			transform.localPosition = localPosition;
			TextMeshPro textMeshPro = gameObject.GetComponent<TextMeshPro>();
			// TextMeshPro.anchor = textAnchor;
			textMeshPro.alignment = textAlignment;
			textMeshPro.text = text;
			textMeshPro.fontSize = fontSize;
			textMeshPro.color = color;
			MeshRenderer meshRenderer = textMeshPro.GetComponent<MeshRenderer>();
			meshRenderer.sortingLayerName = "Sky";
			meshRenderer.sortingOrder = sortingOrder;
			return textMeshPro;
		}

		public static float Distance(int2 a, int2 b) => Mathf.Sqrt(Mathf.Pow(a.x - b.x, 2) + Mathf.Pow(a.y - b.y, 2));

		public static int SimpleDistance(int2 a, int2 b) {
			int xDistance = math.abs(a.x - b.x);
			int yDistance = math.abs(a.y - b.y);
			int remainder = math.abs(xDistance - yDistance);
			return 14 * math.min(xDistance, yDistance) + 10 * remainder;
		}

		/// <summary>
		/// Checks to see if two points are further apart than a given distance, but in an efficient way.
		/// </summary>
		/// <param name="here">Start of distance to compare.</param>
		/// <param name="there">End of distance to compare.</param>
		/// <param name="compareDistance">How long the distance between the two vectors should be.</param>
		/// <returns>A float. Negative if distance is smaller than the given, 0 if equal, and positive if larger.</returns>
		public static float SimpleDistanceCheck(Vector2 here, Vector2 there, float compareDistance) {
			return Mathf.Pow(compareDistance, 2) - (there - here).sqrMagnitude;
		}

		public static bool PointInBox(Vector2Int point, Vector2Int box) => PointInBox(point.x, point.y, box.x, box.y);
		public static bool PointInBox(int2 point, int2 box) => PointInBox(point.x, point.y, box.x, box.y);
		public static bool PointInBox(int pointX, int pointY, int boxX, int boxY) {
			return
				pointX >= 0 &&
				pointY >= 0 &&
				pointX < boxX &&
				pointY < boxY;
		}

		public struct Empty {};

		public static string GetRandomText() {
			string abc = "abcdefghijklmnopqrstuvwxyz\n\n\n\nABCDEFGHIJKLMNOPQRSTUVWXYZ\t\t\t";
			string text = "";
			for (int i = 0; i < UnityEngine.Random.Range(20, 100); i++) {
				text += abc[UnityEngine.Random.Range(0, abc.Length)];
			}
			return text;
		}

		/// <summary>
		/// Maps a position on one vector to an equivalent point on another.
		/// </summary>
		/// <param name="distance">How far down the source vector the point is.</param>
		/// <param name="srcVecStart">Where the source vector starts.</param>
		/// <param name="srcVecEnd">Where the source vector ends.</param>
		/// <param name="dstVecStart">Where the destination vector starts.</param>
		/// <param name="dstVecEnd">Where the destination vector ends.</param>
		/// <returns>A float with the position on the destination vector.</returns>
		public static float Map(float distance, float srcVecStart, float srcVecEnd, float dstVecStart, float dstVecEnd) {
			return (distance - srcVecStart) / (srcVecEnd - srcVecStart) * (dstVecEnd - dstVecStart) + dstVecStart;
		}

		public static Vector2 GetNormalPoint(Vector2 vectorStart, Vector2 vectorEnd, Vector2 projSrc) {
			Vector2 targetVector = vectorEnd - vectorStart;
			Vector2 sourceVector = projSrc - vectorStart;
			return targetVector.Project(sourceVector) + vectorStart;
		}

		public static bool PointOnSegment(Vector2 segmentStart, Vector2 segmentEnd, Vector2 point) {
			Vector2 ab = segmentEnd - segmentStart;
			Vector2 ac = point - segmentStart;
			float dotAC = Vector2.Dot(ab, ac);
			float dotAB = Vector2.Dot(ab, ab);
			return 0 <= dotAC && dotAC <= dotAB;
		}

		public static float RoundToIncrement(float value, float incrementSize) {
			return Mathf.Round(value / incrementSize) * incrementSize;
		}

		/// <summary>
		/// Removes unnecessary positions from A Star result.
		/// This version uses a matrix of acceptable paths to determine which path points are skippable.
		/// </summary>
		/// <param name="path">A list of Vector2s representing the path.</param>
		/// <returns>A list of Vector2s representing the simplified path.</returns>
		public static List<Vector2Int> Waypointify(Vector2Int[] path, WorldGrid<bool> environment) {
			List<Vector2Int> waypoints = new List<Vector2Int>();
			int currIndex = path.Length - 1;
			Vector2Int curr = path[currIndex];
			Vector2Int end = path[0];
			waypoints.Add(curr);

			while (curr != end) {
				// The next waypoint is the last node after a turn that is still visible from the current waypoint
				int turnIndex = currIndex;
				Vector2Int turn = path[turnIndex];

				// Find the first node after a turn
				while (curr.x == turn.x || curr.y == turn.y || math.abs(turn.x - curr.x) == math.abs(turn.y - curr.y)) {
					if (turnIndex == 0) {
						// Debug.Log("Turn finder reached end of path.");
						curr = end;
						break;
					}
					turn = path[--turnIndex];
				}

				// If there's a turn on the last position, the inSight check won't happen. Add the second-last position.
				if (turnIndex == 0) {
					waypoints.Add(path[turnIndex + 1]);
				}

				// Find the last node after the turn that is still visible from the current waypoint
				bool inSight = true;
				while (inSight && turnIndex > 0) {
					// Check if the node after the turn is visible from the current waypoint
					// Check if all nodes between the post-turn node and the current waypoint are walkable
					// Debug.Log($"Possible waypoint is {turn}, {delta} away, slope of {slope}");
					Vector2Int delta = turn - curr;
					float slope = delta.y / delta.x;
					if (math.abs(slope) >= 1) {
						// If slope > 1, path crosses at least one y border for each x.
						// So for each y crossed, check that the positions left or right walkable, based on where the entity is
						// Debug.LogFormat("Slope > 1: {0}", slope);
						int x, y = 0;
						float xf;
						Vector2Int test = default(Vector2Int);
						while (y != delta.y) {
							// Get the exact and rounded x coordinates
							xf = (float)y / slope;
							x = (int)math.trunc(xf);
							test.x = curr.x + x;
							test.y = curr.y + y;

							// If exactX < roundedX, entity is on right half of square. Check square left of current pos.
							// If exactX > roundedX, entity is on left half of square. Check square right of current pos.
							// Debug.LogFormat("Testing walkability of ({0}, {1})", x, y);
							if (!environment[test] ||
								xf < x && !environment[test.x - 1, test.y] ||
								xf > x && !environment[test.x + 1, test.y]) {
								inSight = false;
								break;
							}
							
							// Move up or down based on slope
							y += delta.y > 0 ? 1 : -1;
						}
					} else {
						// Debug.Log("Slope < 1:" + slope);
						// If slope < 1, path crosses at least one x for each y.
						// For each x crossed, check that the position up or down is walkable, based on where the entity is
						int x = 0, y;
						float yf;
						Vector2Int test = default(Vector2Int);
						while (x != delta.x) {
							// Get the exact and rounded y coordinates
							yf = x * slope;
							y = (int)math.trunc(yf);
							test.x = curr.x + x;
							test.y = curr.y + y;

							// Debug.LogFormat("Testing walkability of ({0}, {1})", test.x, test.y);
							// If exactY < roundedY, entity is on bottom half of square. Check the square below.
							// If exactY > roundedY, entity is on top half of square. Check the square above.
							if (!environment[test] ||
								yf < y && !environment[test.x, test.y - 1] ||
								yf > y && !environment[test.x, test.y + 1]) {
								inSight = false;
								break;
							}
							
							// Move left or right based on slope
							x += delta.x > 0 ? 1 : -1;
						}
					}

					// If no collision was found, then the path between this post-turn node and the current waypoint is walkable.
					// Try again with the next post-turn node.
					if (inSight) {
						turn = path[--turnIndex];
					} else {
						// Only a bad turn fails this condition, so move one back. The square before a bad turn is a waypoint.
						turn = path[++turnIndex];
					}
				}

				// Start again at the next waypoint, add it to the waypoint list.
				curr = turn;
				currIndex = turnIndex;
				waypoints.Add(curr);
			}
			return waypoints;
		}
	}

	/// <summary>
	/// A* algorithm to find the shortest path between two generic points.
	/// </summary>
	/// <param name="start">The place to start from.</param>
	/// <param name="end">The place we'd like to wind up at.</param>
	/// <param name="GetSuccessorsFunction">Callback function to get possible successor coordinates. Passes self, parent, and grandparent.</param>
	/// <param name="GetCostFunction">Callback function to get the G value. Passes successor, self, parent, and the currenly stored cost.</param>
	/// <param name="GetHeuristicFunction">Callback function to the the H value. Passes self and end.</param>
	/// <returns>Returns a List of T to path between.</returns>
	public class Pathfinding<T> where T: class, IComparable<T>  {
		public List<T> AStar(
			T start,
			T end,
			Func<T, T, T[]> GetSuccessorsFunction,
			Func<T, T, T, float, float> GetCostFunction,
			Func<T, T, float> GetHeuristicFunction
		) {
			Dictionary<T, T> parents = new Dictionary<T, T>();
			Dictionary<T, float> costs = new Dictionary<T, float>();

			//Ts being considered to find the closest path
			MinHeap<PathNode<T>> open = new MinHeap<PathNode<T>>(300);
			//Ts that have already been considered and do not have to be considered again
			HashSet<T> closed = new HashSet<T>();

			//Add starting position to closed list
			open.Add(new PathNode<T>(start, GetHeuristicFunction(start, end)));
			parents[start] = null;
			costs[start] = 0;
			T currPos = null; //tile to analyze
			while (!open.IsEmpty()) {
				currPos = open.Pop().pos;

				//We've added destination to the closed list, found a path
				if (currPos.Equals(end)) {
					break;
				}
				closed.Add(currPos); //Switch square from open to closed list

				foreach (T suc in GetSuccessorsFunction(currPos, parents[currPos])) {
					if (closed.Contains(suc)) {
						continue;
					}

					float newCost = GetCostFunction(suc, currPos, parents[currPos], costs[currPos]);

					//Only edit dictionaries if node is new or better
					float currentSucCost = 0;
					if (!costs.TryGetValue(suc, out currentSucCost) || newCost < currentSucCost) {
						//Add node to open with F = G + H values as priority
						open.Add(new PathNode<T>(suc, newCost + GetHeuristicFunction(suc, end)));
						costs[suc] = newCost;
						parents[suc] = currPos;
					}
				}
			}

			//Now start at end and work backward through parents
			List<T> pathCoords = new List<T>();
			while (currPos != null) {
				pathCoords.Add(currPos);
				currPos = parents[currPos];
			}
			return pathCoords;
		}
	}

	public struct PathNode<T> : IComparable<PathNode<T>> where T: IComparable<T> {
		public T pos { get; }
		public float heuristic { get; }

		public PathNode(T pos, float heuristic) {
			this.pos = pos;
			this.heuristic = heuristic;
		}

		public int CompareTo(PathNode<T> other) {
			if (this.heuristic != other.heuristic) {
				return this.heuristic.CompareTo(other.heuristic);
			} else {
				return this.pos.CompareTo(other.pos);
			}
		}

		public override string ToString() {
			return string.Format("[{0}, {1}]", this.pos, this.heuristic);
		}
	}

	/// <summary>
	/// Class for shaping dot products.
	/// Creates different behaviors for various angles.
	/// Each function inputs two normalized Vector2s and maybe some other values.
	/// Each function outputs a normalized float based on how much the shaping function likes the angle.
	/// </summary>
	public static class ShapingFunctions {
		/// <summary>
		/// Likes small angles, or similar vectors.
		/// </summary>
		/// <param name="to">1st vector</param>
		/// <param name="from">2nd vector</param>
		/// <returns>Shaping coefficient. 1 for same angle, -1 for opposite, 0 for perpendicular.</returns>
		public static float Positive(Vector2 to, Vector2 from) {
			return Vector2.Dot(to, from);
		}

		/// <summary>
		/// Likes large angles, opposite vectors.
		/// </summary>
		/// <param name="to">1st vector</param>
		/// <param name="from">2nd vector</param>
		/// <returns>-1 for same angle, 1 for opposite, 0 for perpendicular.</returns>
		public static float Negative(Vector2 to, Vector2 from) {
			return -Positive(to, from);
		}

		/// <summary>
		/// Likes angles similar to given angle.
		/// </summary>
		/// <param name="to">1st vector</param>
		/// <param name="from">2nd vector</param>
		/// <param name="radians">The preferred angle in radians.</param>
		/// <returns>1 for same as given angle, -1 for opposite, 0 for perpendicular.</returns>
		public static float Rotate(Vector2 to, Vector2 from, float radians) {
			return Positive(to.Rotate(radians), from);
		}

		/// <summary>
		/// Lessens the weights of values the further positive or negative the offset is. For example, an offset
		/// of 1 sets negative results to 0.
		/// </summary>
		/// <param name="to">1st vector</param>
		/// <param name="from">2nd vector</param>
		/// <param name="offset">Offset to apply. Valid results only between 1 and -1.</param>
		/// <returns>The result of the dot product, squished in a certain direction based on the offset.</returns>
		public static float PositiveOffset(Vector2 to, Vector2 from, float offset = 1f) {
			return (Positive(to, from) + offset) / (1 + offset);
		} 

		/// <summary>
		/// Lessens the weights of values the further positive or negative the offset is. For example, an offset
		/// of 1 sets positive results to 0.
		/// </summary>
		/// <param name="to">1st vector</param>
		/// <param name="from">2nd vector</param>
		/// <param name="offset">Offset to apply. Valid results only between 1 and -1.</param>
		/// <returns>The result of the dot product, squished in a certain direction based on the offset.</returns>
		public static float NegativeOffset(Vector2 to, Vector2 from, float offset = 1f) {
			return (Negative(to, from) + offset) / (1 + offset);
		}

		/// <summary>
		/// Likes perpendicular angles.
		/// </summary>
		/// <param name="to">1st vector</param>
		/// <param name="from">2nd vector</param>
		/// <returns>0 for same and opposite angles, 1 for perpendicular.</returns>
		public static float Perpendicular(Vector2 to, Vector2 from) {
			return Mathf.Abs(Rotate(to, from, Mathf.PI / 2));
		}

		public static float FavorSides(Vector2 to, Vector2 from, float favor = 0.5f) {
			// return 1 - Mathf.Abs(Vector2.Dot(to, from) - favor);
			return NegativeOffset(to, from, 1 - favor);
		}

		/// <summary>
		/// Special combination such that same-ish and perpendicular angles are strongly positive,
		/// but opposite directions are still negative.
		/// </summary>
		/// <param name="to"></param>
		/// <param name="from"></param>
		/// <returns></returns>
		public static float AnywhereButBack(Vector2 to, Vector2 from) {
			return Positive(to, from) + 0.5f * Perpendicular(to, from);
		}

		public static float AnywhereButThere(Vector2 to, Vector2 from) {
			return Negative(to, from) + 0.5f * Perpendicular(to, from);
		}
	}
}