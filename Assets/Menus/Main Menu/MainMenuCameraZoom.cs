using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OliverBeebe.UnityUtilities.Runtime;

public class MainMenuCameraZoom : MonoBehaviour
{
    [SerializeField] private Transform cameraZoomPivot;
    [SerializeField] private Transform zoomTarget;
    [SerializeField] private SmartCurve zoomCurve;
    [SerializeField] private float fadeOutDelay;
    [SerializeField] private SmartCurve fadeOut;
    [SerializeField] private Scene loadScene;

    private Coroutine zoom;

    public void StartZoom()
    {
        if (zoom != null) return;

        zoom = StartCoroutine(Zoom());

        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(fadeOutDelay);

        yield return CameraEffects.BlackFade(fadeOut);

        UnityEngine.SceneManagement.SceneManager.LoadScene(loadScene);
    }

    private IEnumerator Zoom()
    {
        Vector3 start = cameraZoomPivot.position;

        zoomCurve.Start();
        while (!zoomCurve.Done)
        {
            cameraZoomPivot.position = Vector3.Lerp(start, zoomTarget.position, zoomCurve.Evaluate());
            yield return null;
        }
    }
}
