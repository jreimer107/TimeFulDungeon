using System.Collections;
using NoiseTest;
using UnityEngine;
using UnityEngine.Serialization;

namespace TimefulDungeon.AI {
	/// <summary>
	///     Controls NPCs when they don't need to be doing anything.
	///     Works by using opensimplex noise to generate a turning amount. Every update,
	///     the desired movement direction is turned by that amount. Every so often, that amount
	///     is updated. Opensimplex is used so that going straight is more common. Also allows for
	///     a tendency to go back to spawn if too far away.
	/// </summary>
	public class WanderModule : MonoBehaviour {
        #region Configuration Fields

        [Tooltip("Distance from spawn when the entity will start to be guided back towards it.")]
        [SerializeField]
        [Min(0)]
        private float spawnAdjustStart = 2;

        [Tooltip("Distance from spawn past which the entity will head straight back to its spawn.")]
        [SerializeField]
        [Min(0)]
        private float spawnAdjustLimit = 8;

        [FormerlySerializedAs("directionChangeInverval")]
        [Tooltip("How many frames in between wander turn updates. More frames makes smoother turns.")]
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

        private int frameCount;
        private float turnAmount;
        private bool isPausing;

        private Vector2 rawWanderDirection;
        private Vector2 adjustedWanderDirection;
        private OpenSimplexNoise noise;
        private Vector2 spawn;
        private float spawnAdjustSlope;

        #endregion

        #region Public Fields

        /// <summary>
        ///     Where the NPC should wander towards.
        /// </summary>
        public Vector2 WanderDirection => isPausing ? Vector2.zero : adjustedWanderDirection;

        /// <summary>
        ///     Whether the agent is outside the furthest limit it should get while wandering.
        /// </summary>
        public bool OutsideWanderLimit => spawn.LazyDistanceCheck(transform.position, spawnAdjustLimit) > 0;

        #endregion

        #region Unity Methods

        private void Awake() {
            noise = new OpenSimplexNoise();
            rawWanderDirection = Random.insideUnitCircle;
            spawn = transform.position;
            CalculateSpawnAdjustSlope();
        }

        private void OnEnable() {
            StartCoroutine(Pause());
        }

        private void OnValidate() {
            CalculateSpawnAdjustSlope();
        }

        private void Update() {
            // Only change our wander turning every so often to smooth turns
            frameCount++;

            var position = transform.Position2D();
            if (frameCount >= directionChangeInterval) {
                frameCount = 0;
                turnAmount = (float) noise.Evaluate(position.x, position.y) * Mathf.Deg2Rad *
                             turnSpeedModifier;
            }

            // Turn our desired direction
            rawWanderDirection = rawWanderDirection.Rotate(turnAmount);

            // Check if we are too far from spawn, and weight going back if we are
            var spawnDirection = spawn - (Vector2) transform.position;
            var spawnDistance = spawnDirection.magnitude;
            adjustedWanderDirection = rawWanderDirection;
            if (spawnDistance > spawnAdjustStart) {
                var weight = Mathf.Min((spawnDistance - spawnAdjustStart) * spawnAdjustSlope, 1);
                adjustedWanderDirection = rawWanderDirection * (1 - weight) + spawnDirection.normalized * weight;
                adjustedWanderDirection.Normalize();
            }

            Debug.DrawLine(position, position + rawWanderDirection, Color.red);
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Coroutine that pauses movement at random intervals and for random lengths of time.
        /// </summary>
        /// <returns></returns>
        private IEnumerator Pause() {
            // Pause on start to try and eliminate syncing with other enemies
            isPausing = true;
            var pauseDuration = Random.Range(0, pauseDurationMean + pauseDurationDeviation);
            if (debug) Debug.Log($"Start pause for {pauseDuration} seconds");
            yield return new WaitForSeconds(pauseDuration);

            // Cycle pausing until this disabled
            while (enabled) {
                // Calculate when next pause will be, wander for that amount of time
                isPausing = false;
                var nextPauseTime = Random.Range(pauseIntervalMean - pauseIntervalDeviation,
                    pauseIntervalMean + pauseIntervalDeviation);
                if (debug) Debug.Log($"Next pause in {nextPauseTime} seconds");
                yield return new WaitForSeconds(nextPauseTime);

                // Pause for a random amount of time
                if (OutsideWanderLimit) continue;
                isPausing = true;
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
        private void CalculateSpawnAdjustSlope() {
            if (spawnAdjustLimit < spawnAdjustStart) {
                spawnAdjustLimit = spawnAdjustStart;
                Debug.LogWarning("Wander module: spawn adjust limit less than adjust start.");
            }

            if (spawnAdjustStart == spawnAdjustLimit)
                spawnAdjustSlope = 0;
            else
                spawnAdjustSlope = 0.5f / (spawnAdjustLimit - spawnAdjustStart);
        }

        #endregion
    }
}