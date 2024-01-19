using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public readonly struct DamageInfo {

    public readonly float damageAmount;
    public readonly Vector2 direction;

    public DamageInfo(float damageAmount, Vector2 direction) {
        this.damageAmount = damageAmount;
        this.direction = direction;
    }
}

public class EntityHealth : MonoBehaviour {

    [SerializeField] private float maxHealth;
    [SerializeField] private EntityHealthTeam team;

    [field: SerializeField, Readonly] public float Health { get; private set; }

    public EntityHealthTeam Team => team;
    public float HealthPercent => Health / maxHealth;

    public event System.Action<DamageInfo> OnTakeDamage;
    public event System.Action<float> OnHeal;

    private void Start() {
        Health = maxHealth;
    }

    public void TakeDamage(DamageInfo info) {

        Health -= info.damageAmount;

        OnTakeDamage?.Invoke(info);
    }

    public void FullHeal() {

        float healthDiff = maxHealth - Health;

        Health = maxHealth;

        OnHeal?.Invoke(healthDiff);
    }

    public void Heal(float amount) {

        Health += amount;

        OnHeal?.Invoke(amount);
    }
}
