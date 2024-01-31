using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EntityHealthBar : MonoBehaviour {

    [SerializeField] private EntityHealth health;
    [SerializeField] private Image bar;

    private void Awake() {
        health.OnHealthUpdated += () => bar.fillAmount = health.HealthPercent;
    }
}
