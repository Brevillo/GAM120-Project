using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : GenericEnemyComponent {

    [Header("Hit Flash")]
    [SerializeField] private float flashDuration;
    [SerializeField] private SpriteRenderer[] hitFlashRenderers;
    [SerializeField] private Material flashMaterial;

    private void Awake() {
        Health.OnTakeDamage += OnTakeDamage;
        Health.OnDeath += OnDeath;
    }

    protected virtual void OnTakeDamage(DamageInfo info) {
        
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

    protected virtual void OnDeath(DamageInfo info) {
        gameObject.SetActive(false);
    }
}
