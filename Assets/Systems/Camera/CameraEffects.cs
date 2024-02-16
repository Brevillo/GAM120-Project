using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CameraEffects : MonoBehaviour {

    [SerializeField] private Volume volumeComponent;
    [SerializeField] private CanvasGroup whiteFade, blackFade;
    [SerializeField] private Setting cameraShakeSetting;

    private void Awake() {
        I = this;
        volumeComponent.profile = ScriptableObject.CreateInstance<VolumeProfile>();
    }

    // instance
    private static CameraEffects I;

    private static VolumeProfile effectVolume => I.volumeComponent.profile;

    private readonly List<ActiveShake> activeShakes = new();
    private readonly List<ActiveBounce> activeBounces = new();

    public delegate IEnumerator Routine<T>(T component) where T : VolumeComponent;

    public static EffectToken AddShake(CameraShakeProfile profile) {
        ActiveShake active = new(profile);
        I.activeShakes.Add(active);
        return active;
    }
    public static EffectToken AddBounce(CameraBounceProfile profile, Vector2 direction) {
        ActiveBounce active = new(profile, direction);
        I.activeBounces.Add(active);
        return active;
    }
    public static void PostProcessingEffect<T>(Routine<T> r) where T : VolumeComponent {
        if (!effectVolume.TryGet(out T component)) component = effectVolume.Add<T>();
        I.StartCoroutine(r.Invoke(component));
    }
    public static Coroutine BlackFade(SmartCurve curve) => I.NewFade(I.blackFade, curve);
    public static Coroutine WhiteFade(SmartCurve curve) => I.NewFade(I.whiteFade, curve);
    public static void RemoveEffect(EffectToken effect) {

        switch (effect) {
            case ActiveShake shake:     I.activeShakes.Remove(shake);   break;
            case ActiveBounce bounce:   I.activeBounces.Remove(bounce); break;
        }
    }

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

    private static Vector2 CalculateOffset<T>(List<T> effects) where T : ActiveEffect {

        Vector2 totalOffset = Vector2.zero;
        Vector2? largest = null;

        float largestMagnitude = 0;

        foreach (var effect in effects) {

            Vector2 offset = effect.Evaluate();

            switch (effect.interactionType) {

                case InteractionType.Additive:
                    totalOffset += offset;
                    break;

                case InteractionType.Override:

                    float magnitude = offset.magnitude;

                    if (magnitude > largestMagnitude) {
                        largestMagnitude = magnitude;
                        largest = offset;
                    }

                    break;
            }
        }

        effects.RemoveAll(ActiveEffect.IsCompleted);

        return largest ?? totalOffset;
    }

    private void LateUpdate() {

        transform.localPosition = cameraShakeSetting.boolValue
            ? CalculateOffset(activeShakes) + CalculateOffset(activeBounces)
            : Vector2.zero;
    }

    public class EffectToken {

    }

    private abstract class ActiveEffect : EffectToken {

        public ActiveEffect() {
            timer = 0;
        }

        protected float timer;
        protected abstract float duration { get; }
        public abstract InteractionType interactionType { get; }

        public abstract Vector2 Evaluate();

        public static bool IsCompleted(ActiveEffect effect) => effect.timer >= effect.duration;
    }

    private class ActiveShake : ActiveEffect {

        public ActiveShake(CameraShakeProfile profile) : base() {

            this.profile = profile;

            timer = 0;
            moveTimeRemaining = 0;

            prevTargetPosition = targetPosition = Vector2.zero;
        }

        private readonly CameraShakeProfile profile;

        private Vector2 prevTargetPosition, targetPosition;
        private float moveTimeRemaining;

        protected override float duration => profile.duration;
        public override InteractionType interactionType => profile.interactionType;

        public override Vector2 Evaluate() {

            float dt = profile.unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

            timer += dt;

            // calculate current amplitude
            float timePercent      = profile.duration == 0 ? 0 : timer / profile.duration,
                  intensityPercent = profile.intensityCurve.Evaluate(timePercent),
                  amplitude        = intensityPercent * profile.amplitude;

            Vector2 position = Vector2.zero;

            switch (profile.shakeType) {

                case CameraShakeProfile.ShakeType.NewPositionWithinRadiusEachFrame:

                    // choose random position
                    position = Random.insideUnitCircle * amplitude;

                    break;

                case CameraShakeProfile.ShakeType.MoveToRandomPositions:

                    // choose new target position
                    if (moveTimeRemaining <= 0) {
                        prevTargetPosition = targetPosition;
                        targetPosition = Random.insideUnitCircle * amplitude;
                        moveTimeRemaining = profile.timeBetweenPositions;
                    }

                    // decrease timer and calculate position
                    moveTimeRemaining -= dt;
                    float movePercent = 1 - moveTimeRemaining / profile.timeBetweenPositions;
                    position = Vector2.Lerp(prevTargetPosition, targetPosition, movePercent);

                    break;
            }

            return position;
        }
    }

    private class ActiveBounce : ActiveEffect {

        public ActiveBounce(CameraBounceProfile profile, Vector2 direction) : base() {

            this.profile = profile;
            this.direction = direction;

            timer = 0;
        }

        private readonly CameraBounceProfile profile;
        private readonly Vector2 direction;

        protected override float duration => profile.duration;
        public override InteractionType interactionType => profile.interactionType;

        public override Vector2 Evaluate() {

            float dt = profile.unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

            timer += dt;

            // calculate current amplitude
            float timePercent      = timer / profile.duration,
                  intensityPercent = profile.intensityCurve.Evaluate(timePercent),
                  amplitude        = intensityPercent * profile.amplitude;

            return direction * amplitude;
        }
    }

    public enum InteractionType {

        [Tooltip("Shake gets added to other active shakes.")]
        Additive,

        [Tooltip("Shake will override all shakes if it has a higher magnitude.")]
        Override,
    }
}

