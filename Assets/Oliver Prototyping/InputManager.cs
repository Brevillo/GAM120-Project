using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour {

    [SerializeField] private InputActionAsset inputActions;

    [Header("Movement")]
    public Button Jump;
    public Axis Movement;
    public Button Whip;

    [Header("Debug")]
    public Button Debug1;
    public Button Debug2;
    public Button Debug3;
    public Button Debug4;

    private List<Button> buttons;

    private void Awake() {

        buttons = new() {

            Jump,
            Movement,
            Whip,

            Debug1,
            Debug2,
            Debug3,
            Debug4,
        };

        foreach (var button in buttons)
            button.Init();
    }

    private void Update() {

        foreach (var button in buttons)
            button.Update();
    }

    private void OnEnable() {
        inputActions.Enable();
    }

    private void OnDisable() {
        inputActions.Disable();
    }

    [System.Serializable]
    public class Button {

        [SerializeField] protected InputActionReference actionReference;

        public bool Enabled = true;
        public bool Pressed     => Enabled && pressed;
        public bool Down        => Enabled && down;
        public bool Released    => Enabled && released;

        public void ForceNew() {
            pressed = false;
            forceNew = true;
        }

        public void UnforceNew() {
            forceNew = false;
        }

        #region Internals

        public virtual void Init() {
            actionReference.action.performed += context => pressed = true;
            actionReference.action.canceled += context => pressed = false;
        }

        [Readonly, SerializeField] protected bool pressed     = false;
        [Readonly, SerializeField] protected bool down        = false;
        [Readonly, SerializeField] protected bool released    = false;
        protected bool pressedLast = false;

        protected bool forceNew;

        public void Update() {

            down        = pressed && !pressedLast;
            released    = !pressed && pressedLast;
            pressedLast = pressed;

            if (down) forceNew = false;
        }

        #endregion
    }

    [System.Serializable]
    public class Axis : Button {

        [Tooltip("If true, sets the value to zero when not pressed.")]
        [SerializeField] private bool impulse;

        public override void Init() {

            base.Init();

            actionReference.action.performed += context => {
                _vector = context.ReadValue<Vector2>();
                OnPerformed?.Invoke();
            };

            if (impulse) actionReference.action.canceled += context => _vector = Vector2.zero;
        }

        public Vector2 Vector => Enabled && !forceNew ? _vector : Vector2.zero;

        public static implicit operator Vector2(Axis axis) => axis.Vector;
        public static implicit operator Vector3(Axis axis) => axis.Vector;

        public event System.Action OnPerformed;

        [Readonly, SerializeField] private Vector2 _vector;
    }
}
