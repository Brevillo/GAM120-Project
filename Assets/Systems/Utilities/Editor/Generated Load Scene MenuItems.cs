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

        [MenuItem("Load Scene/Bella Scene", priority = 12)]
        private static void LoadScene12() => LoadScene("Assets/Scenes/Testing/Bella Scene.unity");

        [MenuItem("Load Scene/Combat Prototype", priority = 13)]
        private static void LoadScene13() => LoadScene("Assets/Scenes/Testing/Combat Prototype.unity");

        [MenuItem("Load Scene/Josh_ArenaTest", priority = 14)]
        private static void LoadScene14() => LoadScene("Assets/Scenes/Testing/Josh_ArenaTest.unity");

        [MenuItem("Load Scene/Particle Maker Scene", priority = 15)]
        private static void LoadScene15() => LoadScene("Assets/Scenes/Testing/Particle Maker Scene.unity");
    }
}
