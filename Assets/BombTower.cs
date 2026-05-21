using System.Collections;
using UnityEngine;

public class BombTower2D : MonoBehaviour
{
    public Transform target;
    public float range = 15f;
    public string enemyTag = "Ennemy";

    [Header("Configuration Temporelle")]
    public float rechargeTime = 2f;      // Temps de rechargement à vide
    public float visualDelay = 0.3f;     // Temps de pause de la bombe avant le tir
    
    private float fireCountdown = 0f;
    private bool isShooting = false;     // Sécurité pour éviter de lancer la pause en boucle

    [Header("Configuration Préfabs")]
    public GameObject bombPrefab;
    public Transform firePoint;

    private GameObject preparedBomb;

    void Start()
    {
        InvokeRepeating("UpdateTarget", 0f, 0.2f);
    }

    void UpdateTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null || !enemy.activeInHierarchy) continue;

            float distanceToEnemy = Vector2.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy != null && shortestDistance <= range)
        {
            target = nearestEnemy.transform;
        }
        else
        {
            target = null;
        }
    }

    void Update()
    {
        // 1. Le temps de recharge s'écoule en arrière-plan
        if (fireCountdown > 0f)
        {
            fireCountdown -= Time.deltaTime;
        }

        // 2. Si le chrono est fini et qu'on n'est pas déjà en train de tirer -> On recharge
        if (fireCountdown <= 0f && preparedBomb == null && !isShooting)
        {
            PrepareNextBomb();
        }

        // 3. Si un ennemi est là, qu'une bombe est prête et qu'on n'a pas encore initié le tir
        if (target != null && preparedBomb != null && !isShooting)
        {
            // On lance la séquence de tir avec le délai visuel de 0.3s
            StartCoroutine(ShootWithDelay());
        }
    }

    void PrepareNextBomb()
    {
        preparedBomb = Instantiate(bombPrefab, firePoint.position, firePoint.rotation, firePoint);
    }

    // Coroutine pour gérer l'attente de 0.3s avant le départ de la bombe
    IEnumerator ShootWithDelay()
    {
        isShooting = true;

        // La bombe reste statique accrochée à la tour pendant 0.3 seconde
        yield return new WaitForSeconds(visualDelay);

        // Sécurité : On vérifie si la cible n'est pas morte ou sortie de la zone pendant les 0.3s
        if (target != null && preparedBomb != null)
        {
            // Détachement et propulsion de la bombe
            preparedBomb.transform.SetParent(null);
            Bomb2D bomb = preparedBomb.GetComponent<Bomb2D>();
            if (bomb != null)
            {
                bomb.Seek(target);
            }
            preparedBomb = null;
        }
        else if (preparedBomb != null)
        {
            // Si l'ennemi a disparu pendant les 0.3s, on garde la bombe sur la tour pour le prochain
            // On ne fait rien, elle reste en position "armée"
        }

        // On enclenche le temps de recharge de 2 secondes
        fireCountdown = rechargeTime;
        isShooting = false;
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}