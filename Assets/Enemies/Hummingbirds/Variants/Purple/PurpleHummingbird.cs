using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PurpleHummingbird : RedHummingbird
{
    [Header("Babies")]
    [SerializeField] private List<GameObject> babies;

    protected override void Awake()
    {
        base.Awake();
        Health.OnDeath += OnDeath;
    }

    protected override IEnumerator Behaviour() {

        yield return null;

        foreach (var baby in babies)
            baby.SetActive(false);

        yield return base.Behaviour();
    }

    private void OnDeath(DamageInfo info)
    {
        foreach (var baby in babies)
        {
            baby.SetActive(true);
            baby.transform.position = transform.position;
        }
    }
}
