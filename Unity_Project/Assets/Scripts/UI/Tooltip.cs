using UnityEngine;
using TMPro;
using System;

/// <summary>
/// Controller for tooltip UI. Uses a TextMeshPro object for cool stuff. Can follow a Vector3 position, and even stick inside the screen bounds.
/// </summary>
public class Tooltip : MonoBehaviour {
	/// <summary>
	/// Whether or not the tooltip should pop to be entirely on the screen if it ordinarily woudln't be.
	/// </summary>
	public bool constrainToScreen = false;
	[SerializeField] [Range(-1f, 1000f)] private float maxWidth = -1f;
	private Func<Vector3> GetTarget;

	private TextMeshProUGUI textMeshPro;
	private RectTransform backgroundTransform;
	private RectTransform textRectTransform;
	private RectTransform canvasRectTransform = null;

#if UNITY_EDITOR
	private bool changedInEditor = false;
#endif

	private void Awake() {
		backgroundTransform = GetComponent<RectTransform>();
		textMeshPro = transform.Find("Text").GetComponent<TextMeshProUGUI>();
		textRectTransform = transform.Find("Text").GetComponent<RectTransform>();

		GetTarget = () => Input.mousePosition / canvasRectTransform.rect.x;
		// InvokeRepeating("randomText", .5f, .5f);
	}

	private void randomText() => SetText(Utils.GetRandomText());


	private void Update() {
		// Scale mouse position to screen
		Vector2 anchoredPosition = Input.mousePosition / canvasRectTransform.localScale.x;

		// Tooltip left screen on right or top
		if (constrainToScreen) {
			anchoredPosition.x = Mathf.Min(anchoredPosition.x, canvasRectTransform.rect.width - backgroundTransform.rect.width);
			anchoredPosition.y = Mathf.Min(anchoredPosition.y, canvasRectTransform.rect.height - backgroundTransform.rect.height);
		}

		// Assign tooltip position
		backgroundTransform.anchoredPosition = anchoredPosition;

#if UNITY_EDITOR
		if (changedInEditor) {
			SetMaxWidth(maxWidth);
			changedInEditor = false;
		}
#endif
	}

#if UNITY_EDITOR
	private void OnValidate() {
		changedInEditor = true;
	}
#endif

	/// <summary>
	/// Sets the canvas to draw upon.
	/// </summary>
	/// <param name="canvasRectTransform">RectTransform of the canvas to draw upon.</param>
	public void SetCanvasRectTransform(RectTransform canvasRectTransform) {
		this.canvasRectTransform = canvasRectTransform;
	}

	/// <summary>
	/// Sets the text content of the tooltip. Text can be formatted for TMP.
	/// </summary>
	/// <param name="tooltipText">The string to put in the TMP object.</param>
	public void SetText(string tooltipText) {
		textMeshPro.SetText(tooltipText);
		textMeshPro.ForceMeshUpdate();

		Vector2 textSize = textMeshPro.GetRenderedValues(false);
		backgroundTransform.sizeDelta = textSize + textRectTransform.anchoredPosition * 2;
	}

	/// <summary>
	/// Sets the max width of the tooltip rather than letting it drag out to infinity.
	/// </summary>
	/// <param name="maxWidth">The width in pixels to use. -1 will disable the max width.</param>
	public void SetMaxWidth(float maxWidth = -1) {
		this.maxWidth = maxWidth;
		if (maxWidth == -1) {
			textMeshPro.enableWordWrapping = false;
		} else {
			textRectTransform.sizeDelta = new Vector2(maxWidth, textRectTransform.rect.height);
			textMeshPro.enableWordWrapping = true;
		}
		SetText(text);
	}

	/// <summary>
	/// Sets the function that the tooltip will use to get the position it should be following/nearby.
	/// </summary>
	/// <param name="targetGetterFunction">Function to call to get the tooltip's position.</param>
	public void SetTargetGetterFunction(Func<Vector3> targetGetterFunction) {
		GetTarget = targetGetterFunction;
	}

	/// <summary>
	/// Wrapper setup function. Calls SetText, SetMaxWidth, and SetTargetGetterFunction.
	/// </summary>
	/// <param name="canvas">Parameter for SetCanvasRectTransform.</param>
	/// <param name="tooltipText">Parameter for SetText.</param>
	/// <param name="maxWidth">Parameter for SetMaxWidth.</param>
	/// <param name="targetGetterFunction">Parameter for SetTargetGetterFunction.</param>
	public void Setup(RectTransform canvas, string tooltipText = "", float maxWidth = -1, Func<Vector3> targetGetterFunction = null) {
		SetCanvasRectTransform(canvas);
		SetText(tooltipText);
		SetMaxWidth(maxWidth);
		SetTargetGetterFunction(targetGetterFunction);
	}

	public void ShowFormat(string title, string content, string redtext = "") {
		string text = $"<size=32>{title}</size>\n{content}\n<color=red>{redtext}</color>";
	}

	/// <summary>
	/// Alternative for SetText. Also can be used to get the text.
	/// </summary>
	/// <returns>The text in the TMP object.</returns>
	public string text { get { return textMeshPro.text; } set { SetText(value); } }

	/// <summary>
	/// Makes the tooltip invisible.
	/// </summary>
	public void Hide() => gameObject.SetActive(false);

	/// <summary>
	/// Makes the tooltip visible.
	/// </summary>
	public void Show() => gameObject.SetActive(true);

	/// <summary>
	/// Permanently makes the tooltip invisible.
	/// </summary>
	public void Destroy() => Destroy(gameObject);
}
