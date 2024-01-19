using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHornTrigger : MonoBehaviour {

    [SerializeField] private EntityHealthTeam team;
    [SerializeField] private float damage;

    private void OnTriggerEnter2D(Collider2D collision) {

        if (collision.TryGetComponent(out EntityHealth entity) && entity.Team != team)
            entity.TakeDamage(new(damage, entity.transform.position - transform.position));
    }
}
