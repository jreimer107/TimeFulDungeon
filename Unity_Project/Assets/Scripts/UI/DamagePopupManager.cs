using UnityEngine;

public class DamagePopupManager : MonoBehaviour {
	[SerializeField] private GameObject damagePopupPrefab = null;

	#region Singleton
	public static DamagePopupManager instance;
	private void Awake() {
		if (instance != null) {
			Debug.LogWarning("Multiple instances of DamagePopupManger detected.");
		}
		instance = this;
	}
	#endregion

	/// <summary>
	/// Creates a damage popup.
	/// </summary>
	/// <param name="text">The text to display.</param>
	/// <param name="position">The position to display the text at</param>
	/// <param name="color">The color of the text.</param>
	public void CreateDamagePopup(string text, Vector3 position, Color color) {
		DamagePopup damagePopup = Instantiate(damagePopupPrefab).GetComponent<DamagePopup>();
		damagePopup.SetUp(text, position, color);
	}

	private void Update() {
		//if (Input.GetButtonDown("Fire1")) {
		//	CreateDamagePopup("20", Player.instance.transform.position, Color.yellow);
		//}
	}
}
