using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuRotation : MonoBehaviour {

    [SerializeField] private float rotationSpeed;

    private void Update() {

        transform.eulerAngles += Vector3.up * rotationSpeed * Time.deltaTime;
    }
}
