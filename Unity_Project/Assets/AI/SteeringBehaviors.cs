using UnityEngine;
using VoraUtils;

namespace TimefulDungeon.AI {
    public static class SteeringBehaviors {
        public static void Steer(Rigidbody2D rb, Vector2 acceleration, float maxSpeed) {
            rb.velocity = Vector2.ClampMagnitude(rb.velocity + acceleration * Time.fixedDeltaTime, maxSpeed);
        }

        public static Vector2 CalculateSteeringAcceleration(Rigidbody2D rigidbody, Vector2 desiredDirection,
            float maxSpeed, float maxAcceleration) {
            var desiredVelocity = desiredDirection * maxSpeed;
            var steeringAcceleration = Vector2.ClampMagnitude(desiredVelocity - rigidbody.velocity, maxAcceleration) /
                                       (rigidbody.mass / 2);
            return steeringAcceleration;
        }

        public static Vector2 Seek(Vector2 target, Vector2 position, float maxSpeed) {
            var desired = target - position;
            desired = desired.normalized * maxSpeed;


            return desired;
        }

        public static Vector2 Arrive(Vector2 target, Vector2 position, float maxSpeed, float approachDistance) {
            var desired = target - position;
            var distance = desired.magnitude;
            if (distance < approachDistance)
                desired = desired.normalized * Utils.Map(distance, 0, maxSpeed, 0, approachDistance);
            else
                desired = desired.normalized * maxSpeed;
            return desired;
        }

        public static Vector2 Follow(Vector2[] path, Vector2 velocity, Vector2 location, float maxSpeed,
            float approachDistance) {
            // Predict what our location will be in x updates based on current position and velocity
            var prediction = velocity * Time.fixedDeltaTime * 10 + location;
            // Debug.DrawLine(location, prediction, Color.black);
            // Debug.Log("Prediction: " + prediction);

            // Find the normal to the path from the predicted location
            var closestNormalDistance = float.MaxValue;
            var target = Vector2.zero; //path[currentWaypoint];

            LayerMask layerMask = LayerMask.GetMask("Obstacle");

            // Find the closest normal point, this should be our target
            for (var i = 0; i < path.Length - 1; i++) {
                // Get a line segment
                var segmentStart = path[i];
                var segmentEnd = path[i + 1];

                // Get the normal point to that line segment
                var normalPoint = Utils.GetNormalPoint(segmentStart, segmentEnd, prediction);
                // If the normal is not on the segment, consider the normal to be the end of the segment
                if (!Utils.PointOnSegment(segmentStart, segmentEnd, normalPoint)) normalPoint = segmentEnd;

                // Make sure we can even see the waypoint, skip points that are blocked
                // In fact, if we hit a wall, we're probably not going to target any future segments. So break
                var hit = Physics2D.Raycast(location, normalPoint - location, Vector2.Distance(normalPoint, location),
                    layerMask);
                if (hit.collider) break;

                var distance = Vector2.Distance(prediction, normalPoint);
                if (distance < closestNormalDistance) {
                    closestNormalDistance = distance;
                    // Seek a little ahead of the normal to be smart. Can be removed if this is dumb
                    target = normalPoint + (segmentEnd - segmentStart).normalized * Time.fixedDeltaTime * 10;
                }
            }

            return target;
            // return Seek(target, location, maxSpeed);
        }
    }
}