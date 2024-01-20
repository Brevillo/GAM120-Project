using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityTimeScale : MonoBehaviour {

    public float timeScale = 1;

    public static implicit operator float(EntityTimeScale entity) => entity.timeScale;

    public Coroutine TimeFreeze(float duration, bool realTime = false) {

        return StartCoroutine(Routine());

        IEnumerator Routine() {

            timeScale = 0f;

            yield return realTime
                ? new WaitForSecondsRealtime(duration)
                : new WaitForSeconds(duration);

            timeScale = 1f;
        }
    }
}
