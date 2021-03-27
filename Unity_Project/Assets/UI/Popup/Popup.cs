using TMPro;
using UnityEngine;

namespace TimefulDungeon.UI {
    public class Popup : MonoBehaviour {
        // TODO: Prime candidate for object pooling

        #region Static fields and method

        private static Popup popupPrefab;
        
        [Range(0, 3)] [SerializeField] private float yOffset = 1.5f;
        [Range(0, 1)] [SerializeField] private float randomXRange = 0.75f;
        [Range(0, 1)] [SerializeField] private float randomYRange = 0.25f;

        /// <summary>
        ///     Creates a damage popup.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="position">The position to display the text at</param>
        /// <param name="color">The color of the text.</param>
        public static void CreatePopup(string text, Vector2 position, Color color) {
            popupPrefab ??= Resources.Load<Popup>("Popup");
            
            var popup = Instantiate(popupPrefab);
            popup._text = text;
            popup._worldPointPosition = position;
            popup._color = color;
        }

        #endregion

        #region Instance fields and methods

        private Color _color;
        private string _text;
        private Vector2 _worldPointPosition;
        
        private void Start() {
            var textMeshPro = GetComponentInChildren<TextMeshProUGUI>();
            var rectTransform = GetComponent<RectTransform>();

            textMeshPro.SetText(_text);
            textMeshPro.faceColor = _color;
            rectTransform.anchoredPosition = new Vector2(
                _worldPointPosition.x + Random.Range(-randomXRange, randomXRange),
                _worldPointPosition.y + yOffset + Random.Range(-randomYRange, randomYRange));

            LeanTween.moveY(rectTransform, rectTransform.anchoredPosition.y - 1f, 0.15f);
            Destroy(gameObject, 1f);
        }

        #endregion
    }
}