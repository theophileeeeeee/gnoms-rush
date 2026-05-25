using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FlecheMovement : MonoBehaviour
{
    private Transform cible;
    public float damage = 10f;
    private float vitesse;

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

    public void Init(Transform cible, float vitesse)
    {
        this.cible = cible;
        this.vitesse = vitesse;
    }

    void Update()
    {
        if (aTouche) return;

        if (cible == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = Vector2.MoveTowards(
            transform.position,
            cible.position,
            vitesse * Time.deltaTime
        );

        Vector2 direction = (cible.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle + 180);

        if (Vector2.Distance(transform.position, cible.position) < 0.1f)
        {
            ExplodeArrow();
        }
    }

void ExplodeArrow()
{
    aTouche = true;

    EnemyMovement em = cible.GetComponent<EnemyMovement>();
    if (em != null) em.TakeDamage(damage);
    else { FlyingEnemyMovement fem = cible.GetComponent<FlyingEnemyMovement>(); if (fem != null) fem.TakeDamage(damage); }

    float delay = 0.1f;
    if (hitSound != null) { audioSource.PlayOneShot(hitSound, volume); delay = hitSound.length; }

    var renderer = GetComponent<Renderer>();
    if (renderer != null) renderer.enabled = false;
    var collider = GetComponent<Collider2D>();
    if (collider != null) collider.enabled = false;
    foreach (Transform child in transform) child.gameObject.SetActive(false);
    Destroy(gameObject, delay);
}
}