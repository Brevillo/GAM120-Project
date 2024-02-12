using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GenericEnemy : MonoBehaviour, IWhippable {

    [SerializeField] protected EntityHealth health;
    [SerializeField] protected new Rigidbody2D rigidbody;

    protected bool BehaviourActive => behaviour != null;

    protected Vector2 Velocity {
        get => rigidbody.velocity;
        set => rigidbody.velocity = value;
    }

    protected Vector2 Position => transform.position;

    #region Behaviour Management

    private Coroutine behaviour;

    private void OnEnable() {
        StartBehaviour();
    }

    private void OnDisable() {
        StopBehaviour();
    }

    protected virtual void StartBehaviour() {
        if (behaviour != null) StopCoroutine(behaviour);
        if (isActiveAndEnabled) behaviour = StartCoroutine(Behaviour());
    }

    protected virtual void StopBehaviour() {
        StopCoroutine(behaviour);
    }

    protected abstract IEnumerator Behaviour();

    #endregion

    #region Health

    private void Awake() {
        health.OnTakeDamage += OnTakeDamage;
        health.OnDeath += OnDeath;
    }

    protected virtual void OnTakeDamage(DamageInfo info) { }

    protected virtual void OnDeath(DamageInfo info) {
        gameObject.SetActive(false);
    }

    #endregion

    #region IWhippable Implementation

    public abstract IWhippable.Type WhippableType { get; }
    public Vector2 WhippablePosition {
        get => rigidbody.position;
        set => rigidbody.MovePosition(value);
    }
    public void DisableMovement() => StopBehaviour();
    public void EnableMovement() => StartBehaviour();

    #endregion
}
