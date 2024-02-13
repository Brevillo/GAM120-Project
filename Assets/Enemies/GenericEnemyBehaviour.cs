using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GenericEnemyBehaviour : GenericEnemyComponent, IWhippable {

    #region Behaviour

    private Coroutine behaviour;

    protected abstract IEnumerator Behaviour();

    private void OnEnable() => StartBehaviour();

    private void OnDisable() => StopBehaviour();

    private void StartBehaviour() {
        if (behaviour != null) StopCoroutine(behaviour);
        if (isActiveAndEnabled) behaviour = StartCoroutine(Behaviour());
    }

    private void StopBehaviour() {
        StopCoroutine(behaviour);
    }

    #endregion

    #region IWhippable Implementation

    public abstract IWhippable.Type WhippableType { get; }

    public Vector2 WhippablePosition {
        get => Rigidbody.position;
        set => Rigidbody.MovePosition(value);
    }

    public void DisableMovement() => StopBehaviour();
    public void EnableMovement() => StartBehaviour();

    #endregion
}
