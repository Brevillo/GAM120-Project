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

    protected virtual void Start() {
        health.OnTakeDamage += OnTakeDamage;
        health.OnDeath += OnDeath;
    }

    protected virtual void OnTakeDamage(DamageInfo info) { }

    protected virtual void OnDeath() {
        gameObject.SetActive(false);
    }

    #endregion

    #region IWhippable Implementation

    public abstract IWhippable.Type WhippableType { get; }
    public Vector2 WhippablePosition => transform.position;
    public void DisableMovement() => StopBehaviour();
    public void EnableMovement() => StartBehaviour();
    public void MoveTo(Vector2 position) => rigidbody.MovePosition(position);

    #endregion
}
