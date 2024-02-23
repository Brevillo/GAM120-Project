using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialTrigger : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeTime;
    [SerializeField] private InputActionReference actionReference;

    private enum State { Waiting, Triggered, Finished }
    private State state;

    private void Awake()
    {
        state = State.Waiting;

        actionReference.action.performed += Action_performed;
    }

    private void Action_performed(InputAction.CallbackContext obj)
    {
        if (state == State.Triggered)
        {
            state = State.Finished;
            StartCoroutine(CanvasFade(1, 0));
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (state == State.Waiting && collision.TryGetComponent(out Player player))
        {
            state = State.Triggered;
            StartCoroutine(CanvasFade(0, 1));
        }
    }

    private IEnumerator CanvasFade(float start, float end)
    {
        float timer = 0;
        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            float timePercent = timer / fadeTime;
            canvasGroup.alpha = Mathf.Lerp(start, end, timePercent);
            yield return null;
        }
    }
}
