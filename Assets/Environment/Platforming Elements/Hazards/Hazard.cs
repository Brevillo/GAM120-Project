using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Hazard : MonoBehaviour {

    [SerializeField] private float damage, knockbackPercent;
    [SerializeField] private bool respawn;
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private List<EntityHealthCollisionTrigger> triggers;

    private void Awake() {
        foreach (var trigger in triggers)
            trigger.OnEntityStay.AddListener(OnEntityCollision);
    }

    private void OnEntityCollision(EntityHealthCollision collision) {

        Vector2 entityPosition = collision.entity.transform.position,
                selfPosition = collision.collider.ClosestPoint(entityPosition),
                dirToEntity = (entityPosition - selfPosition).normalized;

        collision.entity.TakeDamage(new(damage, dirToEntity, Vector2.one * knockbackPercent, respawn, respawnPoint.position));
    }

    //#region Editor
    //#if UNITY_EDITOR

    //[CustomEditor(typeof(Hazard))]
    //private class HazardEditor : Editor {

    //    public override void OnInspectorGUI()
    //    {
    //        base.OnInspectorGUI();
    //    }
    //}

    //#endif
    //#endregion
}
