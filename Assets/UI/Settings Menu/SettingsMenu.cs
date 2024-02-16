using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsMenu : MonoBehaviour {

    [SerializeField] private Setting
        fullscreen,
        frameRate;

    private void Awake() {
        fullscreen.onValueChanged += () => Screen.fullScreen = fullscreen.boolValue;
        frameRate .onValueChanged += () => Application.targetFrameRate = frameRate.intValue;
    }
}
