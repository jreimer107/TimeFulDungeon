using TMPro;
using UnityEngine;

public class Popup : MonoBehaviour {
    // TODO: Prime candidate for object pooling

    #region Static fields and method

    private static Popup popupPrefab;
    private const float YOffset = 1.5f;
    [Range(0, 1)] private const float RandomXRange = 0.75f;
    [Range(0, 1)] private const float RandomYRange = 0.25f;

    /// <summary>
    ///     Creates a damage popup.
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <param name="position">The position to display the text at</param>
    /// <param name="color">The color of the text.</param>
    public static void CreatePopup(string text, Vector2 position, Color color) {
        popupPrefab ??= Resources.Load<Popup>("Prefabs/Popup");
        
        var popup = Instantiate(popupPrefab);
        popup.text = text;
        popup.worldPointPosition = position;
        popup.color = color;
    }

    #endregion

    #region Instance fields and methods

    private Color color;
    private string text;
    private Vector2 worldPointPosition;

    private void Start() {
        var textMeshPro = GetComponentInChildren<TextMeshProUGUI>();
        var rectTransform = GetComponent<RectTransform>();

        textMeshPro.SetText(text);
        textMeshPro.faceColor = color;
        rectTransform.anchoredPosition = new Vector2(
            worldPointPosition.x + Random.Range(-RandomXRange, RandomXRange),
            worldPointPosition.y + YOffset + Random.Range(-RandomYRange, RandomYRange));

        LeanTween.moveY(rectTransform, rectTransform.anchoredPosition.y - 1f, 0.15f);
        Destroy(gameObject, 1f);
    }

    #endregion
}