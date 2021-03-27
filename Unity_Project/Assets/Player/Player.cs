﻿using System;
using TimefulDungeon.UI;
using UnityEditor;
using UnityEngine;

namespace TimefulDungeon.Core {
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class Player : MonoBehaviour, IDamageable {
        public static Player instance;
        public delegate void OnHealthChanged();

        public int health;
        public int maxHealth;
        public Inventory Inventory { get; private set; }
        public Stamina Stamina { get; private set; }
        public HoldingPoint HoldingPoint { get; private set; }

        private MovementController movementController;
        public OnHealthChanged onHealthChangedCallback;

        private void Awake() {
            if (instance != null) Debug.LogWarning("More than one instance of Player found.");
            instance = this;
            
            movementController = GetComponent<MovementController>();
            movementController.automatedMovement = false;
            Inventory = GetComponent<Inventory>();
            Stamina = GetComponent<Stamina>();
            HoldingPoint = GetComponentInChildren<HoldingPoint>();
        }

        private void Update() {
            //Get input from player
            movementController.SetDesiredDirection(
                new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"))
            );

            if (Input.GetKeyDown(KeyCode.G)) Damage(1);
            if (Input.GetKeyDown(KeyCode.H)) Heal(1);
            if (Input.GetKeyDown(KeyCode.T)) ChatBubble.Create(transform, new Vector3(.5f, .5f), "Quack damn you!");
        }

        public void Damage(int damage) {
            health = Mathf.Max(0, health - damage);
            Popup.CreatePopup(damage.ToString(), transform.position, Color.red);
            onHealthChangedCallback?.Invoke();
            if (health == 0)
                Die();
        }

        public void Heal(int heal) {
            health = Math.Min(maxHealth, health + heal);
            onHealthChangedCallback.Invoke();
        }

        private void Die() {
#if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
#endif
            Debug.Log("Dead");
        }
    }
}