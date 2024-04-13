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

        [MenuItem("Load Scene/Color Selection", priority = 2)]
        private static void LoadScene2() => LoadScene("Assets/Scenes/Production/Color Selection.unity");

        [MenuItem("Load Scene/Combat Tutorial Scene", priority = 3)]
        private static void LoadScene3() => LoadScene("Assets/Scenes/Production/Combat Tutorial Scene.unity");

        [MenuItem("Load Scene/End Scene", priority = 4)]
        private static void LoadScene4() => LoadScene("Assets/Scenes/Production/End Scene.unity");

        [MenuItem("Load Scene/Intro", priority = 5)]
        private static void LoadScene5() => LoadScene("Assets/Scenes/Production/Intro.unity");

        [MenuItem("Load Scene/Level 1", priority = 6)]
        private static void LoadScene6() => LoadScene("Assets/Scenes/Production/Level 1.unity");

        [MenuItem("Load Scene/Level 2", priority = 7)]
        private static void LoadScene7() => LoadScene("Assets/Scenes/Production/Level 2.unity");

        [MenuItem("Load Scene/Level 3", priority = 8)]
        private static void LoadScene8() => LoadScene("Assets/Scenes/Production/Level 3.unity");

        [MenuItem("Load Scene/Level 4 (Maybe)", priority = 9)]
        private static void LoadScene9() => LoadScene("Assets/Scenes/Production/Level 4 (Maybe).unity");

        [MenuItem("Load Scene/Main Menu", priority = 10)]
        private static void LoadScene10() => LoadScene("Assets/Scenes/Production/Main Menu.unity");

        [MenuItem("Load Scene/Tutorial Scene", priority = 11)]
        private static void LoadScene11() => LoadScene("Assets/Scenes/Production/Tutorial Scene.unity");

        [MenuItem("Load Scene/Asset Maker Scene", priority = 22)]
        private static void LoadScene22() => LoadScene("Assets/Scenes/Testing/Asset Maker Scene.unity");

        [MenuItem("Load Scene/Bella Scene", priority = 23)]
        private static void LoadScene23() => LoadScene("Assets/Scenes/Testing/Bella Scene.unity");

        [MenuItem("Load Scene/Combat Prototype", priority = 24)]
        private static void LoadScene24() => LoadScene("Assets/Scenes/Testing/Combat Prototype.unity");

        [MenuItem("Load Scene/Josh_ArenaTest", priority = 25)]
        private static void LoadScene25() => LoadScene("Assets/Scenes/Testing/Josh_ArenaTest.unity");

        [MenuItem("Load Scene/Oliver Level Test", priority = 26)]
        private static void LoadScene26() => LoadScene("Assets/Scenes/Testing/Oliver Level Test.unity");

        [MenuItem("Load Scene/Particle Maker Scene", priority = 27)]
        private static void LoadScene27() => LoadScene("Assets/Scenes/Testing/Particle Maker Scene.unity");

        [MenuItem("Load Scene/Swamp Scene", priority = 28)]
        private static void LoadScene28() => LoadScene("Assets/Scenes/Testing/Swamp Scene.unity");

        [MenuItem("Load Scene/Woods Scene", priority = 29)]
        private static void LoadScene29() => LoadScene("Assets/Scenes/Testing/Woods Scene.unity");
    }
}
