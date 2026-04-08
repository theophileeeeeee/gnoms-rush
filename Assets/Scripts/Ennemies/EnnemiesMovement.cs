using UnityEngine;
using System.Collections.Generic;

public class EnemyMovement : MonoBehaviour
{
    public float speed = 1f;
    public int laneCount = 3;
    public float laneSpacing = 0.35f;
    private UIManager uiManager;

    float distanceTravelled = 0f;
    float laneOffset = 0f;

    public float maxHealth = 10f;
    public float currentHealth;
    public float damage = 2f;

    public bool isEngaged = false;
    private bool movementFrozen = false;

    public Animator animator;
    public Transform opponent;

    public float detectionRange = 0.8f;
    public LayerMask knightLayer;

    public float attackCooldown = 1f;
    private float attackTimer;

    [HideInInspector] public List<Path> allowedPaths;
    Path currentPath;
    private SpriteRenderer spriteRenderer;

    [System.Obsolete]
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        uiManager = FindObjectOfType<UIManager>();
        if (uiManager == null)
            Debug.LogError("UIManager introuvable dans la scène !");

        if (allowedPaths != null && allowedPaths.Count > 0)
            currentPath = allowedPaths[Random.Range(0, allowedPaths.Count)];

        if (currentPath == null)
        {
            Debug.LogError("EnemyMovement: aucun Path assigné !");
            enabled = false;
            return;
        }

        currentHealth = maxHealth;
        attackTimer = attackCooldown;
        animator.SetBool("Walk", true);
        animator.SetBool("Idle", false);
    }

    public void StopMovement()   => movementFrozen = true;
    public void ResumeMovement() => movementFrozen = false;

    public void TakeDamage(float dmg)
    {
        currentHealth -= dmg;
        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        uiManager.EarnMoney(20);
        animator.ResetTrigger("Attack");
        if (opponent != null)
        {
            KnightManager knight = opponent.GetComponent<KnightManager>();
            if (knight != null)
                knight.Unlock();
        }
        Destroy(gameObject);
    }

    void Attack()
    {
        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f)
        {
            animator.SetTrigger("Attack");
            attackTimer = attackCooldown;
        }
    }

    public void DealDamage()
    {
        if (opponent != null)
        {
            KnightManager knight = opponent.GetComponent<KnightManager>();
            if (knight != null)
                knight.TakeDamage(damage);
        }
    }

    void TryDetectKnight()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRange, knightLayer);
        float closestDist = float.MaxValue;
        KnightManager closestKnight = null;

        foreach (var hit in hits)
        {
            KnightManager km = hit.GetComponent<KnightManager>();
            if (km == null) continue;

            float d = Vector2.Distance(transform.position, hit.transform.position);
            if (d < closestDist)
            {
                closestDist = d;
                closestKnight = km;
            }
        }

        if (closestKnight != null)
            closestKnight.EngageBy(this);
    }

    void Update()
    {
        if (opponent == null && isEngaged)
{
    isEngaged = false;
    movementFrozen = false;
    animator.ResetTrigger("Attack");
    animator.SetBool("Idle", false);
    animator.SetBool("Walk", false);  // ← false au lieu de true
}

    if (!isEngaged && !movementFrozen)
    animator.SetBool("Walk", true);

        if (!isEngaged)
            TryDetectKnight();

        if (isEngaged)
        {
            bool inRange = opponent != null && Vector2.Distance(transform.position, opponent.position) <= detectionRange;

            if (inRange)
            {
                animator.SetBool("Idle", false);
                animator.SetBool("Walk", false);
                Attack();
            }
            else
            {
                animator.SetBool("Idle", true);
                animator.SetBool("Walk", false);
            }

            if (opponent != null)
            {
                float dx = opponent.position.x - transform.position.x;
                if (dx != 0f)
                    spriteRenderer.flipX = dx < 0f;
            }
            return;
        }

        if (movementFrozen) return;

        distanceTravelled += speed * Time.deltaTime;

        Vector2 center = currentPath.GetPointAtDistance(distanceTravelled);
        Vector2 dir = currentPath.GetDirectionAtDistance(distanceTravelled);
        Vector2 perp = new Vector2(-dir.y, dir.x);

        if (dir.x != 0f)
            spriteRenderer.flipX = dir.x < 0f;

        transform.position = center + perp * laneOffset;
    }

    public void AssignLane(int laneIndex)
    {
        laneOffset = (laneIndex - (laneCount - 1) / 2f) * laneSpacing;
    }

    public void SetAllowedPaths(List<Path> paths) => allowedPaths = paths;
    public void SetSpawnPosition(Vector2 pos) => transform.position = pos;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}