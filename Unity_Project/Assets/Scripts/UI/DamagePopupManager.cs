using UnityEngine;

public class DamagePopupManager : MonoBehaviour {
	[SerializeField] private GameObject damagePopupPrefab = null;
	private Transform canvas;

	private Player player;

	#region Singleton
	public static DamagePopupManager instance;
	private void Awake() {
		if (instance != null) {
			Debug.LogWarning("Multiple instances of DamagePopupManger detected.");
		}
		instance = this;
	}
	#endregion

	private void Start() {
		canvas = GameObject.Find("Canvas").transform;
		player = Player.instance;
	}

	public void CreateDamagePopup(string text, Vector3 position) {
		Debug.Log(position);
		GameObject damagePopupObject = Instantiate(damagePopupPrefab, canvas);
		DamagePopup damagePopup = damagePopupObject.GetComponent<DamagePopup>();
		// damagePopupObject.transform.SetParent(canvas.transform, false);
		// Vector2 uiPosition = Camera.main.WorldToScreenPoint(position);
		// Debug.Log(uiPosition);

		damagePopup.SetUp(text, position);
		// damagePopupObject.transform.position = uiPosition;
		// Debug.Log(damagePopupObject.transform.position);
	}

	private void Update() {
		if (Input.GetButtonDown("Fire1")) {
			CreateDamagePopup("20", Camera.main.ScreenToWorldPoint(Input.mousePosition));
		}
	}
}