[System.Serializable]
public class CameraShakeProfile {

    [Tooltip("How large the shake will be.")]
    public float amplitude = 0.25f;

    [Tooltip("How long the shake will be.")]
    public float duration = 0.15f;

    [Tooltip("Is the shake affected by time scale?")]
    public bool unscaledTime = false;

    [Tooltip("The percent intensity of the shake over the duration.")]
    public AnimationCurve intensityCurve = AnimationCurve.Linear(0, 1, 1, 0);

    [Tooltip("How the shake will interact with other active shakes.")]
    public CameraEffects.InteractionType interactionType = CameraEffects.InteractionType.Additive;

    [Tooltip("How the shake moves the camera.")]
    public ShakeType shakeType = ShakeType.NewPositionWithinRadiusEachFrame;

    [Tooltip("Duration it takes for the camera to move between shake positions.")]
    public float timeBetweenPositions = 0.02f;

    public enum ShakeType {

        [Tooltip("Every frame the camera moves to a new position within the the amplitude radius.")]
        NewPositionWithinRadiusEachFrame,

        [Tooltip("The camera will move between random positions at a specified speed.")]
        MoveToRandomPositions,
    }

    #region Editor
#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(CameraShakeProfile))]
    private class CameraShakeProfilePropertyDrawer : PropertyDrawer {

        private bool foldoutActive;

        private readonly string[] properties = new[] {
            nameof(amplitude),
            nameof(duration),
            nameof(unscaledTime),
            nameof(intensityCurve),
            nameof(interactionType),
            nameof(shakeType),
        };

        private const string conditionalProp = nameof(timeBetweenPositions);

        // found this here, search for float kIndentPerLevel: https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/EditorGUI.cs
        private const float indentWidth = 15f;

        private CameraShakeProfile GetProfile(SerializedProperty property)
            => fieldInfo.GetValue(property.serializedObject.targetObject) as CameraShakeProfile;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

            void Property(string name) {
                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField(position, property.FindPropertyRelative(name));
            }

            position.height = EditorGUIUtility.singleLineHeight;
            foldoutActive = EditorGUI.Foldout(new(position) { xMin = position.xMin - indentWidth}, foldoutActive, label, true);

            if (foldoutActive) {

                EditorGUI.indentLevel++;

                foreach (var prop in properties)
                    Property(prop);

                if (GetProfile(property).shakeType == ShakeType.MoveToRandomPositions)
                    Property(conditionalProp);

                EditorGUI.indentLevel--;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {

            int lines = foldoutActive
                ? 7 + (GetProfile(property).shakeType == ShakeType.MoveToRandomPositions ? 1 : 0)
                : 1;

            return EditorGUIUtility.singleLineHeight * lines + EditorGUIUtility.standardVerticalSpacing * (lines - 1);
        }
    }

#endif
    #endregion
}

[System.Serializable]
public class CameraBounceProfile {

    [Tooltip("How large the bounce will be.")]
    public float amplitude = 0.25f;

    [Tooltip("How long the bounce will be.")]
    public float duration = 0.15f;

    [Tooltip("Is the bounce affected by time scale?")]
    public bool unscaledTime = false;

    [Tooltip("The percent intensity of the bounce over the duration.")]
    public AnimationCurve intensityCurve = AnimationCurve.Linear(0, 1, 1, 0);

    [Tooltip("How the bounce will interact with other active bounces.")]
    public CameraEffects.InteractionType interactionType = CameraEffects.InteractionType.Additive;
}
