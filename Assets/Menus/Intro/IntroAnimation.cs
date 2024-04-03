using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OliverBeebe.UnityUtilities.Runtime;
using TMPro;

public class IntroAnimation : MonoBehaviour {

    [SerializeField] private RectTransform[] names;
    [SerializeField] private float startDelay;
    [SerializeField] private SmartCurve spinAngleAnimation;
    [SerializeField] private SmartCurve spinAmplitudeAnimation;
    [SerializeField] private SmartCurve spinScaleAnimation;
    [SerializeField] private ParticleSystem burstParticles;
    [SerializeField] private SmartCurve companyExpandAnimation;
    [SerializeField] private TextMeshProUGUI companyText;
    [SerializeField] private float companyVisibleDuration;
    [SerializeField] private SmartCurve companyDisappearAnimation;
    [SerializeField] private Scene loadScene;

    private void Start() {
        StartCoroutine(Animation());
    }

    private IEnumerator Animation() {

        companyText.enabled = false;

        foreach (var name in names)
            name.anchoredPosition = Vector2.positiveInfinity;

        yield return new WaitForSeconds(startDelay);

        spinAngleAnimation.Start();
        while (!spinAngleAnimation.Done) {

            float offset = spinAngleAnimation.Evaluate();
            float amplitude = spinAmplitudeAnimation.EvaluateAt(spinAngleAnimation.timer);
            float scale = spinScaleAnimation.EvaluateAt(spinAngleAnimation.timer);

            for (int i = 0; i < names.Length; i++) {
                float angle = (120f * i + offset) * Mathf.Deg2Rad;
                names[i].anchoredPosition = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * amplitude;
                names[i].localScale = Vector3.one * scale;
            }

            yield return null;
        }

        burstParticles.Play();

        companyExpandAnimation.Start();
        companyText.enabled = true;
        while (!companyExpandAnimation.Done) {
            companyText.characterSpacing = companyExpandAnimation.Evaluate();
            yield return null;
        }

        yield return new WaitForSeconds(companyVisibleDuration);

        companyDisappearAnimation.Start();
        while (!companyDisappearAnimation.Done) {
            companyText.transform.localScale = new(1, companyDisappearAnimation.Evaluate(), 1);
            yield return null;
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene(loadScene);
    }
}
