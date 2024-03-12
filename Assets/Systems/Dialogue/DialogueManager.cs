using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using OliverBeebe.UnityUtilities.Runtime.Input;

public class DialogueManager : MonoBehaviour {

    [SerializeField] private TextMeshProUGUI textMesh;

    private DialogueObject dialogue;
    private InputManager.Button button;

    public void Initialize(DialogueObject dialogue, InputManager.Button button) {

        this.dialogue = dialogue;
        this.button = button;

        StartCoroutine(Routine());
    }

    private IEnumerator Routine() {

        while (true)
            foreach (var line in dialogue.lines) {

                textMesh.text = line;

                yield return new WaitUntil(() => !button.Pressed);
                yield return new WaitUntil(() => button.Down);
            }
    }
}
