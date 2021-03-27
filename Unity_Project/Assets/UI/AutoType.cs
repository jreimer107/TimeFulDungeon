using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace TimefulDungeon.UI {
    [RequireComponent(typeof(AudioSource))]
    public class AutoType : MonoBehaviour {
        [Range(0, 0.2f)] public float letterPause = 0.05f;

        [SerializeField] private TMP_Text textObj;

        [SerializeField] private AudioClip sound;
        private AudioSource audioSource;
        private string content;
        private Action onDoneCallback;

        private Coroutine typer;
        public bool Done { get; private set; } = true;

        private void Start() {
            audioSource = GetComponent<AudioSource>();
        }

        public void PrintMessage(string text = null, Action onDone = null) {
            content = text ?? textObj.text;
            textObj.text = "";
            onDoneCallback = onDone;
            Done = false;
            typer = StartCoroutine(TypeText());
        }

        private IEnumerator TypeText() {
            foreach (var letter in content) {
                textObj.text += letter;
                if (sound) audioSource.PlayOneShot(sound);
                yield return new WaitForSeconds(letterPause);
            }

            Done = true;
            onDoneCallback?.Invoke();
        }

        public void SkipToEnd() {
            if (typer == null) return;
            StopCoroutine(typer);
            textObj.text = content;
            Done = true;
            onDoneCallback?.Invoke();
        }
    }
}