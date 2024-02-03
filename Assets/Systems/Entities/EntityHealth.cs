using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public readonly struct DamageInfo {

    public readonly float damageAmount;
    public readonly Vector2 direction;
    public readonly Vector2 knockback;

    public DamageInfo(float damageAmount, Vector2 direction, Vector2 knockback) {
        this.damageAmount = damageAmount;
        this.direction = direction;
        this.knockback = knockback;
    }
}

public class EntityHealth : MonoBehaviour {

    [SerializeField] private float maxHealth;
    [SerializeField] private float invincibilityDuration;
    [SerializeField] private EntityHealthTeam team;
    [SerializeField, Readonly] private float health;

    #region Public Fields

    public float Health {

        get => health;

        private set {
            health = value;
            OnHealthUpdated?.Invoke();
        }
    }

    public EntityHealthTeam Team => team;

    public float HealthPercent => Health / maxHealth;

    public bool Invincible => invincibilityRemaining > 0;
    public bool Dead;

    public event System.Action<DamageInfo> OnTakeDamage;
    public event System.Action<DamageInfo> OnDeath;
    public event System.Action<float> OnHeal;
    public event System.Action OnHealthUpdated;

    #endregion

    #region Public Functions

    public void ResetInvincibilty() => invincibilityRemaining = 0;

    public void TakeDamage(DamageInfo info) {

        // take no damage if invincible or dead
        if (Invincible || Dead) return;

        Health = Mathf.MoveTowards(Health, 0, info.damageAmount);

        invincibilityRemaining = invincibilityDuration;

        // death
        if (Health <= 0) {
            Dead = true;
            OnDeath?.Invoke(info);
        }

        else OnTakeDamage?.Invoke(info);
    }

    public void FullHeal() {

        float healthDiff = maxHealth - Health;

        Health = maxHealth;

        OnHeal?.Invoke(healthDiff);
    }

    public void Heal(float amount) {

        Health = Mathf.MoveTowards(Health, maxHealth, amount);

        OnHeal?.Invoke(amount);
    }

    #endregion

    #region Internals

    private float invincibilityRemaining;

    private void Start() {
        Health = maxHealth;
    }

    private void Update() {
        invincibilityRemaining -= Time.deltaTime;
    }

    #endregion
}
