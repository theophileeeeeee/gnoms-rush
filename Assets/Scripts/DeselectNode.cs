using UnityEngine;

public class DeselectOnClick : MonoBehaviour
{
    void Update()
    {
        // PC
        if (Input.GetMouseButtonDown(0))
        {
            TryDeselect(Input.mousePosition);
        }

        // MOBILE
        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            TryDeselect(Input.GetTouch(0).position);
        }
    }

    void TryDeselect(Vector2 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        // Si on ne touche PAS un Node
        if (hit.collider == null || hit.collider.GetComponent<Node>() == null)
        {
            BuildManager.instance.DeselectNode();
        }
    }
}
