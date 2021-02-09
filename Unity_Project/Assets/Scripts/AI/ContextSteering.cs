using UnityEngine;
using VoraUtils;
using UnityEditor;
using System.Collections.Generic;
using System;

public class ContextSteering : MonoBehaviour {
    // Configuration variables
    [SerializeField] private float maxInterestRange = 30f;
    [SerializeField] private int resolution = 12;
    [SerializeField] private int mapIncrementCount = 10;
    [SerializeField] private float wallCheckRange = 30f;
    [SerializeField] private float wallAvoidRange = 1f;
    [SerializeField] private float panicSeconds = 3;

    // Cached values calculated from configuration
    private float arcWidthRadians;
    private float incrementSize;
    private LayerMask wallsLayerMask; 
    private Vector2[] mapVectors;

    // Lists modified by public add/remove functions, translated into maps
    private HashSet<Vector2> interestPositions = new HashSet<Vector2>();
    private HashSet<Transform> interestTransforms = new HashSet<Transform>();
    private HashSet<Vector2> dangerPositions = new HashSet<Vector2>();
    private HashSet<Transform> dangerTransforms = new HashSet<Transform>();
    
    // Scalar maps resulting from input lists
    private float[] interestMap;
    private float[] obstacleMap;

    // State
    private bool panicing = false;
    private float panicFrameCount = 0;

    // Output vector
    public Vector2 direction { get; private set; }

    // Input API. Helpers to modify input lists.
    public void AddInterest(Vector2 interest) => interestPositions.Add(interest);
    public void AddInterest(Transform interest) => interestTransforms.Add(interest);
    public void AddDanger(Vector2 danger) => dangerPositions.Add(danger);
    public void AddDanger(Transform danger) => dangerTransforms.Add(danger);
    public void RemoveInterest(Vector2 interest) => interestPositions.Remove(interest);
    public void RemoveInterest(Transform interest) => interestTransforms.Remove(interest);
    public void RemoveDanger(Vector2 danger) => dangerPositions.Remove(danger);
    public void RemoveDanger(Transform danger) => dangerTransforms.Remove(danger);

    public void ClearInterests() {
        interestPositions.Clear();
        interestTransforms.Clear();
    }

    public void ClearDangers() {
        dangerPositions.Clear();
        dangerTransforms.Clear();
    }

    private void Awake() {
        arcWidthRadians = 2 * Mathf.PI / resolution;
        incrementSize = 1 / (float) mapIncrementCount;
        wallsLayerMask = LayerMask.GetMask("Obstacle");
        mapVectors = new Vector2[resolution];
        for (int i = 0; i < resolution; i++) {
            mapVectors[i] = GetVectorForMapSlot(i);
        }
        interestMap = new float[resolution];
        obstacleMap = new float[resolution];
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.B)) {
            Debug.Log("Debug!");
        }
        ClearMaps();
        CreateMapsFromLists();
        bool cornered = AvoidWalls();
        if (cornered) {
            panicing = true;
            panicFrameCount = panicSeconds;
        }
        else if (panicFrameCount > 0) {
            panicFrameCount -= Time.deltaTime;
        }
        else {
            panicing = false;
        }
        if (panicing) {
            Panic();
        }
        direction = Vector2.MoveTowards(direction, CalculateSumInterest(), 0.05f);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos() {
        if (EditorApplication.isPlaying) {
            Vector2 position = transform.position;
            Gizmos.color = Color.black;
            Gizmos.DrawWireSphere(position, 0.5f);
            for (int i = 0; i < resolution; i++) {
                Vector2 start = position + mapVectors[i] / 2;
                float desirability;
                if (obstacleMap[i] < wallAvoidRange) {
                    desirability = obstacleMap[i] * 0.1f;    
                    Gizmos.color = Color.cyan;
                }
                else {
                    desirability = interestMap[i];
                    Gizmos.color = desirability < 0 ? Color.red : Color.green;
                }
                Vector2 desireVector = 2 * mapVectors[i] * Mathf.Abs(desirability);
                Gizmos.DrawLine(start, start + desireVector);
            }
        }
    }
