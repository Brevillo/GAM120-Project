using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PurpleHummingbird : BlueHummingbird
{
    [Header("Babies")]
    [SerializeField] private int spawnNumber;
    [SerializeField] private GameObject miniPurplePrefab;

    protected override void Awake()
    {
        base.Awake();
        Health.OnDeath += Health_OnDeath;

    }

    private void Health_OnDeath(DamageInfo info)
    {
        for(int i = 0; i < spawnNumber; i++)
        {
            Instantiate(miniPurplePrefab, Position, Quaternion.identity);
        }
    }
}
