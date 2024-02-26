using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class AutoZLayerManager : MonoBehaviour {

    #region Editor
    #if UNITY_EDITOR

    [CustomEditor(typeof(AutoZLayerManager))]
    private class Editor : UnityEditor.Editor {

        private AutoZLayerManager manager => target as AutoZLayerManager;

        public override void OnInspectorGUI() {

            base.OnInspectorGUI();

            if (GUILayout.Button("Setup")) Setup();
        }

        private void Setup() {

            var rends = manager.GetComponentsInChildren<SpriteRenderer>();

            Undo.RecordObjects(rends, "Auto Z Layer");
            rends
                .OrderBy(rend => -rend.transform.position.z)
                .Select((rend, index) => (rend, index))
                .ToList()
                .ForEach(item => item.rend.sortingOrder = item.index);
        }
    }

    #endif
    #endregion
}
