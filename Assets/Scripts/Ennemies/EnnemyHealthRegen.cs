using UnityEngine;

public class EnemyHealthRegen : MonoBehaviour
{
    public float regenPercent = 10f;
    public float regenInterval = 5f;
    public GameObject regenVFXPrefab;
    public Transform spawnPoint;
    public float vfxDuration = 2f;

    private EnemyMovement enemyMovement;
    private float timer;

    void Start()
    {
        enemyMovement = GetComponent<EnemyMovement>();
        timer = regenInterval;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            float healAmount = enemyMovement.maxHealth * (regenPercent / 100f);
            enemyMovement.currentHealth = Mathf.Min(enemyMovement.currentHealth + healAmount, enemyMovement.maxHealth);

            if (regenVFXPrefab != null && spawnPoint != null)
            {
                GameObject vfx = Instantiate(regenVFXPrefab, spawnPoint.position, spawnPoint.rotation, spawnPoint);
                Destroy(vfx, vfxDuration);
            }

            timer = regenInterval;
        }
    }
}