using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using OliverBeebe.UnityUtilities.Runtime.Camera;
using OliverBeebe.UnityUtilities.Runtime;

public class CameraEffects : MonoBehaviour {

    [SerializeField] private Volume volumeComponent;
    [SerializeField] private CanvasGroup whiteFade, blackFade;
    [SerializeField] private Setting cameraShakeSetting;

    private CameraEffectsManager cameraEffectsManager = new();

    public static CameraEffectsManager Effects => I.cameraEffectsManager;

    private void Awake() {
        I = this;
        volumeComponent.profile = ScriptableObject.CreateInstance<VolumeProfile>();

        cameraEffectsManager = new();
    }

    // instance
    private static CameraEffects I;

    private static VolumeProfile effectVolume => I.volumeComponent.profile;

    public delegate IEnumerator Routine<T>(T component) where T : VolumeComponent;

    public static void PostProcessingEffect<T>(Routine<T> r) where T : VolumeComponent {
        if (!effectVolume.TryGet(out T component)) component = effectVolume.Add<T>();
        I.StartCoroutine(r.Invoke(component));
    }

    public static Coroutine BlackFade(SmartCurve curve) => I.NewFade(I.blackFade, curve);
    public static Coroutine WhiteFade(SmartCurve curve) => I.NewFade(I.whiteFade, curve);

    private Coroutine activeFade;
    private Coroutine NewFade(CanvasGroup group, SmartCurve curve) {

        if (activeFade != null) StopCoroutine(activeFade);

        return activeFade = StartCoroutine(Fade());

        IEnumerator Fade() {

            SmartCurve fade = new(curve);

            fade.Start();

            while (!fade.Done) {
                group.alpha = fade.Evaluate();
                yield return null;
            }
        }
    }

    private void LateUpdate() {

        transform.localPosition = cameraShakeSetting.boolValue
            ? cameraEffectsManager.Update()
            : Vector2.zero;
    }
}