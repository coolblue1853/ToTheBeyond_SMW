using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenFader : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public GameObject loadingObj;
    public float fadeDuration = 1f;

    public IEnumerator FadeOut(bool isOffLoading = true)
    {
        loadingObj.SetActive(isOffLoading);

        float t = 0;
        while (t < fadeDuration)
        {
            t+=Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0, 1, t/fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1;
    }

    public IEnumerator FadeIn(bool isOffLoading = false)
    {
        float t = 0;
        while (t < fadeDuration)
        {
            t+=Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1, 0, t/fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 0;
    }
}
