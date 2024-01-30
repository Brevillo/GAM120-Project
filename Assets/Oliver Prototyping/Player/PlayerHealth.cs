using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : Player.Component {

    [SerializeField] private Image ZealBar;
    [SerializeField] private float ZenHealRate50;
    [SerializeField] private float ZenHealRate75;
    [SerializeField] private float ZenHealRate100;
    [SerializeField] private float ZealPerHit;
    [Header("Effects")]
    [SerializeField] private CameraShakeProfile damageShake;
    [SerializeField] private float damageTimeFreezeDuration;
    private float energy;

    private void Awake() {
        Health.OnTakeDamage += TakeDamage;
    }

    private void Start()
    {
        energy = 0.5f;
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

        Health.Heal(healAmount * Time.deltaTime);
    }

    private void TakeDamage(DamageInfo info) {

        TimeManager.FreezeTime(damageTimeFreezeDuration, this);
        CameraEffects.AddShake(damageShake);
        CameraEffects.PostProcessingEffect<UnityEngine.Rendering.Universal.ColorAdjustments>(
            duration:   damageTimeFreezeDuration,
            unscaled:   true,
            preEffect:  color => color.saturation.value = -100,
            postEffect: color => color.saturation.value = 0);

        energy = Mathf.MoveTowards(energy, 0.0f, ZealPerHit);

        Movement.TakeKnockback(info.knockback);
    }

    public void IncreaseZen(float energyAmount) {

        energy = Mathf.MoveTowards(energy, 1.0f, energyAmount);
        print(energyAmount);
        
    }
}
