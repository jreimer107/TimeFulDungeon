using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour {
	private Animator animator;
	private TextMeshProUGUI textMeshPro;
	private RectTransform rectTransform;

	[SerializeField] private float yOffset = 0.75f;
	[SerializeField] [Range(0, 1)] private float randomXRange = 0;
	[SerializeField] [Range(0, 1)] private float randomYRange = 0;

	private void Awake() {
		animator = GetComponentInChildren<Animator>();
		textMeshPro = GetComponentInChildren<TextMeshProUGUI>();
		rectTransform = GetComponent<RectTransform>();

		Destroy(gameObject, animator.GetCurrentAnimatorStateInfo(0).length);
	}

	/// <summary>
	/// Sets up the parameters for this damage popup.
	/// </summary>
	/// <param name="text">The text to display.</param>
	/// <param name="worldPointPosition">The position to display the text.</param>
	/// <param name="color">The color of text to display.</param>
	public void SetUp(string text, Vector2 worldPointPosition, Color color) {
		textMeshPro.SetText(text);
		textMeshPro.faceColor = color;
		rectTransform.anchoredPosition = new Vector2(
			worldPointPosition.x + Random.Range(-randomXRange, randomXRange),
			worldPointPosition.y + yOffset + Random.Range(-randomYRange, randomYRange));
	}
}
