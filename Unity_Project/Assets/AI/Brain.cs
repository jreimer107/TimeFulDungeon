using TimefulDungeon.Core.Movement;
using UnityEngine;

namespace TimefulDungeon.AI {
    [RequireComponent(typeof(MovementController))]
    public abstract class Brain : MonoBehaviour {
        public float maxSpeed;
        public float maxAcceleration;

        protected ContextSteering contextSteering;
        private MovementController _movementController;
        private WanderModule _wanderModule;

        private bool _wandering;

        protected virtual void Awake() {
            _movementController = GetComponent<MovementController>();
            contextSteering = GetComponent<ContextSteering>();
            _wanderModule = GetComponent<WanderModule>();
        }

        protected virtual void Start() {
            _movementController.MaxSpeed = maxSpeed;
            _movementController.MaxAcceleration = maxAcceleration;
            SetWandering();
        }

        protected virtual void Update() {
            SetWandering();
            SetDesiredByContext();
        }

        protected void SetDesiredByContext() {
            _movementController.DesiredDirection = contextSteering.Direction;
        }

        protected void SetWandering() {
            var shouldWander = contextSteering.HasNoInterestsOrDangers();
            switch (shouldWander) {
                case true when !_wandering:
                    _wanderModule.enabled = true;
                    _movementController.MaxSpeed = 0.5f;
                    _wandering = true;
                    break;
                case false when _wandering:
                    _wanderModule.enabled = false;
                    _movementController.MaxSpeed = maxSpeed;
                    _wandering = false;
                    break;
            }
            contextSteering.defaultDirection = _wandering ? _wanderModule.WanderDirection : Vector2.zero;
        }
    }
}