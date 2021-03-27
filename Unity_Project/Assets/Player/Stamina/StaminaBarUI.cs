using System;
using TimefulDungeon.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TimefulDungeon.UI {
    public class StaminaBarUI : MonoBehaviour {
        [SerializeField] private GameObject sliderObject;
        private Slider _slider;
        private Stamina _stamina;
        
        private void Start() {
            _stamina = Player.instance.Stamina;
            _slider = sliderObject.GetComponent<Slider>();
        }

        private void Update() {
            _slider.value = _stamina.Current;
            sliderObject.SetActive(Math.Abs(_stamina.Current - _stamina.max) > 0.1f);
        }

        private void MaxStaminaChanged() {
            _slider.maxValue = _stamina.max;
        }
    }
}