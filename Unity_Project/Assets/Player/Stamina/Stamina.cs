using UnityEngine;
using UnityEngine.Events;

namespace TimefulDungeon.Core {
    public class Stamina : MonoBehaviour {
        public float max = 100;
        public float regen = 10;

        [SerializeField] private UnityEvent onExhausted;
        private float continuousUseAmount;
        public bool Exhausted { get; private set; }
        public float Current { get; private set; }

        private void Awake() {
            Current = max;
        }

        private void Update() {
            CheckAndSetExhaustion();
            ContinuousUseAndRegen();
        }

        private void CheckAndSetExhaustion() {
            if (Current == 0) {
                Exhausted = true;
                continuousUseAmount = 0;
                onExhausted.Invoke();
            }
            else if (Exhausted && Mathf.Abs(Current - max) < float.Epsilon) {
                Exhausted = false;
            }
        }

        private void ContinuousUseAndRegen() {
            if (continuousUseAmount > 0)
                Current = Mathf.Max(0, Current - continuousUseAmount * Time.deltaTime);
            else if (Current < max)
                Current = Mathf.Min(max, Current + regen * Time.deltaTime);
        }

        public void SingleUse(float staminaConsumed) {
            if (Exhausted) return;
            Current = Mathf.Max(0, Current - staminaConsumed);
        }

        public void StartContinuousUse(float newStaminaUse) {
            if (Exhausted) return;
            continuousUseAmount += newStaminaUse;
        }

        public void StopContinuousUse(float newStaminaUse) {
            if (Exhausted) return;
            continuousUseAmount = Mathf.Max(0, continuousUseAmount - newStaminaUse);
        }
    }
}