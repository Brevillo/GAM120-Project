using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRoom : MonoBehaviour {

    [SerializeField] private float startDelay;
    [SerializeField] private CombatBound cameraCombatBound;
    [SerializeField] private GameObject enableDuringCombat;
    [SerializeField] private MusicManager musicManager;

    [SerializeField, Readonly] private State state;

    private enum State { Incomplete, Active, Complete }

    private EnemyWave[] waves;
    private int currentWave;

    private void Start() {

        state = State.Incomplete;

        waves = GetComponentsInChildren<EnemyWave>();

        enableDuringCombat.SetActive(false);

        cameraCombatBound.OnPlayerEnterCombatTrigger.AddListener(ActivateRoom);

        foreach (var wave in waves)
            wave.OnWaveCompleted.AddListener(WaveCompleted);
    }

    private void ActivateRoom() {

        if (state != State.Incomplete) return;

        musicManager.StartCombat();

        state = State.Active;

        currentWave = 0;

        CameraMovement.CombatLock(cameraCombatBound);
        enableDuringCombat.SetActive(true);

        StartCoroutine(StartDelay());

        IEnumerator StartDelay() {
            yield return new WaitForSeconds(startDelay);
            waves[0].Activate();
        }
    }

    private void WaveCompleted() {

        currentWave++;

        // start next wave
        if (currentWave < waves.Length)
            waves[currentWave].Activate();

        // end room
        else RoomCompleted();
    }

    private void RoomCompleted() {

        state = State.Complete;

        musicManager.StopCombat();

        CameraMovement.CombatUnlock();
        enableDuringCombat.SetActive(false);
    }
}
