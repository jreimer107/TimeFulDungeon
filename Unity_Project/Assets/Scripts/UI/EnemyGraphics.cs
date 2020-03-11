using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class EnemyGraphics : MonoBehaviour {
    public AIPath aiPath;

    // Update is called once per frame
    void Update()
    {
        if (aiPath.desiredVelocity.x >= 0.01f) {
            transform.localScale = new Vector3(1, 1, 1);
		}
        else if (aiPath.desiredVelocity.x <= 0.01) {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }
}
