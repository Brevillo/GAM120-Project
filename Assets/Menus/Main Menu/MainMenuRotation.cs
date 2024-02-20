using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OliverBeebe.UnityUtilities.Runtime;

public class MainMenuRotation : MonoBehaviour {

    [SerializeField] private float rotationSpeed;
    [SerializeField] private Wave3 cameraBob;

    private void Update() {

        transform.localEulerAngles += Vector3.up * rotationSpeed * Time.deltaTime;
        transform.localPosition = cameraBob.Evaluate();
    }
}
