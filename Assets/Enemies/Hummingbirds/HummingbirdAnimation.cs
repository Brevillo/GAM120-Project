using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HummingbirdAnimation : MonoBehaviour {

    [Header("Wings")]
    [SerializeField] private Wave hoverOscillation;
    [SerializeField] private float wingPivotAngle, wingPivotRange;
    [SerializeField] private Transform visualsPivot, wingPivot;

    [Header("Hit Flash")]
    [SerializeField] private float flashDuration;
    [SerializeField] private SpriteRenderer[] hitFlashRenderers;
    [SerializeField] private Material flashMaterial;

    [Header("References")]
    [SerializeField] private new Rigidbody2D rigidbody;
    [SerializeField] private EntityHealth health;

    public bool turnToTarget;
    public Transform target;

    private bool wingFlip;

    private void Awake() {
        health.OnTakeDamage += OnTakeDamage;
    }

    private void OnTakeDamage(DamageInfo info) {

        StartCoroutine(SpriteFlash());
        IEnumerator SpriteFlash() {

            List<(SpriteRenderer rend, Material ogMaterial, Color color)> rends = new List<SpriteRenderer>(hitFlashRenderers).ConvertAll(rend => (rend, rend.material, rend.color));

            foreach (var rend in hitFlashRenderers) {
                rend.material = flashMaterial;
                rend.color = Color.white;
            }

            yield return new WaitForSeconds(flashDuration);

            foreach (var (rend, ogMaterial, color) in rends) {
                rend.material = ogMaterial;
                rend.color = color;
            }
        }
    }

    private void Start() {
        hoverOscillation.offset = Random.value;
    }

    private void Update() {

        visualsPivot.localPosition = Vector2.up * hoverOscillation.Evaluate();

        wingFlip = !wingFlip;
        wingPivot.localEulerAngles = Vector3.forward * (wingPivotAngle + (wingFlip ? -1 : 1) * wingPivotRange / 2f);

        int direction = (int)Mathf.Sign(turnToTarget ? (target.position - transform.position).x : rigidbody.velocity.x);
        visualsPivot.localScale = new(direction, 1, 1);
    }
}
