using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class StaminaBarUI : MonoBehaviour {
	private Slider slider;
	private Player player;
	private EquipmentManager equipmentManager;
	private HoldingPoint holdingPoint;

	// Start is called before the first frame update
	void Start() {
		slider = GetComponent<Slider>();
		player = Player.instance;
		equipmentManager = EquipmentManager.instance;
		holdingPoint = HoldingPoint.instance;
	}

	// Update is called once per frame
	void Update() {
		if (holdingPoint.IsShielding()) {
			player.stamina -= equipmentManager.Shield.staminaUse / Time.deltaTime;
		} else if (player.stamina < player.maxStamina) {
			player.stamina = Mathf.Min(
				player.maxStamina,
				player.stamina + player.staminaRegen / Time.deltaTime
			);
		}
	}
}
