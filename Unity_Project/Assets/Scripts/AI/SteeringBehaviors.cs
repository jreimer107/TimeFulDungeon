using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoraUtils;

public static class SteeringBehaviors
{
    public static void Steer(Rigidbody2D rb, Vector2 acceleration, float maxVelocity) {
        rb.velocity = Vector2.ClampMagnitude(rb.velocity + acceleration * Time.fixedDeltaTime, maxVelocity);
        // rb.velocity += Vector2.ClampMagnitude(rb.velocity + acceleration, maxVelocity);
    }
    
    public static Vector2 Seek(Vector2 target, Vector2 position, float maxSpeed) {
        Vector2 desired = target - position;
        desired = desired.normalized * maxSpeed;


        return desired;
    }

    public static Vector2 Arrive(Vector2 target, Vector2 position, float maxSpeed, float approachDistance) {
        Vector2 desired = target - position;
        float distance = desired.magnitude;
        if (distance < approachDistance) {
            desired = desired.normalized * Utils.Map(distance, 0, maxSpeed, 0, approachDistance);
        }
        else {
            desired = desired.normalized * maxSpeed;
        }
        return desired;
    }

    public static Vector2 Follow(Vector2[] path, Vector2 velocity, Vector2 location, float maxSpeed, float approachDistance) {
        // Predict what our location will be in x updates based on current position and velocity
        Vector2 prediction = velocity * Time.fixedDeltaTime * 10 + location;
        Debug.DrawLine(location, prediction, Color.black);
        // Debug.Log("Prediction: " + prediction);

        // Find the normal to the path from the predicted location
        float closestNormalDistance = float.MaxValue;
        Vector2 target =  Vector2.zero;//path[currentWaypoint];

        LayerMask layerMask = LayerMask.GetMask("Obstacle");

        // Find the closest normal point, this should be our target
        for (int i = 0; i < path.Length - 1; i++) {
            // Get a line segment
            Vector2 segmentStart = path[i];
            Vector2 segmentEnd = path[i+1];

            // Get the normal point to that line segment
            Vector2 normalPoint = Utils.GetNormalPoint(segmentStart, segmentEnd, prediction);
            // If the normal is not on the segment, consider the normal to be the end of the segment
            if (!Utils.PointOnSegment(segmentStart, segmentEnd, normalPoint)) {
                normalPoint = segmentEnd;
            }

            // Make sure we can even see the waypoint, skip points that are blocked
            // In fact, if we hit a wall, we're probably not going to target any future segments. So break
            RaycastHit2D hit = Physics2D.Raycast(location, normalPoint - location, Vector2.Distance(normalPoint, location), layerMask);
            if (hit.collider) {
                break;
            }

            float distance = Vector2.Distance(prediction, normalPoint);
            if (distance < closestNormalDistance) {
                closestNormalDistance = distance;
                // Seek a little ahead of the normal to be smart. Can be removed if this is dumb
                target = normalPoint + (segmentEnd - segmentStart).normalized * Time.fixedDeltaTime * 10;
            }
        }
        return target;
        // return Seek(target, location, maxSpeed);
    }

    public static Vector2 WallAvoidance(Vector2 velocity, Vector2 location) {
        Vector2 prediction = velocity * Time.fixedDeltaTime * 10 + location;
        RaycastHit2D hit = Physics2D.Raycast(location, prediction, (prediction - location).magnitude, LayerMask.GetMask("Obstacle"));
        if (hit.collider) {
            Vector2 avoidancePoint = Utils.GetNormalPoint(location, prediction, hit.centroid);
            Vector2 avoidance = avoidancePoint - hit.centroid;
            Vector2 avoidanceForce = prediction;
        }
        return Vector2.zero;
    }
}
