using System.Collections.Generic; // Obligatoire pour la liste anti-bug
using UnityEngine;

public class Bomb2D : MonoBehaviour
{
    private Transform target;
    private Vector2 lastKnownPosition;

    [Header("Paramètres de déplacement")]
    public float speed = 15f;

    [Header("Paramètres d'explosion")]
    public float explosionRadius = 5f;
    public string enemyTag = "Ennemy"; // Ton tag avec deux "n"
    public float damage = 2f;          // Tes dégâts de base (comme sur ton Bullet)
    public float animationDuration = 1f; // Temps avant de détruire l'objet (durée du BOOM)

    private Animator anim;
    private bool hasExploded = false;
    private bool isFlying = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        if (anim == null)
        {
            Debug.LogWarning("Attention : Aucun composant Animator trouvé sur la bombe 2D !");
        }
    }

    // Appelée par la tour après les 0.3s d'attente
    public void Seek(Transform _target)
    {
        target = _target;
        isFlying = true;
    }

    void Update()
    {
        if (!isFlying || hasExploded) return;

        if (target != null)
        {
            lastKnownPosition = target.position;
        }

        Vector2 currentPos = transform.position;
        Vector2 dir = lastKnownPosition - currentPos;
        float distanceThisFrame = speed * Time.deltaTime;

        // Si la bombe arrive à destination
        if (dir.magnitude <= distanceThisFrame)
        {
            Explode();
            return;
        }

        // Déplacement vers la cible
        transform.Translate(dir.normalized * distanceThisFrame, Space.World);
        
        if (dir != Vector2.zero)
        {
            transform.up = dir.normalized; 
        }
    }

    void Explode()
    {
        hasExploded = true;

        // 1. Déclenche l'animation de la bombe (le Trigger BOOM)
        if (anim != null)
        {
            anim.SetTrigger("BOOM");
        }

        // 2. Liste de sécurité pour ne pas frapper deux fois le même ennemi s'il a plusieurs colliders
        List<GameObject> enemiesDamagedThisExplosion = new List<GameObject>();

        // Trouve absolument TOUS les colliders dans la zone de la bombe
        Collider2D[] collidersInZone = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        
        foreach (Collider2D col in collidersInZone)
        {
            if (col.CompareTag(enemyTag))
            {
                GameObject enemyGO = col.gameObject;

                // Si on n'a pas encore touché cet ennemi précis pendant cette explosion
                if (!enemiesDamagedThisExplosion.Contains(enemyGO))
                {
                    // On l'enregistre pour bloquer ses autres colliders s'il en a
                    enemiesDamagedThisExplosion.Add(enemyGO);

                    // On va chercher ton script de dégâts sur l'ennemi (comme dans ton Bullet)
                    EnemyMovement enemy = enemyGO.GetComponent<EnemyMovement>();
                    if (enemy != null)
                    {
                        enemy.TakeDamage(damage); // Il prend les dégâts proprement !
                    }
                }
            }
        }

        // 3. Destruction de la bombe après le délai de l'animation
        Destroy(gameObject, animationDuration);
    }

    // Dessine le rayon de l'explosion en rouge dans l'éditeur de Unity quand tu cliques sur la bombe
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}