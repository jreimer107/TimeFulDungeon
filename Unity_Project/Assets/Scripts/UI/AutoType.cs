using UnityEngine;
using System.Collections;
using TMPro;
 
[RequireComponent(typeof(AudioSource))]
public class AutoType : MonoBehaviour {
	[SerializeField] [Range(0, 1)] private float letterPause = 0.2f;
	[SerializeField] private AudioClip sound = null;
	[SerializeField] private TextMeshProUGUI textMeshProUGUI = null;
	private AudioSource audioSource;
 
	private string content;
	private bool done;
 
	private void Start () {
		audioSource = GetComponent<AudioSource>();
		textMeshProUGUI.text = "";
	}

	public void PrintMessage(string message) {
		content = message;
		textMeshProUGUI.text = "";
		done = false;
		StartCoroutine(TypeText());
	}
 
	private IEnumerator TypeText () {
		foreach (char letter in content.ToCharArray()) {
			textMeshProUGUI.text += letter;
			if (sound) {
                audioSource.PlayOneShot(sound);
            }	
		    yield return new WaitForSeconds(letterPause);
		}
		done = true;
	}
}