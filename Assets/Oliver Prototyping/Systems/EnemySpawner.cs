using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour {

    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int aliveAtOnce;

    private List<GameObject> activeEnemies = new();

    private void Update() {

        activeEnemies.RemoveAll(enemy => enemy == null);

        for (int i = 0; i < aliveAtOnce - activeEnemies.Count; i++) {

            var newEnemy = Instantiate(enemyPrefab, transform);

            activeEnemies.Add(newEnemy);
        }
    }
}
