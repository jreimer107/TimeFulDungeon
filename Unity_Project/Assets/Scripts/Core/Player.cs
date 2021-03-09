using System;
using TimefulDungeon.UI;
using UnityEditor;
using UnityEngine;

namespace TimefulDungeon.Core {
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class Player : MonoBehaviour {
        public delegate void OnHealthChanged();

        public delegate void OnMaxStaminaChanged();

        public delegate void OnStaminaEmpty();

        public int health;
        public int maxHealth;
        public Inventory Inventory { get; private set; }

        public float stamina;
        public float maxStamina;
        public float staminaRegen;
        public bool exhausted;
        private BoxCollider2D collisionCollider;

        private MovementController movementController;
        public OnHealthChanged onHealthChangedCallback;
        public OnMaxStaminaChanged onMaxStaminaChangedCallback;
        public OnStaminaEmpty onStaminaEmptyCallback;

        public bool Shielding { get; private set; }

        private void Start() {
            collisionCollider = GetComponent<BoxCollider2D>();
            movementController = GetComponent<MovementController>();
            movementController.automatedMovement = false;
            Inventory = GetComponent<Inventory>();
        }

        private void Update() {
            //Get input from player
            movementController.SetDesiredDirection(
                new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"))
            );

            if (Input.GetKeyDown(KeyCode.G)) Damage(1);
            if (Input.GetKeyDown(KeyCode.H)) Heal(1);

            if (Input.GetKeyDown(KeyCode.T)) ChatBubble.Create(transform, new Vector3(.5f, .5f), "Quack damn you!");

            // Adjust stamina based on shielding or regenning
            if (Shielding)
                stamina = Mathf.Max(
                    0,
                    stamina - Inventory.Shield.staminaUse * Time.deltaTime
                );
            else if (stamina < maxStamina)
                stamina = Mathf.Min(
                    maxStamina,
                    stamina + staminaRegen * Time.deltaTime
                );

            // If stamina runs out, set exhausted so they can't shield until refill
            if (stamina == 0) {
                onStaminaEmptyCallback?.Invoke();
                exhausted = true;
            }

            if (exhausted && Math.Abs(stamina - maxStamina) < 0.01f) exhausted = false;
        }

        private void OnCollisionEnter2D(Collision2D collision) {
            if (collision.collider.CompareTag("Enemy")) Damage(1);
        }

        public void Damage(int damage) {
            health = Math.Max(0, health - damage);
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

        public bool ToggleShielding() {
            Shielding = !Shielding;
            return Shielding;
        }

        #region Singleton

        public static Player instance;

        private void Awake() {
            if (instance != null) Debug.LogWarning("More than one instance of Player found.");
            instance = this;
        }

        #endregion
    }
}