using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EntityLazyBar : MonoBehaviour { 

    [SerializeField] private float healthSlow;
    [SerializeField] private EntityHealth health;
    [SerializeField] private Image barSlow;

    private void Update() {

        // move bar fill amount towards health percent, but move it up instantly if health becomes greater (like through healing)
        barSlow.fillAmount = Mathf.Max(health.HealthPercent, Mathf.MoveTowards(barSlow.fillAmount, health.HealthPercent, healthSlow * Time.deltaTime));
    }
}

