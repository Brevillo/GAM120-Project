using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EntityLazyBar : MonoBehaviour { 
    [SerializeField] private float healthSlow;
    [SerializeField] private EntityHealth health;
    [SerializeField] private Image barSlow;
    void Awake()
    {
        health.OnHeal += OnHeal;
    }

    void Update()
    {
        barSlow.fillAmount = Mathf.MoveTowards(barSlow.fillAmount, health.HealthPercent, healthSlow * Time.deltaTime);
    }

    void OnHeal(float healAmount)
    {
        if (health.HealthPercent > barSlow.fillAmount)
            barSlow.fillAmount = health.HealthPercent;

    }
 
}

