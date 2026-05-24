using System.Collections;
using UnityEngine;

public class KnightManager : MonoBehaviour
{
    public Animator animator;
    public GameObject healthBar;
    public float maxHealth = 10f;
    public float currentHealth;
    public AudioSource audioSource;
    public AudioClip[] swordSounds;

    private Transform homePoint;

    public float detectionRadius = 5f;
    public float attackRadius = 1.2f;

    public float speed = 3f;

    public float damage = 2f;
    public float attackCooldown = 1f;

    public float idleTimeBeforeRegen = 5f;
    public float regenPerSecond = 1f;

    private float attackTimer;
    private float idleTimer;
    private bool isRegening;

    public EnemyMovement target;
    private Vector2 attackPosition;
    private float lockedSide = 1f;
    private SpriteRenderer spriteRenderer;

    private enum KnightState { Idle, MovingToHome, Combat }
    private KnightState state = KnightState.Idle;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        if (homePoint != null)
            transform.position = homePoint.position;

        currentHealth = maxHealth;
        attackTimer = attackCooldown;
        idleTimer = 0f;
        isRegening = false;

        if (homePoint != null)
            state = KnightState.MovingToHome;
    }

    void Update()
    {
        if (currentHealth <= 0) return;

        switch (state)
        {
            case KnightState.MovingToHome: ReturnToHome(); break;
            case KnightState.Combat:       HandleCombat(); break;
            case KnightState.Idle:         SearchForEnemy(); break;
        }

        HandleRegen();
    }

    void HandleRegen()
    {
        bool inCombat = target != null;

        if (inCombat || currentHealth >= maxHealth)
        {
            idleTimer = 0f;
            isRegening = false;
            return;
        }

        idleTimer += Time.deltaTime;

        if (idleTimer >= idleTimeBeforeRegen)
            isRegening = true;

        if (isRegening)
            currentHealth = Mathf.Min(currentHealth + regenPerSecond * Time.deltaTime, maxHealth);
    }

    public void SetHomePoint(Transform point)
    {
        homePoint = point;
        if (state == KnightState.Idle)
            state = KnightState.MovingToHome;
    }

    public void EngageBy(EnemyMovement enemy)
    {
        if (state == KnightState.Combat && target != null) return;
        LockEnemy(enemy);
    }

    void ReturnToHome()
    {
        float distance = Vector2.Distance(transform.position, homePoint.position);

        if (distance > 0.05f)
        {
            animator.SetBool("isWalking", true);

            float dx = homePoint.position.x - transform.position.x;
            if (dx != 0f)
                spriteRenderer.flipX = dx > 0f;

            transform.position = Vector2.MoveTowards(
                transform.position,
                homePoint.position,
                speed * Time.deltaTime
            );
        }
        else
        {
            animator.SetBool("isWalking", false);
            state = KnightState.Idle;
        }
    }

    void SearchForEnemy()
    {
        EnemyMovement[] enemies = FindObjectsByType<EnemyMovement>(FindObjectsSortMode.None);

        float closestDistance = detectionRadius;
        EnemyMovement closestEnemy = null;

        foreach (EnemyMovement enemy in enemies)
        {
            if (enemy.isEngaged && enemy.opponent != this.transform)
                continue;

            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy;
            }
        }

        if (closestEnemy != null)
            LockEnemy(closestEnemy);
    }

    void LockEnemy(EnemyMovement enemy)
    {
        target = enemy;
        state = KnightState.Combat;

        lockedSide = Mathf.Sign(transform.position.x - enemy.transform.position.x);
        if (lockedSide == 0f) lockedSide = 1f;

        attackPosition = new Vector2(
            enemy.transform.position.x + lockedSide * attackRadius,
            enemy.transform.position.y
        );

        enemy.isEngaged = true;
        enemy.opponent = transform;
        enemy.StopMovement();
    }

    void HandleCombat()
    {
        if (target == null) { Unlock(); return; }

        attackPosition = new Vector2(
            target.transform.position.x + lockedSide * attackRadius,
            target.transform.position.y
        );

        float distance = Vector2.Distance(transform.position, attackPosition);

        if (distance > 0.05f)
        {
            animator.SetBool("isWalking", true);

            float dx = attackPosition.x - transform.position.x;
            if (dx != 0f)
                spriteRenderer.flipX = dx > 0f;

            transform.position = Vector2.MoveTowards(
                transform.position,
                attackPosition,
                speed * Time.deltaTime
            );
        }
        else
        {
            animator.SetBool("isWalking", false);

            float dx = target.transform.position.x - transform.position.x;
            if (dx != 0f)
                spriteRenderer.flipX = dx > 0f;

            Attack();
        }
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
        if (target != null)
            target.TakeDamage(damage);

        if (audioSource != null && swordSounds != null && swordSounds.Length > 0)
        {
            audioSource.enabled = true;
            audioSource.PlayOneShot(swordSounds[Random.Range(0, swordSounds.Length)]);
        }
    }

    public void TakeDamage(float dmg)
    {
        if (currentHealth <= 0) return;
        currentHealth -= dmg;
        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        animator.SetBool("isWalking", false);
        GetComponent<Renderer>().enabled = false;

        if (target != null)
        {
            target.isEngaged = false;
            target.opponent = null;
            target.ResumeMovement();
        }

        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        healthBar.SetActive(false);
        yield return new WaitForSeconds(20f);
        Destroy(gameObject);
    }

    public void Unlock()
    {
        if (target != null)
        {
            target.isEngaged = false;
            target.opponent = null;
            target.ResumeMovement();
        }

        target = null;
        attackTimer = attackCooldown;
        animator.SetBool("isWalking", false);

        if (homePoint != null)
            state = KnightState.MovingToHome;
        else
            state = KnightState.Idle;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}