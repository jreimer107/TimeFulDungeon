using System;
using TimefulDungeon.UI;
using UnityEditor;
using UnityEngine;

namespace TimefulDungeon.Core {
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class Player : MonoBehaviour, IDamageable {
        public static Player instance;
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
        private int staminaUse;

        private MovementController movementController;
        public OnHealthChanged onHealthChangedCallback;
        public OnMaxStaminaChanged onMaxStaminaChangedCallback;
        public OnStaminaEmpty onStaminaEmptyCallback;

        private void Awake() {
            if (instance != null) Debug.LogWarning("More than one instance of Player found.");
            instance = this;
            
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

            // If stamina runs out, set exhausted so they can't shield until refill
            if (stamina <= 0) {
                stamina = 0;
                staminaUse = 0;
                exhausted = true;
                onStaminaEmptyCallback?.Invoke();
            }
            
            // Adjust stamina based on shielding or regenning
            if (staminaUse > 0)
                stamina = Mathf.Max(
                    0,
                    stamina - staminaUse * Time.deltaTime
                );
            else if (stamina < maxStamina)
                stamina = Mathf.Min(
                    maxStamina,
                    stamina + staminaRegen * Time.deltaTime
                );


            if (exhausted && Math.Abs(stamina - maxStamina) < 0.01f) exhausted = false;
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

        public void ConsumeStaminaSingleUse(int staminaConsumed) {
            if (exhausted) return;
            stamina = Mathf.Max(0, stamina - staminaConsumed);
        }

        public void StartConsumeStaminaContinuous(int newStaminaUse) {
            if (exhausted) return;
            staminaUse += newStaminaUse;
        }

        public void StopConsumeStaminaContinuous(int newStaminaUse) {
            if (exhausted) return;
            staminaUse = Mathf.Max(0, staminaUse - newStaminaUse);
        }
    }
}