using UnityEngine;
using System.Collections.Generic;

public class EnemyMovement : MonoBehaviour
{
    public float speed = 1f;
    public int laneCount = 3;
    public float laneSpacing = 0.35f;

    float distanceTravelled = 0f;
    float laneOffset = 0f;

    public float maxHealth = 10f;

    public float currentHealth;

    public bool isEngaged = false;

    public Transform opponent;

    [HideInInspector] public List<Path> allowedPaths; 
    Path currentPath;

    void Start()
    {
        if (allowedPaths != null && allowedPaths.Count > 0)
        {
            currentPath = allowedPaths[Random.Range(0, allowedPaths.Count)];
        }

        if (currentPath == null)
        {
            Debug.LogError("EnemyMovement: aucun Path assigné !");
            enabled = false;
            return;
        }
        currentHealth = maxHealth;
    }
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    void Die()
    {
        if (opponent != null)
        {
            KnightManager knight = opponent.GetComponent<KnightManager>();
            if (knight != null)
            {
                knight.Unlock();
            }
        }

        Destroy(gameObject);
    }

    void Update()
    {
        if (isEngaged)
            return;
        distanceTravelled += speed * Time.deltaTime;

        Vector2 center = currentPath.GetPointAtDistance(distanceTravelled);
        Vector2 dir = currentPath.GetDirectionAtDistance(distanceTravelled);
        Vector2 perp = new Vector2(-dir.y, dir.x);

        transform.position = center + perp * laneOffset;
    }
    public void AssignLane(int laneIndex)
    {
        laneOffset = (laneIndex - (laneCount - 1) / 2f) * laneSpacing;
    }
    public void SetAllowedPaths(List<Path> paths)
    {
        allowedPaths = paths;
    }
    public void SetSpawnPosition(Vector2 pos)
    {
        transform.position = pos;
    }
}
