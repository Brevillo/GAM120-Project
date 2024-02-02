using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : Player.Component {

    [Header("Zeal/Zen")]
    [SerializeField] private Image ZealBar;
    [SerializeField] private float ZenHealRate50;
    [SerializeField] private float ZenHealRate75;
    [SerializeField] private float ZenHealRate100;
    [SerializeField] private float ZealPerHit;
    [SerializeField] private float zealDamagePercent;

    [Header("Death")]
    [SerializeField] private SmartCurve deathShake;
    [SerializeField] private SmartCurve deathBarZoom, deathBarFade;
    [SerializeField] private RectTransform deathBar;
    [SerializeField] private CanvasGroup deathCanvasGroup;
    [SerializeField] private SmartCurve deathFadeOut, deathFadeIn;
    [SerializeField] private float deathGravity, deathFriction;

    [Header("Effects")]
    [SerializeField] private CameraShakeProfile damageShake;
    [SerializeField] private float damageTimeFreezeDuration;

    private float energy;

    public float DamageMultiplier => Mathf.Lerp(1, 1 + zealDamagePercent, Mathf.InverseLerp(0.5f, 0, energy));

    private void Awake() {
        Health.OnTakeDamage += TakeDamage;
        Health.OnDeath += OnDeath;
    }

    private void Start() {

        energy = 0.5f;

        CameraEffects.BlackFade(deathFadeIn);
    }

    private void Update()
    {
        ZealBar.fillAmount = 1-energy;
        float healAmount = energy switch
        {
            1 => ZenHealRate100,
            > 0.75f => ZenHealRate75,
            > 0.5f => ZenHealRate50,
            _ => 0,
        } ;

        if (healAmount != 0)
            Health.Heal(healAmount * Time.deltaTime);
    }   

    private void TakeDamage(DamageInfo info) {

        TimeManager.FreezeTime(damageTimeFreezeDuration, this);
        CameraEffects.AddShake(damageShake);
        CameraEffects.PostProcessingEffect<UnityEngine.Rendering.Universal.ColorAdjustments>(InvincibilityFlashing);

        energy = Mathf.MoveTowards(energy, 0.0f, ZealPerHit);

        Movement.TakeKnockback(info.knockback);
    }

    private void OnDeath() {

        Player.Freeze(movement: true, abilities: true, health: true);

        StartCoroutine(DeathShake());
        StartCoroutine(DeathFall());

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

        bool flash = false;

        while (Health.Invincible) {
            flash = !flash;
            colorAdjustment.saturation.value = flash ? -100 : 0;
            yield return null;
        }

        colorAdjustment.saturation.value = 0;
    }

    public void IncreaseZen(float energyAmount) {

        energy = Mathf.MoveTowards(energy, 1.0f, energyAmount);
        print(energyAmount);
        
    }
}
