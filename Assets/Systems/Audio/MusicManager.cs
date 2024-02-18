using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using OliverUtils;
using OliverBeebe.UnityUtilities.Runtime;

[CreateAssetMenu(menuName = "Systems/Music Manager")]
public class MusicManager : GameManager.Manager {

    [SerializeField] private List<SceneMusic> sceneMusic;
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private string masterVolumeKey, musicVolumeKey, soundVolumeKey;
    [SerializeField] private Setting masterVolume, musicVolume, soundVolume;
    [SerializeField] private MusicInstance musicInstancePrefab;

    [Header("Putting this here for no good reason")]
    [SerializeField] private Setting fullscreenSetting;

    [System.Serializable]
    public class SceneMusic {

        [SerializeField, HideInInspector] private string name;

        public Scene scene;

        public AudioClip battleMusic;
        [Range(0, 1)] public float battleMusicVolume;

        public AudioClip passiveMusic;
        [Range(0, 1)] public float passiveMusicVolume;

        public static System.Action<SceneMusic> OnValidate => sceneMusic => {
            if (sceneMusic.scene != null)
                sceneMusic.name = sceneMusic.scene.name;
        };
    }

    protected override void OnValidate() {
        sceneMusic.ForEach(SceneMusic.OnValidate);
    }

    private MusicInstance instance;

    protected override void OnSceneChange(UnityEngine.SceneManagement.Scene from, UnityEngine.SceneManagement.Scene to) {

        if (sceneMusic.TryFind(sceneMusic => sceneMusic.scene.name == to.name, out var found)) {
            instance = Instantiate(musicInstancePrefab);
            instance.Setup(found);
        }
    }

    protected override void RuntimeInitializeOnLoad() {

        void SetVolume(string name, float volume) => mixer.SetFloat(name, volume == 0 ? -80f : Mathf.Log10(volume) * 70 + 20);
        void InitMixerGroup(Setting setting, string name) {
            SetVolume(name, setting.floatValue);
            setting.onValueChanged += () => SetVolume(name, setting.floatValue);
        }

        InitMixerGroup(masterVolume, masterVolumeKey);
        InitMixerGroup(musicVolume, musicVolumeKey);
        InitMixerGroup(soundVolume, soundVolumeKey);

        fullscreenSetting.onValueChanged += () => Screen.fullScreen = fullscreenSetting.boolValue;
    }

    public void StartCombat() => instance.StartCombat();
    public void StopCombat() => instance.StopCombat();
}
