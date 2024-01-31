using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReferenceValue : ScriptableObject {

    [SerializeField] private float value;

    public float Value => value;
    public static implicit operator float(ReferenceValue value) => value.value;
}
