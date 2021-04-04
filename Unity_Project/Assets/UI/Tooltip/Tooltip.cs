using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TimefulDungeon.UI {
	/// <summary>
	///     Controller for tooltip UI. Uses a TextMeshPro object for cool stuff. Can follow a Vector3 position, and even stick
	///     inside the screen bounds.
	/// </summary>
	public class Tooltip : MonoBehaviour {
		/// <summary>
		///     Whether or not the tooltip should pop to be entirely on the screen if it ordinarily wouldn't be.
		/// </summary>
		public bool constrainToScreen = true;

        [SerializeField] [Range(-1, 1000)] private int maxWidth = -1;
        [SerializeField] [Range(0f, 10f)] private float delay = 1f;
        [SerializeField] private RectTransform canvasRectTransform;
        private Image background;
        private RectTransform backgroundTransform;

#if UNITY_EDITOR
        private bool changedInEditor;
#endif

        private Coroutine delayCoroutine;

        private TextMeshProUGUI textMeshPro;
        private RectTransform textRectTransform;

        /// <summary>
        ///     Alternative for SetText. Also can be used to get the text.
        /// </summary>
        /// <value>The string in the TMPro object.</value>
        public string Text => textMeshPro.text;

        /// <summary>
        ///     Alternative for Show and Hide. Can be used to get the current visibility.
        /// </summary>
        /// <value>Whether or not the tooltip is visible.</value>
        private bool Visible => textMeshPro.enabled;

        /// <summary>
        ///     Sets the text content of the tooltip. Text can be formatted for TMP.
        /// </summary>
        /// <param name="tooltipText">The string to put in the TMP object.</param>
        /// <returns>Whether or not the background should be updated.</returns>
        private void SetText(string tooltipText) {
            if (textMeshPro.text != tooltipText) {
                textMeshPro.SetText(tooltipText);
            }
        }

        /// <summary>
        ///     Sets the max width of the tooltip.
        /// </summary>
        /// <param name="newMaxWidth">Max width in pixels.</param>
        /// <returns>Whether or not the background should be updated.</returns>
        private bool SetMaxWidth(int newMaxWidth = -1) {
            if (maxWidth == newMaxWidth) return false;
            maxWidth = newMaxWidth;
            if (newMaxWidth == -1) {
                textMeshPro.enableWordWrapping = false;
            }
            else {
                textRectTransform.sizeDelta = new Vector2(newMaxWidth, textRectTransform.rect.height);
                textMeshPro.enableWordWrapping = true;
            }

            return true;

        }

        /// <summary>
        ///     Wrapper setup function. Calls SetText, SetMaxWidth, and Show.
        /// </summary>
        /// <param name="tooltipText">Parameter for SetText.</param>
        /// <param name="maxWidth">Parameter for SetMaxWidth.</param>
        private void ShowText(string tooltipText, int maxWidth = -1) {
            SetText(tooltipText);
            SetMaxWidth(maxWidth);
            Show();
        }

        /// <summary>
        ///     Updates the background of the tooltip.
        /// </summary>
        private void UpdateBackground() {
            textMeshPro.ForceMeshUpdate();

            var textSize = textMeshPro.GetRenderedValues(false);
            Debug.Log("Setting text size to " + textSize);
            backgroundTransform.sizeDelta = textSize + textRectTransform.anchoredPosition * 2;
        }


        /// <summary>
        ///     Makes the tooltip invisible.
        /// </summary>
        public void Hide() {
            textMeshPro.enabled = false;
            background.enabled = false;
            if (delayCoroutine != null) StopCoroutine(delayCoroutine);
        }

        /// <summary>
        ///     Makes the tooltip visible.
        /// </summary>
        private void Show() {
            textMeshPro.enabled = true;
            background.enabled = true;
            UpdateBackground();
            UpdatePosition();
        }

        private void UpdatePosition() {
            // Scale mouse position to screen
            Vector2 anchoredPosition = Input.mousePosition / canvasRectTransform.localScale.x;

            // Tooltip left screen on right or top
            if (constrainToScreen) {
                anchoredPosition.x = Mathf.Min(anchoredPosition.x,
                    canvasRectTransform.rect.width - backgroundTransform.rect.width);
                anchoredPosition.y = Mathf.Min(anchoredPosition.y,
                    canvasRectTransform.rect.height - backgroundTransform.rect.height);
            }

            // Assign tooltip position
            backgroundTransform.anchoredPosition = anchoredPosition;
        }

        /// <summary>
        ///     Show text with a delay.
        /// </summary>
        /// <param name="tooltipText">Text to show.</param>
        /// <param name="maxWidth">Width of </param>
        public void ShowTextOnDelay(string tooltipText, int maxWidth = -1) {
            if (tooltipText == null) {
                Debug.LogError("Null tool tip text.");
                return;
            }
            delayCoroutine = StartCoroutine(ShowOnDelay(tooltipText, maxWidth));
        }

        private IEnumerator ShowOnDelay(string tooltipText, int maxWidth) {
            yield return new WaitForSeconds(delay);
            ShowText(tooltipText, maxWidth);
        }

        #region Singleton

        public static Tooltip instance;

        private void Awake() {
            if (instance) Debug.LogWarning("Multiple instances of Tooltip detected.");
            instance = this;

            #endregion

            // Get references to stuff we need
            backgroundTransform = GetComponent<RectTransform>();
            background = GetComponent<Image>();
            textMeshPro = transform.Find("Text").GetComponent<TextMeshProUGUI>();
            textRectTransform = transform.Find("Text").GetComponent<RectTransform>();

            // Make the tooltip hidden initially
            Hide();
        }

#if UNITY_EDITOR
        private void Update() {
            if (changedInEditor) {
                if (SetMaxWidth(maxWidth) && Visible) UpdateBackground();
                changedInEditor = false;
            }
        }

        private void OnValidate() {
            changedInEditor = true;
        }
#endif
    }
}