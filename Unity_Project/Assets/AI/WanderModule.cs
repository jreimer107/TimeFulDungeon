using System;
using System.Collections;
using NoiseTest;
using TimefulDungeon.Misc;
using UnityEngine;
using VoraUtils;
using Random = UnityEngine.Random;

namespace TimefulDungeon.AI {
    /// <summary>
    ///     Controls NPCs when they don't need to be doing anything.
    ///     Works by using opensimplex noise to generate a turning amount. Every update,
    ///     the desired movement direction is turned by that amount. Every so often, that amount
    ///     is updated. Opensimplex is used so that going straight is more common. Also allows for
    ///     a tendency to go back to a center if too far away.
    /// </summary>
    public class WanderModule : MonoBehaviour {
        #region Configuration Fields

        [Tooltip("Distance from center when the entity will start to be guided back towards it.")]
        [SerializeField]
        [Min(0)]
        private float centerAdjustStart = 2;

        [Tooltip("Distance from center past which the entity will not be able to continue further.")]
        [SerializeField]
        [Min(0)]
        private float centerAdjustLimit = 8;

        [Tooltip("A measure of time between picking a new direction to turn in. Smaller vales mean less turning.")]
        [SerializeField]
        [Min(1)]
        private int directionChangeInterval = 20;

        [Tooltip("Adjusts how extreme the wander turns are. Less than 1 for smoother, greater than for sharper.")]
        [SerializeField]
        [Min(0)]
        private float turnSpeedModifier = 1;

        [SerializeField] private float pauseDurationMean = 5f;
        [SerializeField] private float pauseDurationDeviation = 1f;
        [SerializeField] private float pauseIntervalMean = 5f;
        [SerializeField] private float pauseIntervalDeviation = 1f;
        [SerializeField] private bool debug;

        #endregion

        #region Private Fields

        private float _turnAmount;
        private bool _isPausing;

        // The adjusted is the raw pulled towards spawn based on how far away we are.
        // It is important to keep these separate to avoid an orbiting pattern.
        private Vector2 _rawWanderDirection;
        private Vector2 _adjustedWanderDirection;
        
        private OpenSimplexNoise _noise;
        private float _centerAdjustSlope;
        public bool OutsideWanderLimit => this.FartherThan(center, centerAdjustLimit);

        #endregion

        #region Public Fields

        /// <summary>
        ///     Where the NPC should wander towards.
        /// </summary>
        public Vector2 WanderDirection => _isPausing ? Vector2.zero : _adjustedWanderDirection;

        /// <summary>
        ///     Where the NPC should wander around.
        /// </summary>
        public Vector2 center;

        #endregion

        #region Unity Methods

        private void Awake() {
            _noise = new OpenSimplexNoise();
            _rawWanderDirection = Random.insideUnitCircle;
            center = transform.position;
            CalculateCenterAdjustSlope();
        }

        private void OnEnable() {
            StartCoroutine(Pause());
            StartCoroutine(ChangeDirection());
        }

        private void OnDisable() {
            StopAllCoroutines();
        }

        private void OnValidate() {
            CalculateCenterAdjustSlope();
        }

        private void Update() {
            // Turn our desired direction
            _rawWanderDirection = _rawWanderDirection.Rotate(_turnAmount);

            KeepNearCenter();

            if (debug) {
                var position = transform.Position2D();
                Debug.DrawLine(position, position + _rawWanderDirection, Color.red);
                Utils.DrawDebugCircle(center, centerAdjustStart, Color.green);
                Utils.DrawDebugCircle(center, centerAdjustLimit, Color.red);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Check if we are too far from spawn, and weight going back if we are
        /// </summary>
        private void KeepNearCenter() {
            var centerDirection = center - (Vector2) transform.position;
            var centerDistance = centerDirection.magnitude;
            _adjustedWanderDirection = _rawWanderDirection;
            if (!(centerDistance > centerAdjustStart)) return;
            var weight = Mathf.Min((centerDistance - centerAdjustStart) * _centerAdjustSlope, 1);
            _adjustedWanderDirection = _rawWanderDirection * (1 - weight) + centerDirection.normalized * weight;
            _adjustedWanderDirection.Normalize();
        }

        /// <summary>
        ///     Coroutine that uses noise to modify how we are turning over time. This modifies a turn amount,
        ///     which is used to rotate our desired direction every update.
        /// </summary>
        private IEnumerator ChangeDirection() {
            while (enabled) {
                yield return new WaitForSeconds(directionChangeInterval * Time.fixedDeltaTime);
                var position = transform.Position2D();
                _turnAmount = (float) _noise.Evaluate(position.x, position.y) * Mathf.Deg2Rad * turnSpeedModifier;
            }
        }


        /// <summary>
        ///     Coroutine that pauses movement at random intervals and for random lengths of time.
        /// </summary>
        private IEnumerator Pause() {
            // Pause on start to try and eliminate syncing with other enemies
            _isPausing = true;
            var pauseDuration = Random.Range(0, pauseDurationMean + pauseDurationDeviation);
            if (debug) Debug.Log($"Start pause for {pauseDuration} seconds");
            yield return new WaitForSeconds(pauseDuration);

            // Cycle pausing until this disabled
            while (enabled) {
                // Calculate when next pause will be, wander for that amount of time
                _isPausing = false;
                var nextPauseTime = Random.Range(pauseIntervalMean - pauseIntervalDeviation,
                    pauseIntervalMean + pauseIntervalDeviation);
                if (debug) Debug.Log($"Next pause in {nextPauseTime} seconds");
                yield return new WaitForSeconds(nextPauseTime);

                // Pause for a random amount of time
                if (OutsideWanderLimit) continue;
                _isPausing = true;
                pauseDuration = Random.Range(pauseDurationMean - pauseDurationDeviation,
                    pauseDurationMean + pauseDurationDeviation);
                if (debug) Debug.Log($"Pausing for {pauseDuration} seconds");
                yield return new WaitForSeconds(pauseDuration);
            }
        }

        /// <summary>
        ///     Uses a linear slope. 1 at start, 0.5 at limit.
        ///     0.5 is used so that no further progress can be made, but the entity can sit still.
        /// </summary>
        private void CalculateCenterAdjustSlope() {
            if (centerAdjustLimit < centerAdjustStart) {
                centerAdjustLimit = centerAdjustStart;
                Debug.LogWarning("Wander module: center adjust limit less than adjust start.");
            }

            if (Math.Abs(centerAdjustStart - centerAdjustLimit) < 0.01)
                _centerAdjustSlope = 0;
            else
                _centerAdjustSlope = 0.5f / (centerAdjustLimit - centerAdjustStart);
        }

        #endregion
    }
}