using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : GenericEnemyComponent {

    [Header("Effects")]
    [SerializeField] private SoundEffect hurtSound;

    [Header("Hit Flash")]
    [SerializeField] private float flashDuration;
    [SerializeField] private List<SpriteRenderer> hitFlashRenderers;
    [SerializeField] private Material flashMaterial;

    private void Awake() {
        Health.OnTakeDamage += OnTakeDamage;
        Health.OnDeath += OnDeath;
    }

    protected virtual void OnTakeDamage(DamageInfo info) {

        if (hurtSound != null) hurtSound.Play(this); 

        StartCoroutine(SpriteFlash());
        IEnumerator SpriteFlash() {
            
            List<(SpriteRenderer rend, Material ogMaterial, Color ogColor)> rends = hitFlashRenderers.ConvertAll(rend => (rend, rend.material, rend.color));

            foreach (var rend in hitFlashRenderers) {
                rend.material = flashMaterial;
                rend.color = Color.white;
            }

            yield return new WaitForSeconds(flashDuration);

            foreach (var (rend, ogMaterial, ogColor) in rends) {
                rend.material = ogMaterial;
                rend.color = ogColor;
            }
        }
    }

    protected virtual void OnDeath(DamageInfo info) {

        OnTakeDamage(info);

        StartCoroutine(DeathDelay());
        IEnumerator DeathDelay() {

            yield return new WaitForSeconds(flashDuration);

            gameObject.SetActive(false);
        }
    }
}
