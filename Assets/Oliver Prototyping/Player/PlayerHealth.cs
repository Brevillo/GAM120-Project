using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : Player.Component {

    [SerializeField] private Image ZealBar;
    [Header("Effects")]
    [SerializeField] private CameraShakeProfile damageShake;
    [SerializeField] private float damageTimeFreezeDuration;
    private float energy;

    private void Awake() {
        Health.OnTakeDamage += TakeDamage;
    }

    private void Update()
    {
        ZealBar.fillAmount = energy;
        float healAmount = energy switch
        { 
            > 0.75f => 2,
            > 0.5f => 1,
            _ => 0,
        };

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
        
        energy -= 1.0f;

        Movement.TakeKnockback(info.knockback);
    }

    public void IncreaseZen(float energyAmount) {

        energy += energyAmount;
        print(energyAmount);
        
    }
}
