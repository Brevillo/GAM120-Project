using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour {

    [SerializeField] private DialogueObject dialogue;
    [SerializeField] private Transform spawnPoint;

    private GameObject instance;

    private void OnTriggerEnter2D(Collider2D collision) {

        if (instance == null && collision.TryGetComponent(out Player player))
            instance = dialogue.Spawn(player, spawnPoint);
    }

    private void OnTriggerExit2D(Collider2D collision) {

        if (instance != null && collision.TryGetComponent(out Player _))
            Destroy(instance);
    }
}
