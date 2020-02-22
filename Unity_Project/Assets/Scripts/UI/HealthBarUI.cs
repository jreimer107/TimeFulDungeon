using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class HealthBarUI : MonoBehaviour {

	public Gradient gradient;
	public Image fill;

	private Slider slider;
	private Player player;

	private void Start() {
		slider = GetComponent<Slider>();

		player = Player.instance;
		ChangeSlider();
		player.onHealthChangedCallback += ChangeSlider;
	}

	private void ChangeSlider() {
		slider.value = player.health;
		slider.maxValue = player.maxHealth;
		fill.color = gradient.Evaluate(slider.normalizedValue);
	}


}