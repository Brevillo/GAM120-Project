using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MultiSpriteOutline : MonoBehaviour {

    [SerializeField] private Material outlineMaterial;
    [SerializeField] private SpriteRenderer[] spriteRenderers;

    private SpriteRenderer[] outlines;

    private void GatherSpriteRenderers() {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(); 
    }

    private void Reset() {
        GatherSpriteRenderers();
    }

    private void Awake() {

        outlines = new SpriteRenderer[spriteRenderers.Length];

        for (int i = 0; i < outlines.Length; i++) {

            var copying = spriteRenderers[i];
            var copy = new GameObject($"MultiSpriteOutline ({copying.name})").AddComponent<SpriteRenderer>();

            copy.sprite = copying.sprite;
            copy.sortingOrder = short.MinValue;
            copy.material = outlineMaterial;

            copy.transform.parent = copying.transform;
            copy.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            copy.transform.localScale = Vector3.one;

            outlines[i] = copy;
        }
    }

    #region Editor
#if UNITY_EDITOR

    [CustomEditor(typeof(MultiSpriteOutline))]
    private class MultiSpriteOutlineEditor : Editor {

        public override void OnInspectorGUI() {

            base.OnInspectorGUI();

            var outline = target as MultiSpriteOutline;

            if (GUILayout.Button("Gather sprite renderers")) {
                outline.GatherSpriteRenderers();
            }
        }
    }

#endif
    #endregion
}
