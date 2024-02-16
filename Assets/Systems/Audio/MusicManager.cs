using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OliverUtils;

[CreateAssetMenu(menuName = "Systems/Music Manager")]
public class MusicManager : GameManager.Manager {

    [SerializeField] private List<SceneMusic> sceneMusic;
    [SerializeField] private MusicInstance musicInstancePrefab;

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

    public void StartCombat() => instance.StartCombat();
    public void StopCombat() => instance.StopCombat();
}
