using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OliverBeebe.UnityUtilities.Runtime;

public class LevelEndTrigger : MonoBehaviour {

    [SerializeField] private Scene loadScene;
    [SerializeField] private SmartCurve fadeOut;

    private void OnTriggerEnter2D(Collider2D collision) {

        if (collision.TryGetComponent(out Player player)) {

            player.Freeze(true, true, true, true);

            StartCoroutine(FadeOut());
            IEnumerator FadeOut() {

                yield return CameraEffects.BlackFade(fadeOut);

                UnityEngine.SceneManagement.SceneManager.LoadScene(loadScene);
            }
        }
    }
}
