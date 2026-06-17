using UnityEngine;

public class SpriteSorter : MonoBehaviour
{
    public int baseOrder = 2000;
    public float yMultiplier = 100f;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        if (spriteRenderer != null)
        {
            int yOrder = Mathf.RoundToInt(transform.position.y * yMultiplier);
            spriteRenderer.sortingOrder = baseOrder - yOrder;
        }
    }
}