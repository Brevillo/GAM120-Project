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
    [SerializeField, Readonly] private float health;

    public float Health {

        get => health;

        private set {
            health = value;
            OnHealthUpdated?.Invoke();
        }
    }

    public EntityHealthTeam Team => team;
    public float HealthPercent => health / maxHealth;

    public event System.Action<DamageInfo> OnTakeDamage;
    public event System.Action<float> OnHeal;
    public event System.Action OnHealthUpdated;

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
