﻿using TimefulDungeon.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TimefulDungeon.UI {
    [RequireComponent(typeof(Slider))]
    public class HealthBarUI : MonoBehaviour {
        public Gradient gradient;
        public Image fill;
        private Player player;

        private Slider slider;

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
}