using UnityEngine;

public class KnightManager : MonoBehaviour
{
    public Transform startPosition;

    [Header("Détection")]
    public float detectionRadius = 5f;
    public float attackRadius = 1.2f;

    [Header("Déplacement")]
    public float speed = 3f;
    public Transform startingPoint;

    [Header("Combat")]
    public float damage = 2f;
    public float attackCooldown = 1f;

    private float attackTimer;
    private EnemyMovement target;
    private bool isEngaged = false;

    void Start()
    {
        if (startPosition != null)
            transform.position = startPosition.position;
    }

    void Update()
    {
        if (!isEngaged)
        {
            SearchForEnemy();
        }
        else
        {
            HandleCombat();
        }
    }

    void SearchForEnemy()
    {
        EnemyMovement[] enemies = FindObjectsByType<EnemyMovement>(FindObjectsSortMode.None);

        float closestDistance = detectionRadius;
        EnemyMovement closestEnemy = null;

        foreach (EnemyMovement enemy in enemies)
        {
            if (enemy.isEngaged)
                continue;

            float distance = Vector2.Distance(transform.position, enemy.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy;
            }
        }

        if (closestEnemy != null)
        {
            LockEnemy(closestEnemy);
        }
    }

    void LockEnemy(EnemyMovement enemy)
    {
        target = enemy;
        isEngaged = true;

        enemy.isEngaged = true;
        enemy.opponent = transform;
    }

    void HandleCombat()
    {
        if (target == null)
            return;

        float distance = Vector2.Distance(transform.position, target.transform.position);

        if (distance > attackRadius)
        {
            Vector2 direction = (target.transform.position - transform.position).normalized;
            transform.position += (Vector3)(direction * speed * Time.deltaTime);
        }
        else
        {
            Attack();
        }
    }

    void Attack()
    {
        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0f)
        {
            target.TakeDamage(damage);
            attackTimer = attackCooldown;
        }
    }

    public void Unlock()
    {
        target = null;
        isEngaged = false;
        attackTimer = 0f;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}
