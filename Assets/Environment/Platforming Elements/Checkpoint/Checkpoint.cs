using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OliverBeebe.UnityUtilities.Runtime;

public class Checkpoint : MonoBehaviour {

    [SerializeField] private Transform spinPivot;
    [SerializeField] private SmartCurve activationSpin, activationScale;
    [SerializeField] private ParticleSystem particles;

    public void Deregister() {
        particles.Stop(true);
    }

    private void OnTriggerEnter2D(Collider2D collision) {

        if (collision.TryGetComponent(out PlayerHealth player) && player.RegisterCheckpoint(this)) {

            particles.Play(true);
            player.GetComponent<EntityHealth>().FullHeal();

            StartCoroutine(Spin());
            IEnumerator Spin() {

                activationSpin.Start();
                while (!activationSpin.Done) {
                    spinPivot.localEulerAngles = Vector3.forward * activationSpin.Evaluate();
                    spinPivot.localScale = Vector3.one * (1 + activationScale.Evaluate());
                    yield return null;
                }
            }
        }
    }
}
