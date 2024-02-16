using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UScene = UnityEngine.SceneManagement.Scene;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = GameManagerResourceName, menuName = "Systems/Game Manager")]
public class GameManager : ScriptableObject {

    [SerializeField] private List<Manager> managers;

    private static MonoBehaviour I => instance != null ? instance : SpawnGameManager();
    private static MonoBehaviour instance;

    private const string GameManagerResourceName = "Game Manager";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static MonoBehaviour SpawnGameManager() {

        var instance = new GameObject("Game Manager Instance").AddComponent<Instance>();
        DontDestroyOnLoad(instance);

        instance.Init(Resources.Load<GameManager>(GameManagerResourceName));

        return GameManager.instance = instance;
    }

    private class Instance : MonoBehaviour {

        public event Action OnStart;
        public event Action OnUpdate;
        public event Action OnFixedUpdate;
        public event Action<UScene, UScene> OnSceneChange;
        public event Action OnOnApplicationQuit;

        public void Init(GameManager gm) {

            SceneManager.activeSceneChanged += (from, to) => OnSceneChange?.Invoke(from, to);

            foreach (var manager in gm.managers)
                manager.GameManagerInitialize(this);
        }

        private void Start              () => OnStart               ?.Invoke();
        private void Update             () => OnUpdate              ?.Invoke();
        private void FixedUpdate        () => OnFixedUpdate         ?.Invoke();
        private void OnApplicationQuit  () => OnOnApplicationQuit   ?.Invoke();
    }

    public class Manager : ScriptableObject {

        public void GameManagerInitialize(UnityEngine.Object instance) {

            var inst = instance as Instance;

            inst.OnStart                    += Start;
            inst.OnUpdate                   += Update;
            inst.OnFixedUpdate              += FixedUpdate;
            inst.OnSceneChange              += OnSceneChange;
            inst.OnOnApplicationQuit        += OnApplicationQuit;

            RuntimeInitializeOnLoad();
        }

        protected static MonoBehaviour Instance => I;

        protected virtual void RuntimeInitializeOnLoad() { }
        protected virtual void Start() { }
        protected virtual void Update() { }
        protected virtual void FixedUpdate() { }
        protected virtual void OnSceneChange(UScene from, UScene to) { }
        protected virtual void OnApplicationQuit() { }
        protected virtual void OnValidate() { }

        #region Editor
#if UNITY_EDITOR

        // displayed in editor for easy access
        [SerializeField, Readonly] private GameManager gameManager;

        private bool IsAddedToGameManager(out GameManager gameManager, out string log) {

            var gameManagerGUIDs = AssetDatabase.FindAssets("t:GameManager");

            gameManager = null;
            log = "";

            if (gameManagerGUIDs.Length == 0) {
                log = "No Game Managers Found!";
                return false;
            }

            if (gameManagerGUIDs.Length > 1) {
                log = "Multiple Game Managers Found!";
                return false;
            }

            string gameManagerGUID = gameManagerGUIDs[0],
                   gameManagerPath = AssetDatabase.GUIDToAssetPath(gameManagerGUID);
            gameManager = AssetDatabase.LoadAssetAtPath<GameManager>(gameManagerPath);

            return gameManager.managers.Contains(this);
        }

        [CustomEditor(typeof(Manager), true)]
        protected class ManagerEditor : Editor {

            public override void OnInspectorGUI() {

                var manager = target as Manager;

                bool canAdd = !manager.IsAddedToGameManager(out var gameManager, out var log);

                if ((canAdd || log != "") && GUILayout.Button("Add to Game Manager"))
                    if (canAdd) {
                        gameManager.managers.Add(manager);
                        manager.gameManager = gameManager;

                        EditorUtility.SetDirty(manager);
                        AssetDatabase.SaveAssets();
                    }
                    else Debug.LogError(log);


                base.OnInspectorGUI();
            }
        }

#endif
        #endregion
    }
}
