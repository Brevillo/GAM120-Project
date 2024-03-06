using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideGround : MonoBehaviour {

    [SerializeField] private new Renderer renderer;

    private void Start() {
        renderer.enabled = false;
    }
}
