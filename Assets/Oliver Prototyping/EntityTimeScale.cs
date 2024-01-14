using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityTimeScale : MonoBehaviour {

    public float timeScale = 1;

    public static implicit operator float(EntityTimeScale entity) => entity.timeScale;
}
