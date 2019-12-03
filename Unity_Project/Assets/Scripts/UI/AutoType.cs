using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AutoType : MonoBehaviour {
	[SerializeField][Range(0, 1)] private float letterPause = 0.2f;
	[SerializeField] private AudioClip sound = null;
	[SerializeField] private TextMeshProUGUI textMeshProUGUI = null;
	private AudioSource audioSource;

	private string content;
	public bool done;

	private bool donePrinting;
	private Coroutine typer;

	private void Start() {
		audioSource = GetComponent<AudioSource>();
		textMeshProUGUI.text = "";
		done = true;
		content = "";
	}

	private void Update() {
		if (Input.GetKeyDown(KeyCode.E)) {
			if (donePrinting) {
				done = true;
			} else {
				StopCoroutine(typer);
				textMeshProUGUI.text = content;
				donePrinting = true;
			}
		}
	}

	public void PrintMessage(string message) {
		content = message;
		textMeshProUGUI.text = "";
		done = false;
		donePrinting = false;
		typer = StartCoroutine(TypeText());
	}

	private IEnumerator TypeText() {
		foreach (char letter in content.ToCharArray()) {
			textMeshProUGUI.text += letter;
			if (sound) {
				audioSource.PlayOneShot(sound);
			}
			yield return new WaitForSeconds(letterPause);
		}
		donePrinting = true;
	}
}