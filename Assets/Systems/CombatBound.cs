using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CombatBound : CameraBound {

    [Header("Combat")]
    [SerializeField] private Vector2 triggerEdgeOffset = new(4f, 4f);

    [Space] public UnityEvent OnPlayerEnterCombatTrigger;

    private void Awake() {

        var trigger = gameObject.AddComponent<BoxCollider2D>();

        trigger.isTrigger = true;
        trigger.size = rect.size - triggerEdgeOffset * 2;
        trigger.offset = Vector2.zero;
    }

    private void OnTriggerEnter2D(Collider2D collision) {

        if (collision.TryGetComponent(out Player _))
            OnPlayerEnterCombatTrigger.Invoke();
    }
}
