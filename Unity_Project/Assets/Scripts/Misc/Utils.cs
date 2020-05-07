using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using TMPro;

public static class Utils {
	public static Vector2 GetMouseWorldPosition2D() {
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
}
