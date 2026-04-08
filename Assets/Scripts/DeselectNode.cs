using UnityEngine;

public class DeselectOnClick : MonoBehaviour
{
    public GameObject gameOverPanel;
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !gameOverPanel.activeSelf)
        {
            TryDeselect(Input.mousePosition);
        }

        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began&& !gameOverPanel.activeSelf)
        {
            TryDeselect(Input.GetTouch(0).position);
        }
    }

    void TryDeselect(Vector2 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
        if (hit.collider == null || hit.collider.GetComponent<Node>() == null)
        {
            BuildManager.instance.DeselectNode();
        }
    }
}
