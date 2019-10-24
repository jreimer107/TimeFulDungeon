using UnityEngine;

public class PickupManager : MonoBehaviour {
	[SerializeField] GameObject pickupPrefab = null;
	private Player player;
	private Inventory inventory;

	#region Singleton
	public static PickupManager instance;
	void Awake() {
		if (instance != null)
			Debug.LogWarning("Multiple instances of PickupManager detected.");
		instance = this;
	}
	#endregion

	// Start is called before the first frame update
	void Start() {
		player = Player.instance;
		inventory = Inventory.instance;
	}

	public void DiscardItem(Item item) {
		Debug.Log("Item discarded.");
		GameObject discardedObject = Instantiate(pickupPrefab);
		discardedObject.transform.position = player.transform.position;
		discardedObject.transform.SetParent(null);
		Pickup discardedPickup = discardedObject.GetComponent<Pickup>();
		discardedPickup.SetItem(item);
		discardedPickup.Discard();
	}
}
