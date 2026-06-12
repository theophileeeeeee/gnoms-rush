using UnityEngine;

public class SpriteSorter : MonoBehaviour
{
    public int baseOrder = 5000;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        if (spriteRenderer != null)
        {
            int yOrder = Mathf.RoundToInt(transform.position.y * 1000);
            int uniqueIdOffset = Mathf.Abs(gameObject.GetInstanceID()) % 10;
            
            spriteRenderer.sortingOrder = baseOrder - yOrder + uniqueIdOffset;
        }
    }
}