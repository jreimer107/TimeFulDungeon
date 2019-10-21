using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ClickAndDrag : MonoBehaviour {
	private Item item;
	private Image icon;

	#region Singleton
	public static ClickAndDrag instance;
	void Awake() {
		if (instance != null) {
			Debug.LogWarning("More than one instance of ClickAndDrag found.");
		}
		instance = this;
	}
	#endregion

	void Start() {
		icon = GetComponent<Image>();
	}

	// Update is called once per frame
	void Update() {
		icon.transform.position = Input.mousePosition;
	}

	public void SetHeldItem(Item item) {
		this.item = item;
		icon.enabled = true;
		icon.sprite = item.sprite;
		icon.preserveAspect = true;
	}

	public void UnsetHeldItem() {
		item = null;
		icon.enabled = false;
	}
}
