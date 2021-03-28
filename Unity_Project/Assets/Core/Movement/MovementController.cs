using System.Collections.Generic;
using UnityEngine;

namespace TimefulDungeon.Core.Movement {
    [RequireComponent(typeof(Rigidbody2D))]
    public class MovementController : MonoBehaviour {
        [SerializeField] private bool drawSteering;
        [SerializeField] private bool drawVelocity;
        [SerializeField] private bool drawDesired;
        [SerializeField] private bool freeze;
        [SerializeField] private bool drawPath;

        // Steering module

        // Automatic pathfinding variables
        private Vector2 _destination;
        private List<Vector2> _path;

        // Physics
        private Rigidbody2D _rigidbody;

        public float MaxSpeed { get; set; } = 10f;
        public float MaxAcceleration { get; set; } = 10f;
        public Vector2 DesiredDirection { get; set; }

        private void Start() {
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        private void Update() {
            if (_destination != default && _path.Count != 0) GetUpdatedPath();

            // Adjust our velocity
            Move(Time.deltaTime);
        }

        /// <summary>
        ///     Sets a position to travel to. Entity will pathfind its way there.
        /// </summary>
        /// <param name="destination">Vector2 destination to travel to.</param>
        public void Travel(Vector2 destination) {
            _destination = destination;
            GetUpdatedPath();
        }

        private void Move(float deltaTime) {
            var desiredVelocity = DesiredDirection * MaxSpeed;
            var acceleration = Vector2.ClampMagnitude(desiredVelocity - _rigidbody.velocity, MaxAcceleration) /
                               (_rigidbody.mass / 2);

            if (freeze) DesiredDirection = Vector2.zero;
            if (drawSteering) Debug.DrawRay(transform.position, acceleration, Color.blue);
            if (drawVelocity) Debug.DrawRay(transform.position, _rigidbody.velocity, Color.red);
            if (drawDesired) Debug.DrawRay(transform.position, DesiredDirection, Color.green);

            _rigidbody.velocity = Vector2.ClampMagnitude(_rigidbody.velocity + acceleration * deltaTime, MaxSpeed);
        }

        private void GetUpdatedPath() {
            if (_destination == default) return;
            _path = PathfindingGrid.instance.RequestPath(transform.position, _destination);

            if (!drawPath) return;
            for (var i = 1; i < _path.Count; i++) Debug.DrawLine(_path[i - 1], _path[i], Color.green);
        }
    }
}