using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class KnightFade : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    
    [Header("UI Elements")]
    public Image bar;
    public Image fill;
    
    private float lifetime = 20f;
    public float fadeDuration = 1f;

    void Start()
    {
        StartCoroutine(LifetimeSequence());
    }

    IEnumerator LifetimeSequence()
    {
        yield return new WaitForSeconds(lifetime - fadeDuration);

        Color originalSpriteColor = spriteRenderer != null ? spriteRenderer.color : Color.white;
        Color originalBarColor = bar != null ? bar.color : Color.white;
        Color originalFillColor = fill != null ? fill.color : Color.white;

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeDuration;
            float targetAlpha = 1f - progress;

            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(originalSpriteColor.r, originalSpriteColor.g, originalSpriteColor.b, targetAlpha);
            }

            if (bar != null)
            {
                bar.color = new Color(originalBarColor.r, originalBarColor.g, originalBarColor.b, targetAlpha);
            }

            if (fill != null)
            {
                fill.color = new Color(originalFillColor.r, originalFillColor.g, originalFillColor.b, targetAlpha);
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}