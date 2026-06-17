using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlyingEnemyMovement : MonoBehaviour
{
    public float speed = 1.5f;
    public int reward = 20;
    public int laneCount = 3;
    public float laneSpacing = 0.35f;
    private UIManager uiManager;

    float distanceTravelled = 0f;
    float laneOffset = 0f;

    public float maxHealth = 6f;
    public float currentHealth;

    public bool isExplosive = false;
    public float explosionRadius = 1f;
    public float explosionDamage = 5f;
    public GameObject explosionVFX;

    private bool movementFrozen = false;
    private bool isFrozen = false;

    public Animator animator;

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
            Debug.LogError("FlyingEnemyMovement: aucun Path assigné !");
            enabled = false;
            return;
        }

        if (PlayerPrefs.GetInt("HardcoreMode", 0) == 1)
        {
            maxHealth *= 1.5f;
        }

        currentHealth = maxHealth;
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
        uiManager.EarnMoney(reward);
        PlayerPrefs.SetInt("EnemiesKilled", PlayerPrefs.GetInt("EnemiesKilled", 0) + 1);
        if (isExplosive)
            Explode();
        Destroy(gameObject);
    }

    void Explode()
    {
        Debug.Log("FlyingEnemy Explode() appelé à : " + transform.position);

        if (explosionVFX != null)
        {
            GameObject vfx = Instantiate(explosionVFX, transform.position, Quaternion.identity);
            Destroy(vfx, 0.67f);
            Debug.Log("VFX instancié !");
        }
        else
        {
            Debug.LogWarning("explosionVFX est null !");
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        Debug.Log("Colliders détectés : " + hits.Length);

        foreach (var hit in hits)
        {
            Debug.Log("Collider : " + hit.gameObject.name + " | Layer : " + LayerMask.LayerToName(hit.gameObject.layer));
            KnightManager knight = hit.GetComponent<KnightManager>();
            if (knight != null)
            {
                Debug.Log("KnightManager trouvé → " + explosionDamage + " dégâts sur " + hit.gameObject.name);
                knight.TakeDamage(explosionDamage);
            }
            else
            {
                Debug.Log("Pas de KnightManager sur : " + hit.gameObject.name);
            }
        }

        Debug.Log("Explode() terminé");
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

        animator.SetBool("Walk", false);
        animator.SetBool("Idle", true);

        Color frozenColor = new Color(1f - blueIntensity, 1f - blueIntensity, 1f);
        spriteRenderer.color = frozenColor;

        yield return new WaitForSeconds(duration);

        isFrozen = false;
        movementFrozen = false;
        spriteRenderer.color = Color.white;

        animator.SetBool("Idle", false);
        animator.SetBool("Walk", true);
    }

    void Update()
    {
        if (movementFrozen)
        {
            animator.SetBool("Walk", false);
            animator.SetBool("Idle", true);
            return;
        }

        animator.SetBool("Walk", true);
        animator.SetBool("Idle", false);

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
        if (isExplosive)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.15f);
            Gizmos.DrawSphere(transform.position, explosionRadius);
        }
    }
}