/* THIS CODE WAS AUTO GENERATED LOL */

using UnityEditor;
using UnityEditor.SceneManagement;

public static partial class LoadSceneMenuItemsGenerator {

    private static void LoadScene(string name) {
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        EditorSceneManager.OpenScene(name);
    }

    private static class MenuItems {

        [MenuItem("Load Scene/Bella Levels", priority = 1)]
        private static void LoadScene1() => LoadScene("Assets/Scenes/Production/Bella Levels.unity");

        [MenuItem("Load Scene/Combat Tutorial Scene", priority = 2)]
        private static void LoadScene2() => LoadScene("Assets/Scenes/Production/Combat Tutorial Scene.unity");

        [MenuItem("Load Scene/Level 1", priority = 3)]
        private static void LoadScene3() => LoadScene("Assets/Scenes/Production/Level 1.unity");

        [MenuItem("Load Scene/Main Menu", priority = 4)]
        private static void LoadScene4() => LoadScene("Assets/Scenes/Production/Main Menu.unity");

        [MenuItem("Load Scene/Tutorial Scene", priority = 5)]
        private static void LoadScene5() => LoadScene("Assets/Scenes/Production/Tutorial Scene.unity");

        [MenuItem("Load Scene/Bella Scene", priority = 16)]
        private static void LoadScene16() => LoadScene("Assets/Scenes/Testing/Bella Scene.unity");

        [MenuItem("Load Scene/Combat Prototype", priority = 17)]
        private static void LoadScene17() => LoadScene("Assets/Scenes/Testing/Combat Prototype.unity");

        [MenuItem("Load Scene/Josh_ArenaTest", priority = 18)]
        private static void LoadScene18() => LoadScene("Assets/Scenes/Testing/Josh_ArenaTest.unity");

        [MenuItem("Load Scene/Oliver Level Test", priority = 19)]
        private static void LoadScene19() => LoadScene("Assets/Scenes/Testing/Oliver Level Test.unity");

        [MenuItem("Load Scene/Particle Maker Scene", priority = 20)]
        private static void LoadScene20() => LoadScene("Assets/Scenes/Testing/Particle Maker Scene.unity");

        [MenuItem("Load Scene/Swamp Scene", priority = 21)]
        private static void LoadScene21() => LoadScene("Assets/Scenes/Testing/Swamp Scene.unity");

        [MenuItem("Load Scene/Woods Scene", priority = 22)]
        private static void LoadScene22() => LoadScene("Assets/Scenes/Testing/Woods Scene.unity");
    }
}
