using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OliverBeebe.UnityUtilities.Runtime;

public readonly struct DamageInfo {

    public readonly float damageAmount;
    public readonly Vector2 direction;
    public readonly Vector2 knockbackPercent;
    public readonly bool forceKill;
    public readonly Vector2 respawnPosition;

    public DamageInfo(float damageAmount, Vector2 direction, Vector2 knockbackPercent) {
        this.damageAmount       = damageAmount;
        this.direction          = direction;
        this.knockbackPercent   = knockbackPercent;
        this.forceKill          = false;
        this.respawnPosition    = Vector2.zero;
    }

    public DamageInfo(float damageAmount, Vector2 direction, Vector2 knockbackPercent, bool forceKill, Vector2 respawnPosition) {
        this.damageAmount       = damageAmount;
        this.direction          = direction;
        this.knockbackPercent   = knockbackPercent;
        this.forceKill          = forceKill;
        this.respawnPosition    = respawnPosition;
    }
}

public class EntityHealth : MonoBehaviour , IResetable {

    [SerializeField] private ReferenceValue maxHealth;
    [SerializeField] private float invincibilityDuration;
    [SerializeField] private float onEnableInvincibility;
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
    public event System.Action<DamageInfo> OnForceKill;
    public event System.Action<DamageInfo> OnDeath;
    public event System.Action OnHealthUpdated;

    #endregion

    #region Public Functions

    public void ResetInvincibilty() => invincibilityRemaining = 0;

    public void TakeDamage(DamageInfo info) {

        // take no damage if invincible or dead
        if (Invincible || Dead) return;

        Health = Mathf.MoveTowards(Health, 0, info.damageAmount);

        invincibilityRemaining = Mathf.Max(invincibilityDuration, invincibilityRemaining);

        // death
        if (Health == 0) {
            Dead = true;
            OnDeath?.Invoke(info);
        }

        else if (info.forceKill) OnForceKill?.Invoke(info);

        else OnTakeDamage?.Invoke(info);
    }

    public void FullHeal() {
        Health = maxHealth;
    }

    public void Heal(float amount) {
        Health = Mathf.MoveTowards(Health, maxHealth, amount);
    }


    public void ResetableReset() {
        FullHeal();
        Dead = false;
        invincibilityRemaining = 0;
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

    private void OnEnable() {
        invincibilityRemaining = onEnableInvincibility;
    }

    #endregion
}
