using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace TimefulDungeon.UI {
	[RequireComponent(typeof(AudioSource))]
	public class AutoType : MonoBehaviour {
		[Range(0, 0.2f)] public float letterPause = 0.05f;
	
		[SerializeField] private TMP_Text textObj = null;
		private string content = null;
		private Action onDoneCallback;

		[SerializeField] private AudioClip sound = null;
		private AudioSource audioSource;

		private Coroutine typer;
		public bool done { get; private set; } = true;

		private void Start() {
			audioSource = GetComponent<AudioSource>();
			// textObj.text = "";
			// done = false;
		}

		public void PrintMessage(string content = null, Action onDone = null) {
			if (content != null) {
				this.content = content;
			} else {
				this.content = textObj.text;
			}
			textObj.text = "";
			onDoneCallback = onDone;
			done = false;
			typer = StartCoroutine(TypeText());
		}

		private IEnumerator TypeText() {
			foreach (char letter in content.ToCharArray()) {
				textObj.text += letter;
				if (sound) {
					audioSource.PlayOneShot(sound);
				}
				yield return new WaitForSeconds(letterPause);
			}
			done = true;
			onDoneCallback?.Invoke();
		}

		public void SkipToEnd() {
			if (typer != null) {
				StopCoroutine(typer);
				textObj.text = content;
				done = true;
				onDoneCallback?.Invoke();
			}
		}
	}
}