using System;
using System.Collections.Generic;
using TimefulDungeon.Misc;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace TimefulDungeon.AI {
    public class ContextSteering : MonoBehaviour {
        #region Configuration Variables

        [SerializeField] [Min(0)] private float distanceDropoff = 3f;
        [SerializeField] private int resolution = 12;
        [SerializeField] private float wallCheckRange = 30f;
        [SerializeField] private float wallAvoidRange = 1f;
        [SerializeField] private float panicSeconds = 3;
        [SerializeField] private LayerMask obstacleMask;
        [SerializeField] private bool debug;

        #endregion

        #region Private Variables
        
        // Cached values calculated from configuration
        private float _arcWidthRadians;
        private Vector2[] _mapVectors;

        // Lists modified by public add/remove functions, translated into maps
        private readonly HashSet<Interest> _interests = new HashSet<Interest>();

        // Scalar maps resulting from input lists
        private float[] _interestMap;
        private float[] _obstacleMap;

        // State
        private bool _panicking;
        private float _panicFrameCount;

        #endregion

        #region Public Variables

        /// <summary>
        ///     Output vector of context steering.
        /// </summary>
        public Vector2 Direction { get; private set; }

        /// <summary>
        ///     When there is nothing to move towards, move in this direction.
        /// </summary>
        [HideInInspector] public Vector2 defaultDirection = Vector2.zero;

        #endregion

        #region Public Methods

        // Input API. Helpers to modify input lists.
        public Interest AddInterestObj(Interest newInterest) {
            _interests.Add(newInterest);
            return newInterest;
        }
        
        public Interest AddInterest(Vector2 interest, bool isDanger = false) {
            return AddInterestObj(new Interest(interest, isDanger));
        }

        public Interest AddInterest(Transform interest, bool isDanger = false) {
            return AddInterestObj(new Interest(interest, isDanger));
        }

        public void RemoveInterest(Vector2 interest) {
            _interests.RemoveWhere(x => x == interest);
        }

        public void RemoveInterest(Transform interest) {
            _interests.RemoveWhere(x => x == (Vector2) interest.position);
        }

        public void ClearInterests() {
            _interests.Clear();
        }

        /// <summary>
        ///     Checks if we have any interests or dangers that are in range.
        /// </summary>
        /// <returns>True if any interests in range, false otherwise.</returns>
        public bool HasNoInterestsOrDangers() {
            return _interests.Count == 0;
        }

        #endregion

        #region Unity Methods

        private void Start() {
            _arcWidthRadians = 2 * Mathf.PI / resolution;
            _mapVectors = new Vector2[resolution];
            for (var i = 0; i < resolution; i++) _mapVectors[i] = GetVectorForMapSlot(i);
            _interestMap = new float[resolution];
            _obstacleMap = new float[resolution];
        }

        private void LateUpdate() {
            ClearMaps();
            CreateMapsFromLists();
            var cornered = AvoidWalls();
            if (cornered) {
                _panicking = true;
                _panicFrameCount = panicSeconds;
            }
            else if (_panicFrameCount > 0) {
                _panicFrameCount -= Time.deltaTime;
            }
            else {
                _panicking = false;
            }

            if (_panicking) Panic();
            Direction = Vector2.MoveTowards(Direction, CalculateSumInterest(), 0.05f);
            
            if (debug) DrawDebug();
        }

        private void DrawDebug() {
            Vector2 position = transform.position;
            Utils.DrawDebugCircle(position, 0.5f, Color.gray);
            for (var i = 0; i < resolution; i++) {
                var start = position + _mapVectors[i] / 2;
                float desirability;
                Color lineColor;
                if (_obstacleMap[i] < wallAvoidRange) {
                    desirability = _obstacleMap[i] * 0.1f;
                    lineColor = Color.cyan;
                }
                else {
                    desirability = _interestMap[i];
                    lineColor = desirability < 0 ? Color.red : Color.green;
                }

                var desireVector = _mapVectors[i] * (2 * Mathf.Abs(desirability));
                Debug.DrawLine(start, start + desireVector, lineColor);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Shortcut for a for loop for each direction.
        /// </summary>
        /// <param name="callback">A function that will be called for each direction. Expects no return.</param>
        private void ForEachInterest(Action<int, float> callback) {
            for (var i = 0; i < resolution; i++) callback(i, _interestMap[i]);
        }

        /// <summary>
        ///     Shortcut for a for loop for each direction.
        /// </summary>
        /// <param name="callback">
        ///     A function that will be called for each direction. Returned floats will be assigned to the
        ///     interest map.
        /// </param>
        private void ForEachInterest(Func<int, float, float> callback) {
            for (var i = 0; i < resolution; i++) _interestMap[i] = callback(i, _interestMap[i]);
        }

        /// <summary>
        ///     Used to reduce desirability for far-away targets.
        /// </summary>
        /// <param name="distance">Our distance to the target.</param>
        /// <param name="desirability">How much we'd like to go to that target.</param>
        /// <returns></returns>
        private float ScaleDesirability(float distance, float desirability) {
            // Logarithmic scale. Distance of zero has 100% modifier. Distance of infinity has 0% modifier.
            // TODO: Agents maintain their interest distance when this is not capped at 0.
            // var distanceModifier = Mathf.Max(1 - distance / maxInterestRange, 0);
            var distanceModifier = distanceDropoff / (distance + distanceDropoff);
            var normalizedDesirability = desirability * distanceModifier;
            return normalizedDesirability;
        }

        /// <summary>
        ///     Resets the maps to arrays of 0.
        /// </summary>
        private void ClearMaps() {
            ForEachInterest((i, interest) => {
                _interestMap[i] = 0;
                _obstacleMap[i] = wallCheckRange;
            });
        }

        /// <summary>
        ///     Converts the input lists into maps.
        /// </summary>
        private void CreateMapsFromLists() {
            var interestAddedToMap = false;
            foreach (var interest in _interests) {
                AddToMap(interest, interest.isDanger);
                interestAddedToMap = true;
            }

            if (!interestAddedToMap && !defaultDirection.IsZero())
                AddToMap(defaultDirection + transform.Position2D(), false);
        }

        /// <summary>
        ///     Takes a target and adds it to the interest map using the desirability function.
        /// </summary>
        /// <param name="target">The position of our target.</param>
        /// <param name="isDanger">Whether we'd like to move towards or away from that target.</param>
        private void AddToMap(Vector2 target, bool isDanger) {
            var getDesirability = GetDesirabilityFunc(isDanger);
            var position = transform.Position2D();
            var distance = Vector2.Distance(target, position);
            var targetVector = (target - position).normalized;
            ForEachInterest((i, interest) => {
                var desirability = getDesirability(_mapVectors[i], targetVector);
                return interest + ScaleDesirability(distance, desirability);
            });
        }

        /// <summary>
        ///     Used to control how the agent determines its interest. Override this for custom behavior.
        /// </summary>
        /// <param name="isDanger">Whether we'd like to move towards or away from our target.</param>
        /// <returns>A function that will be called to get the interest in a given direction.</returns>
        private Func<Vector2, Vector2, float> GetDesirabilityFunc(bool isDanger) {
            if (isDanger)
                return ShapingFunctions.AnywhereButThere;
            return (to, @from) => ShapingFunctions.PositiveOffset(to, @from);
        }

        /// <summary>
        ///     Each of the slots in the map represents a normal vector radiating from our center.
        ///     This calculates that vector based on the slot's index
        /// </summary>
        /// <param name="mapSlot">The index of the slot.</param>
        /// <param name="length">How long the vector should be. Defaults to 1.</param>
        /// <returns>The vector that the map slot represents.</returns>
        private Vector2 GetVectorForMapSlot(int mapSlot, float length = 1) {
            var angle = _arcWidthRadians * mapSlot;
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * length;
        }

        /// <summary>
        ///     Finds a sum interest based on the interest and danger maps.
        ///     Ignores any interests in the same direction as high danger.
        ///     Converts all remaining interest scalars to vectors and sums them.
        /// </summary>
        /// <returns>The normalized sum of the interest vectors.</returns>
        private Vector2 CalculateSumInterest() {
            // Ignore any interests with nearby walls
            ForEachInterest((i, interest) => _obstacleMap[i] < wallAvoidRange ? 0 : interest);

            var highestInterestIndex = 0;
            var highestInterest = float.MinValue;
            ForEachInterest((i, interest) => {
                if (!(interest > highestInterest)) return;
                highestInterest = interest;
                highestInterestIndex = i;
            });

            var interestVector = Vector2.zero;
            for (var i = highestInterestIndex - 2; i <= highestInterestIndex + 2; i++) {
                var index = (i % resolution + resolution) % resolution;
                interestVector += _mapVectors[index] * _interestMap[index];
            }

            return interestVector.normalized;
        }

        /// <summary>
        ///     Checks if their are walls within the wall check range, and assigns the distance to the obstacle map.
        ///     Also checks to see if the agent is cornered or surrounded, and returns a boolean.
        /// </summary>
        /// <returns>True if the agent is cornered or surrounded, false otherwise.</returns>
        private bool AvoidWalls() {
            var badDirections = 0;
            ForEachInterest((i, interest) => {
                var hit = Physics2D.Raycast(transform.position, _mapVectors[i], wallCheckRange, obstacleMask);
                var wallDistance = wallCheckRange;
                if (hit.collider) {
                    wallDistance = hit.distance;
                    _obstacleMap[i] = wallDistance;
                }

                if (wallDistance <= wallAvoidRange || interest < 0) badDirections++;
            });
            return badDirections >= resolution * 0.8f;
        }

        /// <summary>
        ///     Increases interest based on how far away walls are.
        /// </summary>
        private void Panic() {
            ForEachInterest((i, interest) => {
                var normalizedDistance = _obstacleMap[i] / wallCheckRange * 0.5f;
                return interest + normalizedDistance;
            });
        }

        #endregion
    }
}