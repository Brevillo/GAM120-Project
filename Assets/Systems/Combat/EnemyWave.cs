using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyWave : MonoBehaviour {

    [SerializeField] private float startDelay, waveDuration;
    [SerializeField] private bool endWhenAllDead;

    private List<EntityHealth> enemies, activeEnemies;

    public UnityEvent OnWaveCompleted;

    private void Start() {

        enemies = new(GetComponentsInChildren<EntityHealth>(true));

        foreach (var enemy in enemies) {
            enemy.gameObject.SetActive(false);
            enemy.OnDeath += info => RemoveEnemy(enemy);
        }
    }

    public void Activate() {

        if (enemies.Count == 0) OnWaveCompleted.Invoke();

        StartCoroutine(WaveDelay());

        // if the wave duration is being used
        if (!endWhenAllDead)
            StartCoroutine(EndWave());

        IEnumerator WaveDelay() {
            yield return new WaitForSeconds(startDelay);
            StartWave();
        }

        IEnumerator EndWave() {
            yield return new WaitForSeconds(waveDuration);
            OnWaveCompleted.Invoke();
        }
    }

    private void StartWave() {

        activeEnemies = new(enemies);

        foreach (var enemy in enemies)
            enemy.gameObject.SetActive(true);
    }

    private void RemoveEnemy(EntityHealth enemy) {

        activeEnemies.Remove(enemy);

        if (activeEnemies.Count == 0) OnWaveCompleted.Invoke();
    }
}