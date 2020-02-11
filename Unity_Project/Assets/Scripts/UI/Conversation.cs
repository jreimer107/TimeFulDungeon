using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "New Conversation", menuName = "Conversation")]
public class Conversation : ScriptableObject {
	public string ID;
	public List<ConversationPage> sections;

	public int Count {
		get { return sections.Count; }
	}

	public Conversation() {
		ID = "";
		sections = new List<ConversationPage>();
	}

}

[System.Serializable]
public class ConversationPage {
	[TextArea(2, 5)]
	public string content;
	public NPC speaker;

	public ConversationPage(string content, NPC speaker) {
		this.content = content;
		this.speaker = speaker;
	}
}

public class NPC : MonoBehaviour {
	public Sprite portrait;
	new public string name;
}