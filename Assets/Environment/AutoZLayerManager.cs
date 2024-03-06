using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class AutoZLayerManager : MonoBehaviour {

    #region Editor
    #if UNITY_EDITOR

    private readonly string[] ValidSortingLayers = new[] {
        "Background",
        "Ground",
        "Foreground",
    };

    private SpriteRenderer[] renderers;
    private bool sortOnUpdate;

    private void GetRenderers() {
        renderers = GetComponentsInChildren<SpriteRenderer>()
            .Where(rend => ValidSortingLayers.Contains(rend.sortingLayerName))
            .ToArray();
    }

    private void Update() {
        if (renderers == null) GetRenderers();
        if (sortOnUpdate) Sort();
    }

    private void Sort() {
        renderers
            .OrderBy(rend => -rend.transform.position.z)
            .Select((rend, index) => (rend, index))
            .ToList()
            .ForEach(item => item.rend.sortingOrder = item.index);
    }


    [CustomEditor(typeof(AutoZLayerManager))]
    private class Editor : UnityEditor.Editor {

        private AutoZLayerManager manager => target as AutoZLayerManager;

        public override void OnInspectorGUI() {

            base.OnInspectorGUI();

            if (GUILayout.Button($"Toggle constant sorting: {(manager.sortOnUpdate ? "On" : "Off" )}")) manager.sortOnUpdate = !manager.sortOnUpdate;

            if (GUILayout.Button("Setup")) {
                manager.GetRenderers();
                Undo.RecordObjects(manager.renderers, "Auto Z Layer");
                manager.Sort();
            }
        }
    }

    #endif
    #endregion
}
