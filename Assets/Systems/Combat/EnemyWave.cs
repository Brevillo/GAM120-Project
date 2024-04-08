using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

public class EnemyWave : MonoBehaviour, IResetable {

    [SerializeField] private float startDelay, waveDuration;
    [SerializeField] private bool endWhenAllDead;

    private List<(EntityHealth health, Vector2 position)> enemies;
    private List<EntityHealth> activeEnemies;

    public UnityEvent OnWaveCompleted;

    private void Start() {

        enemies = GetComponentsInChildren<EntityHealth>(true).Select(health => (health, (Vector2)health.transform.position)).ToList();

        foreach (var (enemy, position) in enemies) {
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

        activeEnemies = new(enemies.Select(enemy => enemy.health));

        foreach (var enemy in activeEnemies)
            enemy.gameObject.SetActive(true);
    }

    private void RemoveEnemy(EntityHealth enemy) {

        activeEnemies.Remove(enemy);

        if (activeEnemies.Count == 0) OnWaveCompleted.Invoke();
    }

    public void ResetableReset()
    {
        if (enemies == null) return;

        foreach (var (enemy, position) in enemies)
        {
            enemy.gameObject.SetActive(false);
            enemy.transform.position = position;
        }
    }
}
