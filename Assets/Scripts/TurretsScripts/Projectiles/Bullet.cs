using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Bullet : MonoBehaviour
{
    private Transform target;
    public float speed = 40f;
    public GameObject impactEffect;
    public float damage = 2f;

    [Header("Audio")]
    public AudioClip hitSound;
    public float volume = 1f;

    private AudioSource audioSource;
    private bool aTouche = false;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    public void Seek(Transform _target)
    {
        target = _target;
    }

    void Update()
    {
        if (aTouche) return;

        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 dir = target.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        if (dir.magnitude <= distanceThisFrame)
        {
            HitTarget();
            return;
        }

        transform.Translate(dir.normalized * distanceThisFrame, Space.World);
    }

    void HitTarget()
    {
        aTouche = true;

        GameObject effectIns = Instantiate(impactEffect, transform.position, transform.rotation);
        Destroy(effectIns, 1f);

        EnemyMovement enemy = target.GetComponent<EnemyMovement>();
        if (enemy != null)
            enemy.TakeDamage(damage);

        float delay = 0.1f;

        if (hitSound != null)
        {
            audioSource.PlayOneShot(hitSound, volume);
            delay = hitSound.length;
        }

        var renderer = GetComponent<Renderer>();
        if (renderer != null) renderer.enabled = false;

        var collider = GetComponent<Collider2D>();
        if (collider != null) collider.enabled = false;

        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

        Destroy(gameObject, delay);
    }
}