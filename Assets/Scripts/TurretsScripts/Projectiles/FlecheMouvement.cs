using UnityEngine;

public class FlecheMovement : MonoBehaviour
{
    private Transform cible;
    public float damage = 10f;
    private float vitesse;

    [Header("Audio")]
    public AudioClip hitSound;
    public float volume = 1f;

    public void Init(Transform cible, float vitesse)
    {
        this.cible = cible;
        this.vitesse = vitesse;
    }

    void Update()
    {
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
            if (hitSound != null)
                AudioSource.PlayClipAtPoint(hitSound, transform.position, volume);

            cible.GetComponent<EnemyMovement>().TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}