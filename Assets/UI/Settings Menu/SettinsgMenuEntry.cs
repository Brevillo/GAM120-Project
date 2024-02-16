using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettinsgMenuEntry : MonoBehaviour {

    [SerializeField] private Setting setting;
    [SerializeField] private string labelText;

    [SerializeField] private TextMeshProUGUI label;

    [Header("Slider")]
    [SerializeField] private Slider slider;
    [SerializeField] private float minValue, maxValue, increment, multiplyDisplay;
    [SerializeField] private bool percent;
    [SerializeField] private TextMeshProUGUI sliderDisplay;

    [Header("Toggle")]
    [SerializeField] private Toggle toggle;

    private void OnValidate() {

        if (label != null) label.text = labelText;

        if (setting == null) return;

        if (slider != null) {

            if (sliderDisplay != null) sliderDisplay.text = setting.floatValue * multiplyDisplay + (percent ? "%" : "");

            slider.wholeNumbers = true;
            slider.minValue = minValue / increment;
            slider.maxValue = maxValue / increment;
            slider.SetValueWithoutNotify(setting.floatValue / increment);
        }

        if (toggle != null) {
            toggle.isOn = setting.boolValue;
        }
    }

    private void Awake() {

        label.text = labelText;

        if (slider != null) {

            slider.wholeNumbers = true;
            slider.minValue = minValue / increment;
            slider.maxValue = maxValue / increment;

            float value = setting.floatValue / increment;

            slider.onValueChanged.AddListener(value => {
                value *= increment;
                sliderDisplay.text = value * multiplyDisplay + (percent ? "%" : "");
                setting.SetValue(value);
            });

            slider.value = value;
        }

        if (toggle != null) {

            toggle.onValueChanged.AddListener(value => setting.SetValue(value));

            toggle.isOn = setting;
        }

        //if (gameObject.TryGetComponentInChildren(out ))
    }
}
