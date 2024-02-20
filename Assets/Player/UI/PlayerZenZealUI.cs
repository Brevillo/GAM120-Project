using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OliverBeebe.UnityUtilities.Runtime;

public class PlayerZenZealUI : MonoBehaviour {

    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Image zenImage, zealImage;
    [SerializeField] private Slider halfwayMarker;

    [Header("Zeal Particles")]
    [SerializeField] private ParticleSystem zealBarParticles;
    [SerializeField] private SmartCurve zealParticlesEmissionRate, zealParticlesSpeed;

    [Header("Zen Particles")]
    [SerializeField] private ParticleSystem zenBarParticles;
    [SerializeField] private SmartCurve zenParticlesEmissionRate, zenParticlesSpeed;
    [SerializeField] private SmartCurve zenBarParticlesEmissionRate, zenBarParticlesSpeed;

    private void Awake() {
        playerHealth.OnEnergyUpdated += OnEnergyUpdated;
    }

    private void OnEnergyUpdated() {

        float zen = playerHealth.Energy,
              zeal = 1 - zen;

        zenImage.fillAmount = zen;
        zealImage.fillAmount = zeal;
        halfwayMarker.value = zen;

        var rect = (transform as RectTransform).rect;

        // zeal particles

        var zealParticlesShape = zealBarParticles.shape;
        zealParticlesShape.scale = new(rect.width * zeal, rect.height, 0);
        zealParticlesShape.position = new(Mathf.Lerp(rect.xMax - rect.width * zeal, rect.xMax, 0.5f), 0, 0);

        var zealParticlesEmission = zealBarParticles.emission;
        zealParticlesEmission.rateOverTime = zealParticlesEmissionRate.EvaluateAt(zeal);

        var zealParticlesMain = zealBarParticles.main;
        zealParticlesMain.simulationSpeed = zealParticlesSpeed.EvaluateAt(zeal);

        // zen particles

        var zenBarParticlesShape = zenBarParticles.shape;
        zenBarParticlesShape.scale = new(rect.width * zen, 0, rect.height);
        zenBarParticlesShape.position = new(Mathf.Lerp(rect.xMin, rect.xMin + rect.width * zen, 0.5f), 0, 0);

        var zenBarParticlesEmission = zenBarParticles.emission;
        zenBarParticlesEmission.rateOverTime = zenBarParticlesEmissionRate.EvaluateAt(zen);

        var zenBarParticlesMain = zenBarParticles.main;
        zenBarParticlesMain.simulationSpeed = zenBarParticlesSpeed.EvaluateAt(zen);
    }
}
