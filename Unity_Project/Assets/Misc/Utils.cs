using System.Collections.Generic;
using TimefulDungeon.Core;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TimefulDungeon.Misc {
    public static class Utils {
        public static readonly Camera MainCamera = Camera.main;

        public static Vector2 GetMouseWorldPosition2D() {
            return MainCamera.ScreenToWorldPoint(Input.mousePosition);
        }

        public static Vector3 GetMouseWorldPosition3D() {
            return MainCamera.ScreenToWorldPoint(Input.mousePosition);
        }

        public static TextMeshPro CreateWorldText(string text, Transform parent = null, Vector2 localPosition = default,
            int fontSize = 40, Color? color = null, TextAnchor textAnchor = TextAnchor.UpperLeft,
            TextAlignmentOptions textAlignment = TextAlignmentOptions.Center, int sortingOrder = 5000) {
            color ??= Color.white;
            return CreateWorldText(parent, text, localPosition, fontSize, (Color) color, textAnchor, textAlignment,
                sortingOrder);
        }

        public static void DrawDebugCircle(Vector2 pos, float radius, Color color) {
            var angle = 0f;
            var segments = radius * 32;
            var deltaAngle = 2 * Mathf.PI / segments;
            var currPoint = Vector2.up * radius;
            var nextPoint = Vector2.zero;


            for (var i = 0; i < segments; i++) {
                angle += deltaAngle;
                nextPoint.x = Mathf.Sin(angle) * radius;
                nextPoint.y = Mathf.Cos(angle) * radius;

                Debug.DrawLine(currPoint + pos, nextPoint + pos, color, 1);

                currPoint = nextPoint;
            }
        }

        public static TextMeshPro CreateWorldText(Transform parent, string text, Vector2 localPosition, int fontSize,
            Color color, TextAnchor textAnchor, TextAlignmentOptions textAlignment, int sortingOrder = 5000) {
            var gameObject = new GameObject("World_Text", typeof(TextMeshPro));
            var transform = gameObject.transform;
            transform.SetParent(parent, false);
            transform.localPosition = localPosition;
            var textMeshPro = gameObject.GetComponent<TextMeshPro>();
            // TextMeshPro.anchor = textAnchor;
            textMeshPro.alignment = textAlignment;
            textMeshPro.text = text;
            textMeshPro.fontSize = fontSize;
            textMeshPro.color = color;
            var meshRenderer = textMeshPro.GetComponent<MeshRenderer>();
            meshRenderer.sortingLayerName = "Sky";
            meshRenderer.sortingOrder = sortingOrder;
            return textMeshPro;
        }

        public static bool PointInBox(Vector2Int point, Vector2Int box) {
            return PointInBox(point.x, point.y, box.x, box.y);
        }

        public static bool PointInBox(int pointX, int pointY, int boxX, int boxY) {
            return
                pointX >= 0 &&
                pointY >= 0 &&
                pointX < boxX &&
                pointY < boxY;
        }

        public static string GetRandomText() {
            const string abc = "abcdefghijklmnopqrstuvwxyz\n\n\n\nABCDEFGHIJKLMNOPQRSTUVWXYZ\t\t\t";
            var text = "";
            for (var i = 0; i < Random.Range(20, 100); i++) text += abc[Random.Range(0, abc.Length)];
            return text;
        }

        /// <summary>
        ///     Maps a position on one vector to an equivalent point on another.
        /// </summary>
        /// <param name="distance">How far down the source vector the point is.</param>
        /// <param name="srcVecStart">Where the source vector starts.</param>
        /// <param name="srcVecEnd">Where the source vector ends.</param>
        /// <param name="dstVecStart">Where the destination vector starts.</param>
        /// <param name="dstVecEnd">Where the destination vector ends.</param>
        /// <returns>A float with the position on the destination vector.</returns>
        public static float Map(float distance, float srcVecStart, float srcVecEnd, float dstVecStart,
            float dstVecEnd) {
            return (distance - srcVecStart) / (srcVecEnd - srcVecStart) * (dstVecEnd - dstVecStart) + dstVecStart;
        }

        public static Vector2 GetNormalPoint(Vector2 vectorStart, Vector2 vectorEnd, Vector2 projSrc) {
            var targetVector = vectorEnd - vectorStart;
            var sourceVector = projSrc - vectorStart;
            return targetVector.Project(sourceVector) + vectorStart;
        }

        public static bool PointOnSegment(Vector2 segmentStart, Vector2 segmentEnd, Vector2 point) {
            var ab = segmentEnd - segmentStart;
            var ac = point - segmentStart;
            var dotAc = Vector2.Dot(ab, ac);
            var dotAb = Vector2.Dot(ab, ab);
            return 0 <= dotAc && dotAc <= dotAb;
        }

        public static float RoundToIncrement(float value, float incrementSize) {
            return Mathf.Round(value / incrementSize) * incrementSize;
        }

        public static float Gauss(float mean, float deviation) {
            float x1, x2, w;
            do {
                x1 = Random.value * 2 - 1;
                x2 = Random.value * 2 - 1;
                w = x1 * x1 + x2 * x2;
            } while (w < 0 || w > 1);

            w = Mathf.Sqrt(-2 * Mathf.Log(w) / w);
            return mean + deviation * x1 * w;
        }

        /// <summary>
        ///     Removes unnecessary positions from A Star result.
        ///     This version uses a matrix of acceptable paths to determine which path points are skippable.
        /// </summary>
        /// <param name="path">A list of Vector2s representing the path.</param>
        /// <returns>A list of Vector2s representing the simplified path.</returns>
        public static List<Vector2Int> Waypointify(Vector2Int[] path, WorldGrid<bool> environment) {
            var waypoints = new List<Vector2Int>();
            var currIndex = path.Length - 1;
            var curr = path[currIndex];
            var end = path[0];
            waypoints.Add(curr);

            while (curr != end) {
                // The next waypoint is the last node after a turn that is still visible from the current waypoint
                var turnIndex = currIndex;
                var turn = path[turnIndex];

                // Find the first node after a turn
                while (curr.x == turn.x || curr.y == turn.y ||
                       Mathf.Abs(turn.x - curr.x) == Mathf.Abs(turn.y - curr.y)) {
                    if (turnIndex == 0) {
                        // Debug.Log("Turn finder reached end of path.");
                        curr = end;
                        break;
                    }

                    turn = path[--turnIndex];
                }

                // If there's a turn on the last position, the inSight check won't happen. Add the second-last position.
                if (turnIndex == 0) waypoints.Add(path[turnIndex + 1]);

                // Find the last node after the turn that is still visible from the current waypoint
                var inSight = true;
                while (inSight && turnIndex > 0) {
                    // Check if the node after the turn is visible from the current waypoint
                    // Check if all nodes between the post-turn node and the current waypoint are walkable
                    // Debug.Log($"Possible waypoint is {turn}, {delta} away, slope of {slope}");
                    var delta = turn - curr;
                    float slope = delta.y / delta.x;
                    if (Mathf.Abs(slope) >= 1) {
                        // If slope > 1, path crosses at least one y border for each x.
                        // So for each y crossed, check that the positions left or right walkable, based on where the entity is
                        // Debug.LogFormat("Slope > 1: {0}", slope);
                        int x, y = 0;
                        float xf;
                        var test = default(Vector2Int);
                        while (y != delta.y) {
                            // Get the exact and rounded x coordinates
                            xf = y / slope;
                            x = Mathf.FloorToInt(xf);
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
                    }
                    else {
                        // Debug.Log("Slope < 1:" + slope);
                        // If slope < 1, path crosses at least one x for each y.
                        // For each x crossed, check that the position up or down is walkable, based on where the entity is
                        int x = 0, y;
                        float yf;
                        var test = default(Vector2Int);
                        while (x != delta.x) {
                            // Get the exact and rounded y coordinates
                            yf = x * slope;
                            y = Mathf.FloorToInt(yf);
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
                    if (inSight)
                        turn = path[--turnIndex];
                    else // Only a bad turn fails this condition, so move one back. The square before a bad turn is a waypoint.
                        turn = path[++turnIndex];
                }

                // Start again at the next waypoint, add it to the waypoint list.
                curr = turn;
                currIndex = turnIndex;
                waypoints.Add(curr);
            }

            return waypoints;
        }
    }
}