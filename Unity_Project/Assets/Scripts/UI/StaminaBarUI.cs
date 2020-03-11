using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class StaminaBarUI : MonoBehaviour {
	private Slider slider;
	private Player player;

	// Start is called before the first frame update
	private void Start() {
		slider = GetComponent<Slider>();
		player = Player.instance;
		player.onMaxStaminaChangedCallback += MaxStaminaChanged;
	}

	// Update is called once per frame
	private void Update() {
		slider.value = player.stamina;
	}

	private void MaxStaminaChanged() {
		slider.maxValue = player.maxStamina;
	}
}
