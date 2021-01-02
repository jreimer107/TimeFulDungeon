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
}
