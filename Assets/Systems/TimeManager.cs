using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TimeManager {

    private const float fixedTimeStep = 0.005f;

    public static float timeScale {
        get => Time.timeScale;
        set {
            Time.timeScale = value;
            Time.fixedDeltaTime = fixedTimeStep * value;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ResetTimeScale() => timeScale = 1f;

    private static readonly List<TimeFreeze> timeFreezes = new();

    private class TimeFreeze {
        public TimeFreeze(Coroutine coroutine, MonoBehaviour host) => (this.coroutine, this.host) = (coroutine, host);
        public Coroutine coroutine;
        public MonoBehaviour host;
    }

    /// <summary> Freezes time for the specified duration. </summary>
    /// <param name="duration"> The duration to freeze time. </param>
    /// <param name="host"> The Monobehaveiour to run the coroutine on. You can just pass in "this" if you're working in a MonoBehaviour. </param>
    /// <returns> The generated coroutine. Can be used to stop the time freeze early. </returns>
    public static Coroutine FreezeTime(float duration, MonoBehaviour host) {

        var freeze = new TimeFreeze(null, host);
        freeze.coroutine = host.StartCoroutine(Routine());
        timeFreezes.Add(freeze);

        return freeze.coroutine;

        IEnumerator Routine() {

            timeScale = 0f;

            yield return new WaitForSecondsRealtime(duration);

            timeScale = 1f;

            timeFreezes.Remove(freeze);
        }
    }

    /// <summary> Stops all active time freeze coroutines. </summary>
    public static void StopAllTimeFreezes() {

        foreach (var freeze in timeFreezes)
            freeze.host.StopCoroutine(freeze.coroutine);

        timeFreezes.Clear();
    }
}
