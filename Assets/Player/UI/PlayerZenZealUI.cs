using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OliverBeebe.UnityUtilities.Runtime;

public class PlayerZenZealUI : MonoBehaviour {

    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Image zenImage, zealImage;
    [SerializeField] private Slider halfwayMarker;
    [SerializeField] private ParticleSystem zealParticles;
    [SerializeField] private SmartCurve zealParticlesEmissionRate, zealParticlesSpeed;

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

        var zealParticlesShape = zealParticles.shape;
        zealParticlesShape.scale = new(rect.width * zeal, rect.height, 1);
        zealParticlesShape.position = new(Mathf.Lerp(rect.xMax, rect.xMin + rect.width * zen, 0.5f), 0, 0);

        var zealParticlesEmission = zealParticles.emission;
        zealParticlesEmission.rateOverTime = zealParticlesEmissionRate.EvaluateAt(zeal);

        var zealParticlesMain = zealParticles.main;
        zealParticlesMain.simulationSpeed = zealParticlesSpeed.EvaluateAt(zeal);
    }
}
