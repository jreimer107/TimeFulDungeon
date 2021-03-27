using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TimefulDungeon.UI {
    public class ConversationManager : MonoBehaviour {
        [SerializeField] private GameObject conversationPanel;

        public Conversation testConversation;
        private Queue<Conversation> conversations;
        private AutoType conversationTyper;
        private Conversation currentConversation;

        private void Start() {
            conversations = new Queue<Conversation>();
            conversationTyper = conversationPanel.GetComponent<AutoType>();
            conversationPanel.SetActive(false);
            StartConversation(testConversation);
        }

        private void Update() {
            if (conversations.Count != 0 && !currentConversation) {
                currentConversation = conversations.Dequeue();
                StartCoroutine(PlayConversation());
            }

            if (currentConversation && Input.GetKeyDown(KeyCode.E)) conversationTyper.SkipToEnd();
        }

        public void StartConversation(Conversation conversation) {
            conversations.Enqueue(conversation);
        }

        private IEnumerator PlayConversation() {
            conversationPanel.SetActive(true);
            yield return new WaitUntil(() => conversationTyper.Done);
            foreach (var section in currentConversation) {
                conversationTyper.PrintMessage(section.content);
                yield return new WaitUntil(() => conversationTyper.Done);
                yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.E));
            }

            currentConversation = null;
            conversationPanel.SetActive(false);
        }

        #region Singleton

        public static ConversationManager instance;

        private void Awake() {
            if (instance != null) Debug.LogWarning("Multiple instances of ConversationManager detected");
            instance = this;
        }

        #endregion
    }
}