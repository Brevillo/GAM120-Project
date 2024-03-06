using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CombatBound : CameraBound {

    [Header("Combat")]
    [SerializeField] private BoxCollider2D trigger;

    [Space] public UnityEvent OnPlayerEnterCombatTrigger;

    private void OnTriggerEnter2D(Collider2D collision) {

        if (collision.TryGetComponent(out Player _))
            OnPlayerEnterCombatTrigger.Invoke();
    }

    #region
    #if UNITY_EDITOR

    [CustomEditor(typeof(CombatBound))]
    private class CombatBoundEditor : CameraBoundEditor {

        private CombatBound bound => target as CombatBound;

        public override void OnInspectorGUI() {

            base.OnInspectorGUI();

            if (GUILayout.Button("Snap collider to camera bounds")) {
                bound.trigger.size = bound.rect.size;
                bound.trigger.offset = Vector2.zero;
            }
        }
    }

    #endif
    #endregion
}