#endif

    /// <summary>
    /// Shortcut for a for loop for each direction.
    /// </summary>
    /// <param name="callback">A function that will be called for each direction. Expects no return.</param>
    private void ForEachInterest(Action<int, float> callback) {
        for (int i = 0; i < resolution; i++) {
            callback(i, interestMap[i]);
        }
    }
    
    /// <summary>
    /// Shortcut for a for loop for each direction.
    /// </summary>
    /// <param name="callback">A function that will be called for each direction. Returned floats will be assigned to the interest map.</param>
    private void ForEachInterest(Func<int, float, float> callback) {
        for (int i = 0; i < resolution; i++) {
            interestMap[i] = callback(i, interestMap[i]);
        }
    }

    /// <summary>
    /// Used to reduce desirability for far-away targets.
    /// </summary>
    /// <param name="distance">Our distance to the target.</param>
    /// <param name="desirability">How much we'd like to go to that target.</param>
    /// <returns></returns>
    private float ScaleDesirability(float distance, float desirability) {
        // Linear scale. Distance of zero has 100% modifier. Distance of max or larger has 0% modifier.
        // TODO: Agents maintain their interest distance when this is not capped at 0.
        // float distanceModifier = 1 - (distance / (float) maxInterestRange);
        float distanceModifier = Mathf.Max(1 - (distance / (float) maxInterestRange), 0);
        float normalizedDesirability = desirability * distanceModifier;
        return normalizedDesirability;
    }

    /// <summary>
    /// Resets the maps to arrays of 0.
    /// </summary>
    private void ClearMaps() {
        ForEachInterest((i, interest) => {
            interestMap[i] = 0;
            obstacleMap[i] = wallCheckRange;
        });
    }

    /// <summary>
    /// Converts the input lists into maps.
    /// </summary>
    private void CreateMapsFromLists() {
        foreach (Vector2 interest in interestPositions) {
            AddToMap(interest, false);
        }
        foreach (Transform interest in interestTransforms) {
            AddToMap(interest.position, false);
        }
        foreach (Vector2 danger in dangerPositions) {
            // AddInterestOrDanger(danger, dangerMap);
            AddToMap(danger, true);
        }
        foreach (Transform danger in dangerTransforms) {
            AddToMap(danger.position, true);
        }
    }

    /// <summary>
    /// Takes a target and adds it to the interst map using the desirability function.
    /// </summary>
    /// <param name="target">The position of our target.</param>
    /// <param name="isDanger">Wether we'd like to move towards or away from that target.</param>
    private void AddToMap(Vector2 target, bool isDanger) {
        Func<Vector2, Vector2, float> getDesirability = GetDesirabilityFunc(isDanger);
        float distance = Vector2.Distance(target, transform.position);
        Vector2 targetVector = (target - (Vector2) transform.position).normalized;
        ForEachInterest((i, interest) => {
            float desirability = getDesirability(mapVectors[i], targetVector);
            return interest + ScaleDesirability(distance, desirability);
        });
    }

    /// <summary>
    /// Used to control how the agent determines its interest. Override this for custom behavior.
    /// </summary>
    /// <param name="isDanger">Wether we'd like to move towards or away from our target.</param>
    /// <returns>A function that will be called to get the interest in a given direction.</returns>
    private Func<Vector2, Vector2, float> GetDesirabilityFunc(bool isDanger) {
        if (isDanger) {
            return ShapingFunctions.AnywhereButThere;
        }
        else {
            return (Vector2 to, Vector2 from) => ShapingFunctions.PositiveOffset(to, from);
        }
    }

    /// <summary>
    /// Each of the slots in the map represents a normal vector radiating from our center.
    /// This calculates that vector based on the slot's index
    /// </summary>
    /// <param name="mapSlot">The index of the slot.</param>
    /// <param name="length">How long the vector should be. Defaults to 1.</param>
    /// <returns>The vector that the map slot represents.</returns>
    private Vector2 GetVectorForMapSlot(int mapSlot, float length = 1) {
        float angle = arcWidthRadians * mapSlot;
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * length;
    }

    /// <summary>
    /// Finds a sum interest based on the interest and danger maps.
    /// Ignores any interests in the same direction as high danger.
    /// Converts all remaining interest scalars to vectors and sums them.
    /// </summary>
    /// <returns>The normalized sum of the interest vectors.</returns>
    private Vector2 CalculateSumInterest() {
        // Ignore any interests with nearby walls
        ForEachInterest((i, interest) => {
            return obstacleMap[i] < wallAvoidRange ? 0 : interest;
        });

        int highestInterestIndex = 0;
        float highestInterest = float.MinValue;
        ForEachInterest((i, interest) => {
            if (interest > highestInterest) {
                highestInterest = interest;
                highestInterestIndex = i;
            }
        });

        Vector2 interestVector = Vector2.zero;
        for (int i = highestInterestIndex - 2; i <= highestInterestIndex + 2; i++) {
            int index = (i % resolution + resolution) % resolution;
            interestVector += mapVectors[index] * interestMap[index];
        }
        return interestVector.normalized;
    }

    /// <summary>
    /// Checks if their are walls within the wallcheck range, and assigns the distance to the obstacle map.
    /// Also checks to see if the agent is cornered or surrounded, and returns a boolean.
    /// </summary>
    /// <returns>True if the agent is cornered or surrounded, false otherwise.</returns>
    private bool AvoidWalls() {
        int badDirections = 0;
        ForEachInterest((i, interest) => {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, mapVectors[i], wallCheckRange, wallsLayerMask);
            float wallDistance = wallCheckRange;
            if (hit.collider) {
                wallDistance = hit.distance;
                obstacleMap[i] = wallDistance;
            }
            if (wallDistance <= wallAvoidRange || interest < 0) {
                badDirections++;
            }
        });
        return badDirections >= resolution * 0.8f;
    }

    /// <summary>
    /// Increases interest based on how far away walls are.
    /// </summary>
    private void Panic() {
        ForEachInterest((i, interest) => {
            float normalizedDistance = obstacleMap[i] / wallCheckRange * 0.5f;
            return interest + normalizedDistance;
        });
    }
}