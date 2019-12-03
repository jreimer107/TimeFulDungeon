using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Conversation class. Contains list of ConversationPage sections and NPC speakers.
/// </summary>
[CreateAssetMenu(fileName = "New Conversation", menuName = "Conversation")]
public class Conversation : ScriptableObject, IList<ConversationPage> {
	private List<ConversationPage> sections;
	private List<NPC> speakers;

	public int Count => sections.Count;
	public int SpeakerCount => speakers.Count;

	public bool IsReadOnly => false;

	public ConversationPage this [int index] {
		get => sections[index];
		set => sections[index] = value;
	}

	public Conversation() {
		sections = new List<ConversationPage>();
		speakers = new List<NPC>();
	}

	public void AddSpeaker(NPC speaker) {
		speakers.Add(speaker);
	}

	public bool HasSpeaker(NPC speaker) {
		return speakers.Contains(speaker);
	}

	public NPC GetSpeaker(int index) {
		if (index < 0 || index >= SpeakerCount) {
			return null;
		}
		return speakers[index];
	}

	public NPC GetSpeaker(ConversationPage section) {
		return GetSpeaker(section.speakerIndex);
	}

	public void RemoveSpeaker(int index) {
		for (int i = 0; i < Count; i++) {
			if (sections[i].speakerIndex == index) {
				sections[i].speakerIndex = -1;
			}
		}
		speakers.RemoveAt(index);
	}

	public string[] GetSpeakerNames() {
		string[] ret = new string[speakers.Count];
		for (int i = 0; i < speakers.Count; i++) {
			ret[i] = speakers[i].name;
		}
		return ret;
	}

	public void AddEmptySection() {
		sections.Add(new ConversationPage());
	}

	public void Move(int toIndex, int fromIndex) {
		ConversationPage temp = this [fromIndex];
		RemoveAt(fromIndex);
		Insert(toIndex, temp);
	}

	public void Swap(int toIndex, int fromIndex) {
		ConversationPage temp = sections[toIndex];
		sections[toIndex] = sections[fromIndex];
		sections[fromIndex] = temp;
	}

	public IEnumerator<ConversationPage> GetEnumerator() {
		foreach (ConversationPage section in sections) {
			yield return section;
		}
	}

	IEnumerator IEnumerable.GetEnumerator() {
		return (IEnumerator) GetEnumerator();
	}

	public int IndexOf(ConversationPage item) {
		return sections.IndexOf(item);
	}

	public void Insert(int index, ConversationPage item) {
		sections.Insert(index, item);
	}

	public void RemoveAt(int index) {
		sections.RemoveAt(index);
	}

	public void Add(ConversationPage item) {
		sections.Add(item);
	}

	public void Clear() {
		sections.Clear();
		speakers.Clear();
	}

	public bool Contains(ConversationPage item) {
		return sections.Contains(item);
	}

	public void CopyTo(ConversationPage[] array, int arrayIndex) {
		sections.CopyTo(array, arrayIndex);
	}

	public bool Remove(ConversationPage item) {
		return sections.Remove(item);
	}

}

/// <summary>
/// Section in a Conversation object.
/// Contains a content string and a speaker NPC.
/// </summary>
[System.Serializable] //Needs to exist so custom editor doesn't reset objects
public class ConversationPage {
	public string content;
	public int speakerIndex;

	/// <summary>
	/// Whether the inspector shows this page's foldout content.
	/// </summary>
	public bool shownInInspector;

	/// <summary>
	/// Creates a new conversation page given some content and a speaker.
	/// </summary>
	/// <param name="content">A content string.</param>
	/// <param name="speaker">A NPC object.</param>
	public ConversationPage(string content, int speakerIndex) {
		this.content = content;
		this.speakerIndex = speakerIndex;
		this.shownInInspector = true;
	}

	/// <summary>
	/// Creates an empty conversation page.
	/// </summary>
	public ConversationPage() {
		this.content = null;
		this.speakerIndex = -1;
		this.shownInInspector = true;
	}
}