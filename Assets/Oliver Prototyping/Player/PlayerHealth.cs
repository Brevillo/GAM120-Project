using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : Player.Component {

    [Header("Effects")]
    [SerializeField] private CameraShakeProfile damageShake;
    [SerializeField] private float damageTimeFreezeDuration;

    private void Awake() {
        Health.OnTakeDamage += TakeDamage;
    }

    private void TakeDamage(DamageInfo info) {

        TimeManager.FreezeTime(damageTimeFreezeDuration, this);
        CameraEffects.AddShake(damageShake);
        CameraEffects.PostProcessingEffect<UnityEngine.Rendering.Universal.ColorAdjustments>(
            duration:   damageTimeFreezeDuration,
            unscaled:   true,
            preEffect:  color => color.saturation.value = -100,
            postEffect: color => color.saturation.value = 0);

        Movement.TakeKnockback(info.knockback);
    }
}
