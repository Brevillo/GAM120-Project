using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GenericEnemyBehaviour : GenericEnemyComponent, IWhippable {

    [SerializeField] private bool takeKnockback;
    [SerializeField] private SmartCurve knockback;

    #region Behaviour

    private Coroutine behaviour;

    protected abstract IEnumerator Behaviour();

    protected virtual void Awake() {
        Health.OnTakeDamage += OnTakeDamage;
    }

    private void OnTakeDamage(DamageInfo info) {

        StopBehaviour();

        if (takeKnockback) {

            StartCoroutine(Knockback());
            IEnumerator Knockback() {

                knockback.Start();

                while (!knockback.Done) {
                    Rigidbody.velocity = info.knockbackPercent * knockback.Evaluate(1);
                    yield return null;
                }

                StartBehaviour();
            }
        }

        else StartBehaviour();
    }

    private void OnEnable() => StartBehaviour();

    private void OnDisable() => StopBehaviour();

    protected virtual void StartBehaviour() {
        if (behaviour != null) StopCoroutine(behaviour);
        if (isActiveAndEnabled) behaviour = StartCoroutine(Behaviour());
    }

    protected virtual void StopBehaviour() {
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
