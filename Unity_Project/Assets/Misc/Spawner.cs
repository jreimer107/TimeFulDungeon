using System.Collections;
using TimefulDungeon.Core;
using UnityEngine;

namespace TimefulDungeon.Misc {
    public class Spawner : MonoBehaviour {
        private Player _player;
        public GameObject enemyPrefab;

        private void Start()
        {
            _player = Player.instance;
            StartCoroutine(IntermittentSpawn());
        }

        private IEnumerator IntermittentSpawn() {
            var delay = 2.5f;
            while (true) {
                Instantiate(enemyPrefab, (Vector2)(_player.transform.position + Random.insideUnitSphere * 20), Quaternion.identity);
                // enemy.GetComponent<AIDestinationSetter>().target = player.transform;
                yield return new WaitForSeconds(delay);
                delay *= 0.95f;
            }
        }
    }
}
