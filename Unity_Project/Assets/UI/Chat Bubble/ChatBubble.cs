using TMPro;
using UnityEngine;

namespace TimefulDungeon.UI {
    /// <summary>
    ///     Chat bubble class. Allows creating a chat bubble that will follow gameobjects.
    /// </summary>
    public class ChatBubble : MonoBehaviour {
        #region Static fields and methods

        private static ChatBubble chatBubblePrefab;

        /// <summary>
        ///     Creates a chat bubble.
        /// </summary>
        /// <param name="parent">The gameobject to follow.</param>
        /// <param name="localPosition">Offset from the gameobject's postion.</param>
        /// <param name="text">The text to print.</param>
        public static void Create(Transform parent, Vector3 localPosition, string text) {
            chatBubblePrefab ??= Resources.Load<ChatBubble>("ChatBubble");

            var chatBubble = Instantiate(chatBubblePrefab, parent);
            chatBubble.transform.localPosition = localPosition;
            chatBubble._text = text;
        }

        #endregion

        #region Instance fields and methods

        private string _text;

        private void Start() {
            var backgroundSprite = transform.Find("Background").GetComponent<SpriteRenderer>();
            var textMeshPro = transform.Find("Text").GetComponent<TextMeshPro>();
            var autoType = GetComponent<AutoType>();

            // Get the background the right size
            textMeshPro.text = _text;
            textMeshPro.ForceMeshUpdate();
            var textSize = textMeshPro.GetRenderedValues(false);
            backgroundSprite.size = textSize + new Vector2(0.2f, 0.1f);
            backgroundSprite.transform.localPosition = new Vector3(backgroundSprite.size.x / 2f - 0.1f, 0f);

            // Make background transparent
            backgroundSprite.color = new Color(1, 1, 1, 0);

            // Remove text from chat bubble so that it can be autotyped in
            textMeshPro.text = "";

            // Tween in the alpha and then print the message
            LeanTween.alpha(backgroundSprite.gameObject, 1f, 0.5f).setOnComplete(
                () => autoType.PrintMessage(_text, () => Destroy(gameObject, 3f))
            );
        }

        #endregion
    }
}