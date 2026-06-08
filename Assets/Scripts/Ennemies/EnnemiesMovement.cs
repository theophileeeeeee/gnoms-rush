using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyMovement : MonoBehaviour
{
    public float speed = 1f;
    public bool isABoss = false;
    public bool isExplosive = false;
    public int laneCount = 3;
    public float laneSpacing = 0.35f;
    private UIManager uiManager;

    public float explosionRadius = 1f;
    public float explosionDamage = 5f;
    public GameObject explosionVFX;

    float distanceTravelled = 0f;
    float laneOffset = 0f;

    public float maxHealth = 10f;
    public int moneyReward = 20;
    public float currentHealth;
    public float damage = 2f;

    public bool isEngaged = false;
    private bool movementFrozen = false;
    private bool isFrozen = false;

    public Animator animator;
    public Transform opponent;

    public float detectionRange = 0.8f;
    public LayerMask knightLayer;

    public float attackCooldown = 1f;
    private float attackTimer;

    [HideInInspector] public List<Path> allowedPaths;
    Path currentPath;
    private SpriteRenderer spriteRenderer;

    bool initialized = false;

    [System.Obsolete]
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        uiManager = FindObjectOfType<UIManager>();
        if (uiManager == null)
            Debug.LogError("UIManager introuvable dans la scène !");

        currentHealth = maxHealth;
        attackTimer = attackCooldown;
    }

    void Init()
    {
        if (initialized) return;
        if (allowedPaths == null || allowedPaths.Count == 0) return;

        currentPath = allowedPaths[Random.Range(0, allowedPaths.Count)];
        initialized = true;

        animator.SetBool("Walk", true);
        animator.SetBool("Idle", false);
    }

    public void StopMovement() => movementFrozen = true;
    public void ResumeMovement() => movementFrozen = false;

    public void TakeDamage(float dmg)
    {
        currentHealth -= dmg;
        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        uiManager.EarnMoney(moneyReward);
        PlayerPrefs.SetInt("EnemiesKilled", PlayerPrefs.GetInt("EnemiesKilled", 0) + 1);
        animator.ResetTrigger("Attack");
        animator.speed = 1f;
        if (opponent != null)
        {
            KnightManager knight = opponent.GetComponent<KnightManager>();
            if (knight != null)
                knight.Unlock();
        }
        if (isExplosive)
            Explode();
        Destroy(gameObject);
    }

void Explode()
{
    if (explosionVFX != null)
    {
        GameObject vfx = Instantiate(explosionVFX, transform.position, Quaternion.identity);
        Destroy(vfx, 0.67f);
    }

    Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
    foreach (var hit in hits)
    {
        KnightManager knight = hit.GetComponent<KnightManager>();
        if (knight != null)
            knight.TakeDamage(explosionDamage);
    }
}

    void Attack()
    {
        if (isFrozen) return;

        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f)
        {
            animator.SetTrigger("Attack");
            attackTimer = attackCooldown;
        }
    }

    public void DealDamage()
    {
        if (isFrozen) return;

        if (opponent != null)
        {
            KnightManager knight = opponent.GetComponent<KnightManager>();
            if (knight != null)
                knight.TakeDamage(damage);
        }
    }

    public void GetFrozen(float duration, float blueIntensity = 0.5f)
    {
        if (isFrozen)
            StopCoroutine(nameof(FreezeCoroutine));
        StartCoroutine(FreezeCoroutine(duration, blueIntensity));
    }

    private IEnumerator FreezeCoroutine(float duration, float blueIntensity)
    {
        isFrozen = true;
        movementFrozen = true;

        animator.speed = 0f;

        Color frozenColor = new Color(1f - blueIntensity, 1f - blueIntensity, 1f);
        spriteRenderer.color = frozenColor;

        yield return new WaitForSeconds(duration);

        isFrozen = false;
        movementFrozen = false;
        animator.speed = 1f;

        spriteRenderer.color = Color.white;

        if (!isEngaged)
        {
            animator.SetBool("Idle", false);
            animator.SetBool("Walk", true);
        }
        else
        {
            animator.SetBool("Idle", false);
            animator.SetBool("Walk", false);
        }
    }

    void TryDetectKnight()
    {
        if (isFrozen) return;

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
        Init();
        if (!initialized) return;

        if (opponent == null && isEngaged)
        {
            isEngaged = false;
            movementFrozen = false;
            animator.ResetTrigger("Attack");
            animator.SetBool("Idle", false);
            animator.SetBool("Walk", false);
        }

        if (isFrozen) return;

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

        if (movementFrozen)
        {
            animator.SetBool("Idle", true);
            return;
        }

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

        if (isExplosive)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.15f);
            Gizmos.DrawSphere(transform.position, explosionRadius);
        }
    }
}