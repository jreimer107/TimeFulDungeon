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
			MinHeap<PathNode> open = new MinHeap<PathNode>(300);
			//Ts that have already been considered and do not have to be considered again
			HashSet<T> closed = new HashSet<T>();

			//Add starting position to closed list
			open.Add(new PathNode(start, GetHeuristicFunction(start, end)));
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
						open.Add(new PathNode(suc, newCost + GetHeuristicFunction(suc, end)));
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

		private struct PathNode : IComparable<PathNode> {
			public T pos { get; }
			public float heuristic { get; }

			public PathNode(T pos, float heuristic) {
				this.pos = pos;
				this.heuristic = heuristic;
			}

			public int CompareTo(PathNode other) {
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
	}
}