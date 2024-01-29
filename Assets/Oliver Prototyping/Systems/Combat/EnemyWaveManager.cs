using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWaveManager : MonoBehaviour {

    [SerializeField] private float startDelay;

    private List<EnemyWave> waves;

    private int currentWave;

    private void Start() {

        waves = new(GetComponentsInChildren<EnemyWave>());

        foreach (var wave in waves)
            wave.OnWaveCompleted.AddListener(WaveCompleted);
    }

    private void Activate() {

        currentWave = 0;

        StartCoroutine(StartDelay());

        IEnumerator StartDelay() {
            yield return new WaitForSeconds(startDelay);
            waves[0].Activate();
        }
    }

    private void WaveCompleted() {
        currentWave++;
        waves[currentWave].Activate();
    }

    private void OnTriggerEnter2D(Collider2D collision) {

        if (collision.TryGetComponent(out Player _))
            Activate();
    }
}
