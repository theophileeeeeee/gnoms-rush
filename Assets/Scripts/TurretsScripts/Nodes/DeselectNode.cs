using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class DeselectOnClick : MonoBehaviour
{
    public GameObject gameOverPanel;
    public int time = 1;

    void Start()
    {
        Time.timeScale = time;
    }

    void Update()
    {
        if (gameOverPanel.activeSelf) return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            TryDeselect(Pointer.current.position.ReadValue());
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