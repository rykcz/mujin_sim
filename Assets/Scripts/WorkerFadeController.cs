using System.Collections;
using UnityEngine;

public class WorkerFadeController : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void FadeOutAndDisable()
    {
        StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator FadeOutRoutine()
    {
        float duration = 1.0f;
        float timer = 0f;

        Color startColor = spriteRenderer.color;

        while (timer < duration)
        {
            float t = timer / duration;
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, 1f - t);
            timer += Time.deltaTime;
            yield return null;
        }

        spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
        gameObject.SetActive(false);
    }

    public void FadeIn()
    {
        StartCoroutine(FadeInRoutine());
    }

    private IEnumerator FadeInRoutine()
    {
        float duration = 1.0f;
        float timer = 0f;

        Color startColor = spriteRenderer.color;
        startColor.a = 0f;
        spriteRenderer.color = startColor;

        while (timer < duration)
        {
            float t = timer / duration;
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, t);
            timer += Time.deltaTime;
            yield return null;
        }

        spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, 1f);
    }
}
