using System.Collections.Generic;
using UnityEngine;

public class Bomb2D : MonoBehaviour
{
    private Transform target;
    private Vector2 lastKnownPosition;

    [Header("Paramètres de déplacement")]
    public float speed = 15f;

    [Header("Paramètres d'explosion")]
    public float explosionRadius = 5f;
    public string enemyTag = "Ennemy";
    public float damage = 2f;
    public float animationDuration = 1f;

    [Header("Audio")]
    public AudioClip explosionSound;
    public float volume = 1f;

    private Animator anim;
    private bool hasExploded = false;
    private bool isFlying = false;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void Seek(Transform _target)
    {
        target = _target;
        isFlying = true;
    }

    void Update()
    {
        if (!isFlying || hasExploded) return;

        if (target != null)
            lastKnownPosition = target.position;

        Vector2 currentPos = transform.position;
        Vector2 dir = lastKnownPosition - currentPos;
        float distanceThisFrame = speed * Time.deltaTime;

        if (dir.magnitude <= distanceThisFrame)
        {
            Explode();
            return;
        }

        transform.Translate(dir.normalized * distanceThisFrame, Space.World);

        if (dir != Vector2.zero)
            transform.up = dir.normalized;
    }

    void Explode()
    {
        hasExploded = true;

        if (anim != null)
            anim.SetTrigger("BOOM");

        if (explosionSound != null)
            AudioSource.PlayClipAtPoint(explosionSound, transform.position, volume);

        List<GameObject> enemiesDamagedThisExplosion = new List<GameObject>();
        Collider2D[] collidersInZone = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D col in collidersInZone)
        {
            if (col.CompareTag(enemyTag))
            {
                GameObject enemyGO = col.gameObject;
                if (!enemiesDamagedThisExplosion.Contains(enemyGO))
                {
                    enemiesDamagedThisExplosion.Add(enemyGO);
                    EnemyMovement enemy = enemyGO.GetComponent<EnemyMovement>();
                    if (enemy != null)
                        enemy.TakeDamage(damage);
                }
            }
        }

        Destroy(gameObject, animationDuration);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}