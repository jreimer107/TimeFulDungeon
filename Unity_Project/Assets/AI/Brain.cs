using TimefulDungeon.Core.Movement;
using UnityEngine;

namespace TimefulDungeon.AI {
    [RequireComponent(typeof(MovementController))]
    public abstract class Brain : MonoBehaviour {
        [SerializeField] private float maxSpeed = 10f;
        [SerializeField] private float maxAcceleration = 10f;

        public bool debug;

        private bool _wandering;

        private ContextSteering _contextSteering;
        private MovementController _movementController;
        private WanderModule _wanderModule;

        protected virtual void Awake() {
            _movementController = GetComponent<MovementController>();
            _contextSteering = GetComponent<ContextSteering>();
            _wanderModule = GetComponent<WanderModule>();
        }

        protected virtual void Start() {
            _movementController.MaxSpeed = maxSpeed;
            _movementController.MaxAcceleration = maxAcceleration;
            SetWanderingDirection();
        }

        protected virtual void Update() {
            if (_wandering) {
                SetWanderingDirection();
            }
            SetDesiredByContext();
        }

        protected virtual void OnValidate() {
            if (!_movementController) return;
            _movementController.ChangeMaxSpeedOverTime(maxSpeed);
            _movementController.MaxAcceleration = maxAcceleration;
        }

        protected void SetDesiredByContext() {
            _movementController.DesiredDirection = _contextSteering.Direction;
        }
        
        protected void SetWanderingDirection() {
            _contextSteering.defaultDirection = _wanderModule.WanderDirection;
        }

        #region Wandering
        public void WanderAround(Vector2 position) {
            if (!_wandering) {
                _wanderModule.enabled = true;
                _wandering = true;
            }
            _wanderModule.center = position;
        }

        public void StopWandering() {
            if (!_wandering) return;
            _wandering = false;
            _wanderModule.enabled = false;
        }
        
        public bool OutsideWanderLimit => _wanderModule.OutsideWanderLimit;
        #endregion

        #region Context steering
        public Interest AddInterest(Vector2 interest) => _contextSteering.AddInterest(interest);
        public Interest AddInterest(Transform interest) => _contextSteering.AddInterest(interest);
        public void RemoveInterest(Vector2 interest) => _contextSteering.RemoveInterest(interest);
        public void RemoveInterest(Transform interest) => _contextSteering.RemoveInterest(interest);
        #endregion

        #region Movement
        public void SetSpeed(float newSpeed) {
            _movementController.ChangeMaxSpeedOverTime(newSpeed);
        }

        public void RestoreSpeed() {
            _movementController.ChangeMaxSpeedOverTime(maxSpeed);
        }
        #endregion
    }
}