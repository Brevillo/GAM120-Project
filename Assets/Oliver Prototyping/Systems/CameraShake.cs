using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CameraShake : MonoBehaviour {

    [SerializeField] private Transform shakeTransform;

    private void Awake() {
        I = this;
    }

    // instance
    private static CameraShake I;

    private readonly List<ActiveShake> activeShakes = new();

    public static void AddShake(CameraShakeProfile profile) => I.activeShakes.Add(new(profile));

    private void Update() {

        Vector2 totalOffset = Vector2.zero;

        foreach (var shake in activeShakes)
            totalOffset = shake.Evaluate(totalOffset);

        activeShakes.RemoveAll(ActiveShake.IsCompleted);

        shakeTransform.localPosition = totalOffset;
    }

    private class ActiveShake {

        public ActiveShake(CameraShakeProfile profile) {

            this.profile = profile;

            timer = 0;
            moveTimeRemaining = 0;

            prevTargetPosition = targetPosition = Vector2.zero;
        }

        private readonly CameraShakeProfile profile;

        private float timer;

        private Vector2 prevTargetPosition, targetPosition;
        private float moveTimeRemaining;

        public Vector2 Evaluate(Vector2 current) {

            float dt = profile.unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

            timer += dt;

            // calculate current amplitude
            float timePercent      = timer / profile.duration,
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

            switch (profile.interactionType) {

                case CameraShakeProfile.InteractionType.Additive:
                    current += position;
                    break;

                case CameraShakeProfile.InteractionType.Override:
                    if (position.magnitude > current.magnitude)
                        current = position;
                    break;
            }

            return current;
        }

        public static bool IsCompleted(ActiveShake shake) => shake.timer >= shake.profile.duration;
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
    [Tooltip("How the shake will interact with other active shapes.")]
    public InteractionType interactionType = InteractionType.Additive;
    [Tooltip("How the shake moves the camera.")]
    public ShakeType shakeType = ShakeType.NewPositionWithinRadiusEachFrame;
    [Tooltip("Duration it takes for the camera to move between shake positions.")]
    public float timeBetweenPositions = 0.02f;

    public enum InteractionType {

        [Tooltip("Shake gets added to other active shakes.")]
        Additive,

        [Tooltip("Shake will override all shakes if it has a higher magnitude.")]
        Override,
    }

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