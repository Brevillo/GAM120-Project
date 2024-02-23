/* THIS CODE WAS AUTO GENERATED LOL */

using UnityEditor;
using UnityEditor.SceneManagement;

public static partial class LoadSceneMenuItemsGenerator {

    private static void LoadScene(string name) {
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        EditorSceneManager.OpenScene(name);
    }

    private static class MenuItems {

        [MenuItem("Load Scene/Main Menu", priority = 1)]
        private static void LoadScene1() => LoadScene("Assets/Scenes/Production/Main Menu.unity");

        [MenuItem("Load Scene/Tutorial Scene", priority = 2)]
        private static void LoadScene2() => LoadScene("Assets/Scenes/Production/Tutorial Scene.unity");

        [MenuItem("Load Scene/Bella Scene", priority = 13)]
        private static void LoadScene13() => LoadScene("Assets/Scenes/Testing/Bella Scene.unity");

        [MenuItem("Load Scene/Combat Prototype", priority = 14)]
        private static void LoadScene14() => LoadScene("Assets/Scenes/Testing/Combat Prototype.unity");

        [MenuItem("Load Scene/Josh_ArenaTest", priority = 15)]
        private static void LoadScene15() => LoadScene("Assets/Scenes/Testing/Josh_ArenaTest.unity");

        [MenuItem("Load Scene/Particle Maker Scene", priority = 16)]
        private static void LoadScene16() => LoadScene("Assets/Scenes/Testing/Particle Maker Scene.unity");

        [MenuItem("Load Scene/Swamp Scene", priority = 17)]
        private static void LoadScene17() => LoadScene("Assets/Scenes/Testing/Swamp Scene.unity");

        [MenuItem("Load Scene/Woods Scene", priority = 18)]
        private static void LoadScene18() => LoadScene("Assets/Scenes/Testing/Woods Scene.unity");
    }
}
