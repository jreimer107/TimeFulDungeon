using TimefulDungeon.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TimefulDungeon.UI {
    [RequireComponent(typeof(Slider))]
    public class StaminaBarUI : MonoBehaviour {
        private Stamina stamina;
        private Slider slider;

        private void Awake() {
            slider = GetComponent<Slider>();
        }

        private void Start() {
            stamina = Player.instance.Stamina;
        }

        private void Update() {
            slider.value = stamina.Current;
        }

        private void MaxStaminaChanged() {
            slider.maxValue = stamina.max;
        }
    }
}