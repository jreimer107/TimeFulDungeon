using System.Collections;
using System.Collections.Generic;
using TimefulDungeon.Core;
using UnityEngine;
public class Spawner : MonoBehaviour
{
    private Player player;
    public GameObject enemyPrefab;

    // Start is called before the first frame update
    void Start()
    {
        player = Player.instance;
        StartCoroutine(ItermittentSpawn());
    }

    private IEnumerator ItermittentSpawn() {
        float delay = 2.5f;
        while (true) {
            GameObject enemy = Instantiate(enemyPrefab, (Vector2)(player.transform.position + Random.insideUnitSphere * 20), Quaternion.identity);
            // enemy.GetComponent<AIDestinationSetter>().target = player.transform;
            yield return new WaitForSeconds(delay);
            delay *= 0.95f;
        }
    }
}
