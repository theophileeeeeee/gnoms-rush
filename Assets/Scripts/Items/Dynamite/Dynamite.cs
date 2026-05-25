using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Dynamite : MonoBehaviour
{
    public float explosionDelay = 1.5f;
    public float radius = 5f;
    public int damage = 50;
    public Animator animator;

    public float explosionScaleMultiplier = 2f;
    public float scaleDuration = 0.15f;

    [Header("Audio")]
    public AudioClip explosionSound;
    public float volume = 1f;

    private Vector3 initialScale;
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    void Start()
    {
        initialScale = transform.localScale;
        Invoke(nameof(Explode), explosionDelay);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    void Explode()
    {
        StartCoroutine(ExplosionEffect());

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);

        foreach (var hit in hits)
        {
            EnemyMovement enemy = hit.GetComponent<EnemyMovement>();
            if (enemy != null)
                enemy.TakeDamage(damage);
        }

        if (explosionSound != null)
            audioSource.PlayOneShot(explosionSound, volume);

        animator?.SetTrigger("Boom");

        StartCoroutine(DestroyAfterAnimation());
    }

    IEnumerator ExplosionEffect()
    {
        Vector3 targetScale = initialScale * explosionScaleMultiplier;

        float t = 0f;
        while (t < scaleDuration)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(initialScale, targetScale, t / scaleDuration);
            yield return null;
        }

        transform.localScale = targetScale;
    }

    IEnumerator DestroyAfterAnimation()
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
}