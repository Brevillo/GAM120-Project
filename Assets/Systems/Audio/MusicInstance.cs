using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicInstance : MonoBehaviour {

    [SerializeField] private AnimationCurve fadeCurve;
    [SerializeField] private float fadeDuration;
    [SerializeField] private MusicInfo battleMusicInfo, passiveMusicInfo;

    public void Setup(MusicManager.SceneMusic sceneMusic) {

        battleMusicInfo.source.clip = sceneMusic.battleMusic;
        passiveMusicInfo.source.clip = sceneMusic.passiveMusic;

        passiveMusicInfo.source.Play();
        battleMusicInfo.source.Play();

        battleMusicInfo.volume = sceneMusic.battleMusicVolume;
        passiveMusicInfo.volume = sceneMusic.passiveMusicVolume;

        battleMusicInfo.volume = 0;
        passiveMusicInfo.volume = 0;

        FadeTo(passiveMusicInfo, 1);
    }

    [System.Serializable]
    private class MusicInfo {
        public AudioSource source;
        public float volume;
    }

    public void StartCombat() => FadeFromTo(passiveMusicInfo, battleMusicInfo);

    public void StopCombat() => FadeFromTo(battleMusicInfo, passiveMusicInfo);

    private void FadeTo(MusicInfo fade, float targetVolume) {

        StartCoroutine(FadeTo());
        IEnumerator FadeTo() {

            float ogVolume = fade.source.volume / fade.volume;

            for (float timer = 0; timer < fadeDuration; timer += Time.deltaTime) {

                float percent = fadeCurve.Evaluate(timer / fadeDuration);

                fade.source.volume = Mathf.Lerp(ogVolume, targetVolume, percent) * fade.volume;

                yield return null;
            }
        }
    }

    private void FadeFromTo(MusicInfo fadeOut, MusicInfo fadeIn) {

        StartCoroutine(FadeFromTo());
        IEnumerator FadeFromTo() {

            for (float timer = 0; timer < fadeDuration; timer += Time.deltaTime) {

                float percent = fadeCurve.Evaluate(timer / fadeDuration);

                fadeIn.source.volume = percent * fadeIn.volume;
                fadeOut.source.volume = (1 - percent) * fadeOut.volume;

                yield return null;
            }
        }
    }
}
