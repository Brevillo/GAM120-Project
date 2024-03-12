using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class DialogueObject : ScriptableObject {

    [SerializeField] private DialogueManager managerPrefab;
    [TextArea] public List<string> lines;

    public GameObject Spawn(Player player, Transform parent) {

        var manager = Instantiate(managerPrefab, parent);
        manager.Initialize(this, player.Input.Interact);

        return manager.gameObject;
    }
}
