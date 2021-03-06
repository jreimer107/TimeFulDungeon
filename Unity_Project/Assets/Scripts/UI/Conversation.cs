using System.Collections;
using System.Collections.Generic;
using TimefulDungeon.Core;
using UnityEngine;

namespace TimefulDungeon.UI {
	/// <summary>
	/// Conversation class. Contains list of ConversationPage sections and NPC speakers.
	/// </summary>
	[CreateAssetMenu(fileName = "New Conversation", menuName = "Conversation")]
	public class Conversation : ScriptableObject, IEnumerable {
		[SerializeField] private List<Page> sections = null;
		[SerializeField] private List<NPC> speakers = null;

		/// <summary>
		/// Section in a Conversation object.
		/// Contains a content string and a speaker NPC.
		/// </summary>
		[System.Serializable] //Needs to exist so custom editor doesn't reset objects
		public class Page {
			public string content;
			public int speakerIndex;
			public NPC speaker;
			public string speakerName => speaker.name;

			/// <summary>
			/// Whether the inspector shows this page's foldout content.
			/// </summary>
			public bool shownInInspector;

			/// <summary>
			/// Creates a new conversation page given some content and a speaker.
			/// </summary>
			/// <param name="content">A content string.</param>
			/// <param name="speaker">A NPC object.</param>
			public Page(string content, int speakerIndex) {
				this.content = content;
				this.speakerIndex = speakerIndex;
				this.shownInInspector = true;
			}

			/// <summary>
			/// Creates an empty conversation page.
			/// </summary>
			public Page() {
				this.content = null;
				this.speakerIndex = -1;
				this.shownInInspector = true;
			}
		}

		public int Count => sections.Count;
		public int SpeakerCount => speakers.Count;

		public Page this[int index] {
			get => sections[index];
			set => sections[index] = value;
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

		public NPC GetSpeaker(Page section) {
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
				if (speakers[i] == null) {
					continue;
				}
				ret[i] = speakers[i].name;
			}
			return ret;
		}

		public IEnumerator<Page> GetEnumerator() {
			foreach (Page section in sections) {
				yield return section;
			}
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return (IEnumerator)GetEnumerator();
		}
	}
}
