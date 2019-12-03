using System.Collections;
using UnityEngine;

public class ConversationManager : MonoBehaviour {
	[SerializeField] private GameObject conversationPanel = null;
	private AutoType conversationTyper;
	private Conversation currentConversation;
	private Conversation nextConversation;

	public Conversation testConversation;

	#region Singleton
	public static ConversationManager instance;
	private void Awake() {
		if (instance != null) {
			Debug.LogWarning("Multiple instances of ConversationManager detected");
		}
		instance = this;
	}
	#endregion

	private void Start() {
		conversationTyper = conversationPanel.GetComponent<AutoType>();
		conversationPanel.SetActive(false);
		StartConversation(testConversation);
	}

	public void StartConversation(Conversation conversation) {
		nextConversation = conversation;
	}

	private IEnumerator PlayConversation() {
		conversationPanel.SetActive(true);
		yield return new WaitUntil(() => conversationTyper.done);
		foreach (ConversationPage section in currentConversation) {
			conversationTyper.PrintMessage(section.content);
			yield return new WaitUntil(() => conversationTyper.done);
		}
		currentConversation = null;
		conversationPanel.SetActive(false);
	}

	// Update is called once per frame
	private void Update() {
		if (nextConversation != null && currentConversation == null) {
			currentConversation = nextConversation;
			nextConversation = null;
			StartCoroutine(PlayConversation());
		}
	}
}