using UnityEngine;
using TMPro;

[RequireComponent(typeof(Animator), typeof(TextMeshProUGUI))]
public class DamagePopup : MonoBehaviour {
	private Animator animator;
	private TextMeshProUGUI textMeshPro;
	private Vector2 worldPointPosition = new Vector2(0, 0);
	private RectTransform rectTransform;

	private void Awake() {
		animator = GetComponent<Animator>();
		textMeshPro = GetComponent<TextMeshProUGUI>();
		rectTransform = GetComponent<RectTransform>();

		//Destroy(gameObject, animator.GetCurrentAnimatorStateInfo(0).length);
	}

	private void Update() {
		rectTransform.anchoredPosition = Camera.main.WorldToScreenPoint(worldPointPosition);
		Debug.Log(worldPointPosition);
		Debug.Log(transform.position);
	}



	public void SetUp(string text, Vector2 worldPointPosition) {
		textMeshPro.SetText(text);
		this.worldPointPosition = worldPointPosition;
	}
}
