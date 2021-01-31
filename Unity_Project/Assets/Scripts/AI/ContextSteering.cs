using UnityEngine;
using VoraUtils;
using System.Collections.Generic;

public class ContextSteering : MonoBehaviour {
    // Configuration variables
    [SerializeField] private float maxInterestRange = 30f;
    [SerializeField] private int resolution = 12;
    [SerializeField] private int mapIncrementCount = 10;

    // Cached values calculated from configuration
    private float arcWidth;
    private float incrementSize;

    // Lists modified by public add/remove functions, translated into maps
    private List<Vector2> interestPositions;
    private List<Transform> interestTransforms;
    private List<Vector2> dangerPositions;
    private List<Transform> dangerTransforms;
    
    // Scalar maps resulting from input lists
    private float[] interestMap;
    private float[] dangerMap;

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

    private void Awake() {
        arcWidth = 2 * Mathf.PI / resolution;
        incrementSize = 1 / (float) mapIncrementCount;
        Debug.Log("Increment size: " + incrementSize);
        interestMap = new float[resolution];
        dangerMap = new float[resolution];
        interestPositions = new List<Vector2>();
        interestTransforms = new List<Transform>();
        dangerPositions = new List<Vector2>();
        dangerTransforms = new List<Transform>();
    }

    private void Update() {
        ClearMaps();
        CreateMapsFromLists();
        RoundMapsToIncrements();
        this.direction = CalculateSumInterest();
    }

    private float ScaleDesirability(float distance, float desirability) {
        // Linear scale. Distance of zero has 100% modifier. Distance of max or larger has 0% modifier.
        float distanceModifier = 1 - (distance / maxInterestRange);
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
        for (int i = 0; i < resolution; i++) {
            // Find a point represented by this slot of the interest map
            Vector2 interestPoint = GetVectorForMapSlot(i, distance);

            // Get its distance from the actual target
            float interestDistance = Vector2.Distance(target, interestPoint + (Vector2) transform.position);

            // Any points that are further away than we are are not of interest
            if (interestDistance > distance) {
                continue;
            }

            // Its desirability is 1 when the distance is zero, and zero when the distance is infinity.
            float desirability = 1 / (interestDistance + 1);

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
        float angle = arcWidth * mapSlot;
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * length;
    }

    /// <summary>
    /// Converts each of the maps to uniform increments.
    /// </summary>
    private void RoundMapsToIncrements() {
        for (int i = 0; i < resolution; i++) {
            // interestMap[i] = Utils.RoundToIncrement(interestMap[i], incrementSize);
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
            if (dangerMap[i] <= lowestDanger) {
                // Sum vectors to create sum interest
                interestVector += GetVectorForMapSlot(i, interestMap[i]);
            }
        }
        return interestVector.normalized;
    }
}