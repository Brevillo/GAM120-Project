using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OliverBeebe.UnityUtilities.Runtime.Camera;
using OliverBeebe.UnityUtilities.Runtime;

public class PlayerHealth : Player.Component {

    [Header("Zeal/Zen")]
    [SerializeField] private float ZenHealRate50;
    [SerializeField] private float ZenHealRate75;
    [SerializeField] private float ZenHealRate100;
    [SerializeField] private float ZealPerHit;
    [SerializeField] private float zenPerEat;
    [SerializeField] private float healPerEat;
    [SerializeField] private float zealDamagePercent;

    [Header("Death")]
    [SerializeField] private SmartCurve deathShake;
    [SerializeField] private SmartCurve deathBarZoom, deathBarFade;
    [SerializeField] private RectTransform deathBar;
    [SerializeField] private CanvasGroup deathCanvasGroup;
    [SerializeField] private SmartCurve deathFadeOut, deathFadeIn;
    [SerializeField] private float deathGravity, deathFriction;
    [SerializeField] private SoundEffect deathSound;

    [Header("Damage")]
    [SerializeField] private float damageTimeFreezeDuration;
    [SerializeField] private CameraShakeProfile damageScreenShake, damageUIShake;
    [SerializeField] private SmartCurve hurtScreenDarken;
    [SerializeField] private RectTransform uiTransform;
    [SerializeField] private SoundEffect playerHit;

    [Header("Healing")]
    [SerializeField] private Wave healthBrightnessPulse;
    [SerializeField] private Wave healingBrightnessPulse;
    [SerializeField] private float minHealthBrightness, maxHealthBrightness, healthBrightnessAdjustSpeed;
    [SerializeField] private Image healthbarImage;
    [SerializeField] private ParticleSystem zenBodyParticles;
    [SerializeField] private SmartCurve zenBodyParticlesEmissionRate;

    private float energy;
    private float healthBrightnessVelocity;

    private CameraEffectsManager uiEffects = new();

    #region Public Fields/Methods

    public float Energy {
        get => energy;
        private set {
            energy = value;
            OnEnergyUpdated?.Invoke();
        }
    }

    public event System.Action OnEnergyUpdated;

    public float DamageMultiplier => Mathf.Lerp(1, 1 + zealDamagePercent, Mathf.InverseLerp(0.5f, 0, Energy));

    public bool CanEatMore => Energy < 1.0f;

    public float DebugSetEnergy(float energy) => Energy = energy;

    #endregion

    private void Awake() {
        Health.OnTakeDamage += OnDamage;
        Health.OnDeath += OnDeath;
    }

    private void Start() {

        Energy = 0.5f;

        CameraEffects.BlackFade(deathFadeIn);
    }

    private void Update() {

        float healAmount = Energy switch {
                  1 => ZenHealRate100,
            > 0.75f => ZenHealRate75,
            >  0.5f => ZenHealRate50,
                  _ => 0,
        };

        bool healing = healAmount != 0 && Health.HealthPercent != 1;

        if (healing) Health.Heal(healAmount * Time.deltaTime);

        // UI shake

        uiTransform.localPosition = uiEffects.Update();

        // healing zen body particles

        var zenBodyParticlesEmission = zenBodyParticles.emission;
        zenBodyParticlesEmission.rateOverTime = healing ? zenBodyParticlesEmissionRate.EvaluateAt(Energy) : 0;

        // health bar brightness effects

        Color.RGBToHSV(healthbarImage.color, out float healthColorH, out float healthColorS, out float currentHealthColorValue);

        float healthColorTargetValue
            = (healing ? healingBrightnessPulse : healthBrightnessPulse).Evaluate()
            + Mathf.Lerp(minHealthBrightness, maxHealthBrightness, Health.HealthPercent);

        currentHealthColorValue = Mathf.SmoothDamp(currentHealthColorValue, healthColorTargetValue, ref healthBrightnessVelocity, healthBrightnessAdjustSpeed);

        healthbarImage.color = Color.HSVToRGB(healthColorH, healthColorS, currentHealthColorValue);
    }   

    private void OnDamage(DamageInfo info) {

        DamageEffects(info);
    }

    private void DamageEffects(DamageInfo info) {

        TimeManager.FreezeTime(damageTimeFreezeDuration, this);
        CameraEffects.Effects.AddShake(damageScreenShake);
        CameraEffects.PostProcessingEffect<UnityEngine.Rendering.Universal.ColorAdjustments>(InvincibilityFlashing);

        uiEffects.AddShake(damageUIShake);

        Energy = Mathf.MoveTowards(Energy, 0.0f, ZealPerHit);

        Movement.TakeKnockback(info.knockbackPercent);
    }

    private void OnDeath(DamageInfo info) {

        Player.Freeze(movement: true, abilities: true, health: true);
        DamageEffects(info);

        StartCoroutine(DeathShake());
        StartCoroutine(DeathFall());

        if (deathSound != null) deathSound.Play(this);

        IEnumerator DeathFall() {

            Vector2 velocity = Rigidbody.velocity;

            while (true) {

                velocity.y -= deathGravity * Time.deltaTime;
                velocity.x = Mathf.MoveTowards(velocity.x, 0, deathFriction * Time.deltaTime);

                Rigidbody.velocity = velocity;

                yield return null;
            }
        }

        IEnumerator DeathShake() {

            deathShake.Start();

            while (!deathShake.Done) {
                BodyPivot.localPosition = Random.insideUnitCircle * deathShake.Evaluate();
                yield return null;
            }

            BodyPivot.localPosition = Vector2.zero;
            BodyPivot.localEulerAngles = Vector3.forward * 180;

            deathBarZoom.Start();
            deathBarFade.Start();

            while (!deathBarZoom.Done) {

                deathCanvasGroup.alpha = deathBarFade.Evaluate();
                deathBar.localScale = Vector3.one * deathBarZoom.Evaluate();

                yield return null;
            }

            yield return CameraEffects.BlackFade(deathFadeOut);

            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }

    private IEnumerator InvincibilityFlashing(UnityEngine.Rendering.Universal.ColorAdjustments colorAdjustment) {

        colorAdjustment.postExposure.overrideState = true;

        hurtScreenDarken.Start();
        while (!hurtScreenDarken.Done) {
            colorAdjustment.postExposure.value = hurtScreenDarken.Evaluate();
            yield return null;
        }

        colorAdjustment.postExposure.value = 0;
    }

    public void EatingZenIncrease() {

        Energy = Mathf.MoveTowards(Energy, 1.0f, zenPerEat);
        Health.Heal(healPerEat);
    }
}
