using System;
using System.Collections;
using System.Collections.Generic;
using TimefulDungeon.Core;
using UnityEngine;

namespace TimefulDungeon.UI {
	/// <summary>
	///     Conversation class. Contains list of ConversationPage sections and NPC speakers.
	/// </summary>
	[CreateAssetMenu(fileName = "New Conversation", menuName = "Conversation")]
    public class Conversation : ScriptableObject, IEnumerable {
        [SerializeField] private List<Page> sections;
        [SerializeField] private List<Npc> speakers;

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public string[] GetSpeakerNames() {
            var ret = new string[speakers.Count];
            for (var i = 0; i < speakers.Count; i++) {
                if (!speakers[i]) continue;
                ret[i] = speakers[i].name;
            }

            return ret;
        }

        public IEnumerator<Page> GetEnumerator() {
            return ((IEnumerable<Page>) sections).GetEnumerator();
        }

        /// <summary>
        ///     Section in a Conversation object.
        ///     Contains a content string and a speaker NPC.
        /// </summary>
        [Serializable] //Needs to exist so custom editor doesn't reset objects
        public class Page {
            public string content;
            public int speakerIndex;
            public Npc speaker;

            /// <summary>
            ///     Whether the inspector shows this page's foldout content.
            /// </summary>
            public bool shownInInspector;

            /// <summary>
            ///     Creates a new conversation page given some content and a speaker.
            /// </summary>
            /// <param name="content">A content string.</param>
            /// <param name="speakerIndex">Index of what speaker from the speaker list is talking.</param>
            public Page(string content, int speakerIndex) {
                this.content = content;
                this.speakerIndex = speakerIndex;
                shownInInspector = true;
            }

            /// <summary>
            ///     Creates an empty conversation page.
            /// </summary>
            public Page() {
                content = null;
                speakerIndex = -1;
                shownInInspector = true;
            }

            public string SpeakerName => speaker.name;
        }
    }
}