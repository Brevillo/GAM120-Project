/* THIS CODE WAS AUTO GENERATED LOL */

using UnityEditor;
using UnityEditor.SceneManagement;

public static partial class LoadSceneMenuItemsGenerator {

    private static void LoadScene(string name) {
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        EditorSceneManager.OpenScene(name);
    }

    private static class MenuItems {

        [MenuItem("Load Scene/Demo", priority = 1)]
        private static void LoadScene1() => LoadScene("Assets/Handpainted_Grass_and_Ground_Textures/Demo/Demo.unity");

        [MenuItem("Load Scene/Color Selection", priority = 12)]
        private static void LoadScene12() => LoadScene("Assets/Scenes/Production/Color Selection.unity");

        [MenuItem("Load Scene/Combat Tutorial Scene", priority = 13)]
        private static void LoadScene13() => LoadScene("Assets/Scenes/Production/Combat Tutorial Scene.unity");

        [MenuItem("Load Scene/Intro", priority = 14)]
        private static void LoadScene14() => LoadScene("Assets/Scenes/Production/Intro.unity");

        [MenuItem("Load Scene/Level 1", priority = 15)]
        private static void LoadScene15() => LoadScene("Assets/Scenes/Production/Level 1.unity");

        [MenuItem("Load Scene/Level 2", priority = 16)]
        private static void LoadScene16() => LoadScene("Assets/Scenes/Production/Level 2.unity");

        [MenuItem("Load Scene/Level 3", priority = 17)]
        private static void LoadScene17() => LoadScene("Assets/Scenes/Production/Level 3.unity");

        [MenuItem("Load Scene/Level 4 (Maybe)", priority = 18)]
        private static void LoadScene18() => LoadScene("Assets/Scenes/Production/Level 4 (Maybe).unity");

        [MenuItem("Load Scene/Main Menu", priority = 19)]
        private static void LoadScene19() => LoadScene("Assets/Scenes/Production/Main Menu.unity");

        [MenuItem("Load Scene/Tutorial Scene", priority = 20)]
        private static void LoadScene20() => LoadScene("Assets/Scenes/Production/Tutorial Scene.unity");

        [MenuItem("Load Scene/Asset Maker Scene", priority = 31)]
        private static void LoadScene31() => LoadScene("Assets/Scenes/Testing/Asset Maker Scene.unity");

        [MenuItem("Load Scene/Bella Scene", priority = 32)]
        private static void LoadScene32() => LoadScene("Assets/Scenes/Testing/Bella Scene.unity");

        [MenuItem("Load Scene/Combat Prototype", priority = 33)]
        private static void LoadScene33() => LoadScene("Assets/Scenes/Testing/Combat Prototype.unity");

        [MenuItem("Load Scene/Josh_ArenaTest", priority = 34)]
        private static void LoadScene34() => LoadScene("Assets/Scenes/Testing/Josh_ArenaTest.unity");

        [MenuItem("Load Scene/Oliver Level Test", priority = 35)]
        private static void LoadScene35() => LoadScene("Assets/Scenes/Testing/Oliver Level Test.unity");

        [MenuItem("Load Scene/Particle Maker Scene", priority = 36)]
        private static void LoadScene36() => LoadScene("Assets/Scenes/Testing/Particle Maker Scene.unity");

        [MenuItem("Load Scene/Swamp Scene", priority = 37)]
        private static void LoadScene37() => LoadScene("Assets/Scenes/Testing/Swamp Scene.unity");

        [MenuItem("Load Scene/Woods Scene", priority = 38)]
        private static void LoadScene38() => LoadScene("Assets/Scenes/Testing/Woods Scene.unity");
    }
}
