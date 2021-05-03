using System.Collections;
using System.Linq;
using TimefulDungeon.Core;
using TimefulDungeon.Enemies;
using UnityEngine;

namespace TimefulDungeon.Misc {
    public class Spawner : MonoBehaviour {
        private Player _player;
        public Enemy[] enemyPrefabs;
        public int maxEnemies = 100;
        private Enemy[] _enemyPool;

        private void Start()
        {
            _player = Player.instance;
            _enemyPool = new Enemy[maxEnemies];
            for (var i = 0; i < maxEnemies; i++) {
                var enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
                _enemyPool[i] = Instantiate(enemyPrefab, Vector2.zero, Quaternion.identity);
                _enemyPool[i].gameObject.SetActive(false);
            }
            StartCoroutine(IntermittentSpawn());
        }

        private IEnumerator IntermittentSpawn() {
            var delay = 2.5f;
            while (true) {
                yield return new WaitUntil(RespawnEnemy);
                // Instantiate(enemyPrefab, (Vector2)(_player.transform.position + Random.insideUnitSphere * 20), Quaternion.identity);
                // enemy.GetComponent<AIDestinationSetter>().target = player.transform;
                yield return new WaitForSeconds(delay);
                delay *= 0.95f;
            }
        }

        private Enemy GetInactiveEnemy() {
            return _enemyPool.FirstOrDefault(enemy => !enemy.gameObject.activeSelf);
        }

        private bool RespawnEnemy() {
            var respawn = GetInactiveEnemy();
            if (!respawn) return false;
            var enemyGO = respawn.gameObject;
            respawn.Damage(-10);
            enemyGO.SetActive(true);
            enemyGO.transform.position = _player.transform.position + Random.insideUnitSphere * 40;
            return true;
        }
    }
}
