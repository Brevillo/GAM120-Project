using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OliverBeebe.UnityUtilities.Runtime;

public class GenericStickBug : GenericEnemyBehaviour {

    [Header("Attacking")]
    [SerializeField] private float damage;
    [SerializeField] private float knockbackPercent;
    [SerializeField] private EntityHealthTeam playerTeam;

    [Header("Attack Sequence")]
    [SerializeField] private SmartCurve attackDelayShake;
    [SerializeField] private float attackDuration;
    [SerializeField] private Transform visualsPivot;
    [SerializeField] private EntityHealthCollisionTrigger attackHitTrigger;

    [Header("Activation")]
    [SerializeField] private MultiSpriteOutline outline;
    [SerializeField] private SmartCurve outlineSize;
    [SerializeField] private EntityHealthCollisionTrigger beginAttackTrigger;

    public override IWhippable.Type WhippableType => IWhippable.Type.Heavy;

    private int DirToTarget => (int)Mathf.Sign(target.position.x - Position.x);

    private Transform target;

    private enum State { Inactive, Active, BeginningAttack, Attacking }
    private State state;

    private void Start() {
        beginAttackTrigger.OnEntityStay.AddListener(BeginAttackTrigger);
        attackHitTrigger.OnEntityStay.AddListener(AttackTrigger);
    }

    private void BeginAttackTrigger(EntityHealthCollision collision) {

        if (collision.entity.Team == playerTeam && state == State.Inactive) {

            state = State.Active;

            target = collision.entity.transform;
        }

        if (state == State.Active)
            state = State.BeginningAttack;
    }

    private void AttackTrigger(EntityHealthCollision collision) {

        if (state != State.Attacking) return;

        collision.entity.TakeDamage(new(
            damage,
            Vector2.right * DirToTarget,
            Vector2.one * knockbackPercent));
    }

    public void DoneAttacking() => state = State.Attacking;

    protected override IEnumerator Behaviour() {

        state = State.Inactive;
        attackHitTrigger.enabled = false;

        // this is dumb, but for some reason OnEnable gets called first
        yield return null;
        outline.Size = 0;

        // wait to be activated
        yield return new WaitUntil(() => state != State.Inactive);

        // activation

        outlineSize.Start();
        while (!outlineSize.Done) {
            outline.Size = outlineSize.Evaluate();
            yield return null;
        }

        while (true) {

            do {
                visualsPivot.localScale = new(DirToTarget, 1, 1);
                yield return null;
            }
            while (state != State.BeginningAttack);

            // attack telegraphing

            attackDelayShake.Start();
            while (!attackDelayShake.Done) {
                visualsPivot.localPosition = attackDelayShake.Evaluate() * Random.insideUnitCircle;
                yield return null;
            }
            visualsPivot.localPosition = Vector3.zero;

            // attacking

            state = State.Attacking;
            attackHitTrigger.enabled = true;

            yield return new WaitForSeconds(attackDuration);

            state = State.Active;
            attackHitTrigger.enabled = false;
        }
    }
}
