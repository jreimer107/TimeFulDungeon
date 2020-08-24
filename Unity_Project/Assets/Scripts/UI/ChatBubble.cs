using UnityEngine;
using TMPro;

/// <summary>
/// Chat bubble class. Allows creating a chat bubble that will follow gameobjects.
/// </summary>
public class ChatBubble : MonoBehaviour {

	private SpriteRenderer backgroundSprite;
	private TextMeshPro textMeshPro;
	private AutoType autoType;

	/// <summary>
	/// Creates a chat bubble.
	/// </summary>
	/// <param name="parent">The gameobject to follow.</param>
	/// <param name="localPosition">Offset from the gameobject's postion.</param>
	/// <param name="text">The text to print.</param>
	public static void Create(Transform parent, Vector3 localPosition, string text) {
		Transform chatBubbleTrfm = Instantiate(Resources.Load<Transform>("Prefabs/ChatBubble"), parent);
		chatBubbleTrfm.localPosition = localPosition;
		chatBubbleTrfm.GetComponent<ChatBubble>().SetUp(text);
	}

	private void Awake() {
		backgroundSprite = transform.Find("Background").GetComponent<SpriteRenderer>();
		textMeshPro = transform.Find("Text").GetComponent<TextMeshPro>();
		autoType = GetComponent<AutoType>();
	}

	private void Start() {
		// Make background transparent
		backgroundSprite.color = new Color(1, 1, 1, 0);

		// Remove text from chat bubble so that it can be autotyped in
		string text = textMeshPro.text;
		textMeshPro.text = "";

		// Tween in the alpha and then print the message
		LeanTween.alpha(backgroundSprite.gameObject, 1f, 0.5f).setOnComplete(
			() => autoType.PrintMessage(text, () => Destroy(gameObject, 3f))
		);
	}

	private void SetUp(string text) {
		// Get the background the right size
		textMeshPro.SetText(text);
		textMeshPro.ForceMeshUpdate();
		Vector2 textSize = textMeshPro.GetRenderedValues(false);
		backgroundSprite.size = textSize + new Vector2(0.2f, 0.1f);
		backgroundSprite.transform.localPosition = new Vector3(backgroundSprite.size.x / 2f - 0.1f, 0f);
	}

}
