using UnityEngine;
using NoiseTest;

/// <summary>
/// Controls NPCs when they don't need to be doing anything.
/// Works by using opensimplex noise to generate a turning amount. Every update,
/// the desired movement direction is turned by that amount. Every so often, that amount
/// is updated. Opensimplex is used so that going straight is more common. Also allows for
/// a tendency to go back to spawn if too far away.
/// </summary>
public class WanderModule : MonoBehaviour {
	/// <summary>
	/// Where the NPC should wander towards.
	/// </summary>
	public  Vector2 WanderDirection { get; private set; }

	[Tooltip("Distance from spawn when the entity will start to be guided back towards it.")]
	[SerializeField] [Min(0)] private float spawnAdjustStart = 2;
	[Tooltip("Distance from spawn past which the entity will head straight back to its spawn.")]
	[SerializeField] [Min(0)] private float spawnAdjustLimit = 8;
	[Tooltip("How many frames in between wander turn updates. More frames makes smoother turns.")]
	[SerializeField] [Min(1)] private int directionChangeInverval = 20; 

	private int frameCount = 0;
	private float turnAmount = 0;
	
	private OpenSimplexNoise noise;
	private new Rigidbody2D rigidbody;
	private Vector2 spawn;
	private float spawnAdjustSlope;

	private void Start() {
		noise = new OpenSimplexNoise();
		rigidbody = GetComponent<Rigidbody2D>();
		WanderDirection = new Vector2(Random.value, Random.value);
		spawn = transform.position;
		CalculateSpawnAdjustSlope();
	}

	private void OnValidate() {
		CalculateSpawnAdjustSlope();
	}

	private void Update() {
		// Only change our wander turning every so often to smooth turns
		frameCount++;
		if (frameCount >= directionChangeInverval) {
			frameCount = 0;
			turnAmount = (float) noise.Evaluate(transform.position.x, transform.position.y) * Mathf.Deg2Rad;
		}

		// Turn our desired direction
		Vector2 newWanderDirection = WanderDirection.Rotate(turnAmount);

		// Check if we are too far from spawn, and weight going back if we are
		Vector2 spawnDirection = spawn - (Vector2) transform.position;
		float spawnDistance = spawnDirection.magnitude;
		if (spawnDistance > spawnAdjustStart) {
			float weight = (spawnDistance - spawnAdjustStart) * spawnAdjustSlope;
			newWanderDirection = newWanderDirection * (1 - weight) + spawnDirection.normalized * weight;
			newWanderDirection.Normalize();
		}

		WanderDirection = newWanderDirection;
		Debug.DrawLine(transform.position, (Vector2) transform.position + WanderDirection, Color.red);
	}

	/// <summary>
	/// Uses a linear slope. 1 at start, 0.5 at limit.
	/// 0.5 is used so that no further progress can be made, but the entity can sit still.
	/// </summary>
	private void CalculateSpawnAdjustSlope() {
		if (spawnAdjustLimit < spawnAdjustStart) {
			spawnAdjustLimit = spawnAdjustStart;
			Debug.LogWarning("Wander module: spawn adjust limit less than adjust start.");
		}
		if (spawnAdjustStart == spawnAdjustLimit) {
			spawnAdjustSlope = 0;
		}
		else {
			spawnAdjustSlope = 0.5f / (spawnAdjustLimit - spawnAdjustStart);
		}
	}
}
