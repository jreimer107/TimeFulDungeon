using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class ConversationManager : MonoBehaviour {
	[SerializeField] private GameObject conversationPanel = null;
	private AutoType conversationTyper;
	private Conversation currentConversation;
	private Queue<Conversation> conversations;

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
		conversations = new Queue<Conversation>();
		conversationTyper = conversationPanel.GetComponent<AutoType>();
		conversationPanel.SetActive(false);
		StartConversation(testConversation);
	}

	public void StartConversation(Conversation conversation) {
		conversations.Enqueue(conversation);
	}

	private IEnumerator PlayConversation() {
		conversationPanel.SetActive(true);
		yield return new WaitUntil(() => conversationTyper.done);
		foreach (Conversation.Page section in currentConversation) {
			conversationTyper.PrintMessage(section.content);
			yield return new WaitUntil(() => conversationTyper.done);
			yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.E));
		}
		currentConversation = null;
		conversationPanel.SetActive(false);
	}

	// Update is called once per frame
	private void Update() {
		if (conversations.Count != 0 && !currentConversation) {
			currentConversation = conversations.Dequeue();
			StartCoroutine(PlayConversation());
		}

		if (currentConversation && Input.GetKeyDown(KeyCode.E)) {
			conversationTyper.SkipToEnd();
		}
	}
}