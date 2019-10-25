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

	public Pickup SpawnPickup(Item item, Vector2 position) {
		Pickup pickup = Instantiate(pickupPrefab, position, Quaternion.identity).GetComponent<Pickup>();
		pickup.SetItem(item);
		Debug.Log("Item set by manager");
		return pickup;
	}

	public void DiscardItem(Item item) {
		Debug.Log("Item discarded.");
		SpawnPickup(item, player.transform.position).Discard();
	}
}
