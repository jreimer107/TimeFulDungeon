using UnityEngine;
using VoraUtils;
using UnityEditor;
using System.Collections.Generic;
using System;

public class ContextSteering : MonoBehaviour {
    private enum InterestCalulationMode { distanceBased, dotProduct, dotProductPositive, dotProductOffset }

    // Configuration variables
    [SerializeField] private float maxInterestRange = 30f;
    [SerializeField] private int resolution = 12;
    [SerializeField] private int mapIncrementCount = 10;
    [SerializeField] private float wallCheckRange = 1f;
    [SerializeField] private InterestCalulationMode interestCalculationMode = 0;
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
    private float[] dangerMap;
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
        dangerMap = new float[resolution];
        obstacleMap = new float[resolution];
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.B)) {
            Debug.Log("Debug!");
        }
        ClearMaps();
        CreateMapsFromLists();
        AvoidWalls();
        RoundMapsToIncrements();
        if (IsCornered()) {
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
            Debug.Log("Is cornered!");
            Panic();
        }
        this.direction = (direction + 0.1f * CalculateSumInterest()).normalized;
    }

    private void OnDrawGizmos() {
        if (EditorApplication.isPlaying) {
            Vector2 position = transform.position;
            Gizmos.color = Color.black;
            Gizmos.DrawWireSphere(position, 0.5f);
            for (int i = 0; i < resolution; i++) {
                Vector2 start = position + mapVectors[i] / 2;
                float desirability;
                if (obstacleMap[i] > 0) {
                    desirability = obstacleMap[i] * 0.1f;    
                    Gizmos.color = Color.cyan;
                }
                else {
                    desirability = interestMap[i] - dangerMap[i];
                    Gizmos.color = desirability < 0 ? Color.red : Color.green;
                }
                Vector2 desireVector = 2 * mapVectors[i] * Mathf.Abs(desirability);
                Gizmos.DrawLine(start, start + desireVector);
                // if (dangerMap[i] > interestMap[i]) {
                //     Gizmos.color = Color.red;
                //     Vector2 dangerVector = 2 * mapVectors[i] * dangerMap[i];
                //     Gizmos.DrawLine(start, start + dangerVector);
                // }
                // else if (interestMap[i] > 0) {
                //     Gizmos.color = Color.green;
                //     Vector2 interestVector = 2 * mapVectors[i] * interestMap[i];
                //     Gizmos.DrawLine(start, start + interestVector);
                // }
            }
        }
    }

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
        for (int i = 0; i < resolution; i++) {
            interestMap[i] = 0;
            dangerMap[i] = 0;
            obstacleMap[i] = 0;
        }
    }

    /// <summary>
    /// Converts the input lists into maps.
    /// </summary>
    private void CreateMapsFromLists() {
        foreach (Vector2 interest in interestPositions) {
            AddInterestOrDanger(interest, interestMap);
        }
        foreach (Transform interest in interestTransforms) {
            AddInterestOrDanger(interest.position, interestMap);
        }
        foreach (Vector2 danger in dangerPositions) {
            AddInterestOrDanger(danger, dangerMap);
        }
        foreach (Transform danger in dangerTransforms) {
            AddInterestOrDanger(danger.position, dangerMap);
        }
    }

    /// <summary>
    /// Adds a specified interest or danger to the given map.
    /// Calculates a ring of points with distance equal to the target's. Find's each point's desirability,
    /// and scales them all based on distance to the target.
    /// </summary>
    /// <param name="target">The point in space we are interested in.</param>
    /// <param name="map">The map to modify.</param>
    private void AddInterestOrDanger(Vector2 target, float[] map) {
        // Create a circle of points with equal distance as the target, and cal
        float distance = Vector2.Distance(target, transform.position);
        Vector2 targetVector = (target - (Vector2) transform.position).normalized;
        for (int i = 0; i < resolution; i++) {
            float desirability = 0;
            switch (interestCalculationMode) {
                case InterestCalulationMode.distanceBased:
                    // // Find a point represented by this slot of the interest map
                    Vector2 interestPoint = mapVectors[i] * distance;

                    // // Get its distance from the actual target
                    float interestDistance = Vector2.Distance(target, interestPoint + (Vector2) transform.position);

                    // // Any points that are further away than we are are not of interest
                    if (interestDistance > distance) {
                        interestDistance *= -1;
                    }

                    // // Its desirability is 1 when the distance is zero, and zero when the distance is infinity.
                    desirability = 1 / (interestDistance + 1);
                    break;
                case InterestCalulationMode.dotProduct:
                    desirability = Vector2.Dot(mapVectors[i], targetVector);
                    break;
                case InterestCalulationMode.dotProductPositive:
                    desirability = Mathf.Max(Vector2.Dot(mapVectors[i], targetVector), 0);
                    break;
                case InterestCalulationMode.dotProductOffset:
                    desirability = (Vector2.Dot(mapVectors[i], targetVector) + 1) / 2;
                    break;
                default:
                    break;
            }

            // Further reduce the desirability based on our distance to the target
            map[i] += ScaleDesirability(distance, desirability);
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
    /// Converts each of the maps to uniform increments.
    /// </summary>
    private void RoundMapsToIncrements() {
        for (int i = 0; i < resolution; i++) {
            // interestMap[i] = Utils.RoundToIncrement(interestMap[i], incrementSize);
            if (dangerMap[i] < 0) {
                interestMap[i] -= dangerMap[i];
                dangerMap[i] = 0;
            }
            else {
                interestMap[(i + resolution / 2) % resolution] += dangerMap[i];
            }
            dangerMap[i] = Utils.RoundToIncrement(dangerMap[i], incrementSize);
        }
    }

    /// <summary>
    /// Finds a sum interest based on the interest and danger maps.
    /// Ignores any interests in the same direction as high danger.
    /// Converts all remaining interest scalars to vectors and sums them.
    /// </summary>
    /// <returns>The normalized sum of the interest vectors.</returns>
    private Vector2 CalculateSumInterest() {
        // Find lowest danger in dangermap
        float lowestDanger = float.MaxValue;
        foreach (float danger in dangerMap) {
            if (danger < lowestDanger) {
                lowestDanger = danger;
            }
        }

        // Sum the interest vectors together to find the true interest direction.
        Vector2 interestVector = Vector2.zero;
        for (int i = 0; i < resolution; i++) {
            // Ignore any interests in direction of higher danger
            if (obstacleMap[i] > 0 || dangerMap[i] > lowestDanger) {
                // Sum vectors to create sum interest
                interestMap[i] = 0;
            }
        }

        int highestInterestIndex = 0;
        float highestInterest = float.MinValue;
        for (int i = 0; i < resolution; i++) {
            float interest = interestMap[i] - dangerMap[i];
            if (interest > highestInterest) {
                highestInterest = interest;
                highestInterestIndex = i;
            }
        }

        for (int i = highestInterestIndex - 2; i <= highestInterestIndex + 2; i++) {
            int index = (i % resolution + resolution) % resolution;
            interestVector += mapVectors[index] * interestMap[index];
        }

        // for (int i = 0; i < resolution; i++) {
        //     interestVector += mapVectors[i] * (interestMap[i] - dangerMap[i]);
        // }
        return interestVector.normalized;
    }

    private void AvoidWalls() {
        for (int i = 0; i < resolution; i++) {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, mapVectors[i], wallCheckRange, wallsLayerMask);
            if (hit.collider) {
                // Debug.DrawLine(transform.position, (Vector2) transform.position + mapVectors[i] * wallCheckRange, Color.cyan);
                // AddInterestOrDanger(hit.centroid, obstacleMap);
                obstacleMap[i] = ScaleDesirability(Vector2.Distance(hit.centroid, transform.position), 1);
                // interestMap[i] = 0;
            }
        }
    }

    public bool IsCornered() {
        // Find lowest danger in dangermap
        float lowestDanger = float.MaxValue;
        foreach (float danger in dangerMap) {
            if (danger < lowestDanger) {
                lowestDanger = danger;
            }
        }

        int badDirections = 0;
        for (int i = 0; i < resolution; i++) {
            if (obstacleMap[i] > 0 || dangerMap[i] > lowestDanger) {
                badDirections++;
            }
        }
        if (badDirections >= resolution * 0.8f) {
            return true;
        }
        return false;
    }

    public void Panic() {
        // Vector2 panicVector = Vector2.zero;
        // for (int i = 0; i < resolution; i++) {
        //     // if (obstacleMap[i] == 0) {
        //     //     interestMap[i] += 3;
        //     // }
        //     panicVector -= obstacleMap[i] * mapVectors[i];
        // }

        // for (int i = 0; i < resolution; i++) {
        //     float dot = Vector2.Dot(panicVector.normalized, mapVectors[i]);
        //     interestMap[i] += 1 - Mathf.Abs(dot - 0.65f);
        // }
        for (int i = 0; i < resolution; i++) {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, mapVectors[i], 25, wallsLayerMask);
            float distance = 25;
            if (hit.collider) {
                distance = Vector2.Distance(transform.position, hit.centroid);
            }
            float normalizedDistance = distance / 25;
            interestMap[i] += normalizedDistance;
        }
    }
}