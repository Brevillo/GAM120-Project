using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TimeManager {

    private const float fixedTimeStep = 0.02f;

    public static float timeScale {
        get => Time.timeScale;
        set {
            Time.timeScale = value;
            Time.fixedDeltaTime = fixedTimeStep * value;
        }
    }

    public static Coroutine FreezeTime(float duration, MonoBehaviour host) {

        return host.StartCoroutine(Routine());

        IEnumerator Routine() {

            timeScale = 0f;

            yield return new WaitForSecondsRealtime(duration);

            timeScale = 1f;
        }
    }
}
