using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicInstance : MonoBehaviour {

    [SerializeField] private AnimationCurve fadeCurve;
    [SerializeField] private float fadeDuration;
    [SerializeField] private AudioSource battleMusicSource, passiveMusicSource;
    [SerializeField] private MusicManager manager;

    private MusicManager.SceneMusic sceneMusic;

    public void Setup(MusicManager.SceneMusic sceneMusic) {

        this.sceneMusic = sceneMusic;

        battleMusicSource.clip = sceneMusic.battleMusic;
        passiveMusicSource.clip = sceneMusic.passiveMusic;

        passiveMusicSource.Play();
        battleMusicSource.Play();

        battleMusicInfo = (battleMusicSource, sceneMusic.battleMusicVolume);
        passiveMusicInfo = (passiveMusicSource, sceneMusic.passiveMusicVolume);

        battleMusicSource.volume = 0;
        passiveMusicSource.volume = 0;

        FadeTo(passiveMusicInfo, 1);
    }

    private (AudioSource source, float volume) battleMusicInfo, passiveMusicInfo;

    public void StartCombat() => FadeBetween(passiveMusicInfo, battleMusicInfo);

    public void StopCombat() => FadeBetween(battleMusicInfo, passiveMusicInfo);

    private void FadeTo((AudioSource source, float volume) fade, float targetVolume) {

        StartCoroutine(FadeIn());
        IEnumerator FadeIn() {

            float ogVolume = fade.source.volume / fade.volume;

            for (float timer = 0; timer < fadeDuration; timer += Time.deltaTime) {

                float percent = fadeCurve.Evaluate(timer / fadeDuration);

                fade.source.volume = Mathf.Lerp(ogVolume, targetVolume, percent) * fade.volume;

                yield return null;
            }
        }
    }

    private void FadeBetween((AudioSource source, float volume) fadeOut, (AudioSource source, float volume) fadeIn) {

        StartCoroutine(Fade());
        IEnumerator Fade() {

            for (float timer = 0; timer < fadeDuration; timer += Time.deltaTime) {

                float percent = fadeCurve.Evaluate(timer / fadeDuration);

                fadeIn.source.volume = percent * fadeIn.volume;
                fadeOut.source.volume = (1 - percent) * fadeOut.volume;

                yield return null;
            }
        }
    }
}
