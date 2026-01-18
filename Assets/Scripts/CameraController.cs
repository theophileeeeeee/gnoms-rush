using UnityEngine;

public class CameraController2D : MonoBehaviour
{
    [Header("Références")]
    public BoxCollider2D levelBounds;

    [Header("Déplacement")]
    public float dragSpeed = 1.2f;
    public float smoothTime = 0.15f;

    [Header("Zoom")]
    public float minZoom = 3f;
    public float maxZoom = 8f;

    private Camera cam;

    private Vector3 targetPosition;
    private Vector3 velocity;

    private Vector2 lastTouchPos;

    void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
        targetPosition = transform.position;
    }

    void Update()
    {
        HandleDrag();
        HandlePinchZoom();

        ClampTargetPosition();

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothTime
        );
    }
    void HandleDrag()
    {
        if (Input.touchCount != 1) return;

        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
            lastTouchPos = touch.position;

        if (touch.phase == TouchPhase.Moved)
        {
            Vector2 delta = touch.position - lastTouchPos;

            Vector3 move = new Vector3(
                -delta.x,
                -delta.y,
                0f
            ) * dragSpeed * cam.orthographicSize / Screen.dpi;

            targetPosition += move;
            lastTouchPos = touch.position;
        }
    }
    void HandlePinchZoom()
    {
        if (Input.touchCount != 2) return;

        Touch t0 = Input.GetTouch(0);
        Touch t1 = Input.GetTouch(1);

        Vector2 t0Prev = t0.position - t0.deltaPosition;
        Vector2 t1Prev = t1.position - t1.deltaPosition;

        float prevDist = Vector2.Distance(t0Prev, t1Prev);
        float currDist = Vector2.Distance(t0.position, t1.position);

        float delta = prevDist - currDist;

        cam.orthographicSize += delta * 0.004f;
        cam.orthographicSize = Mathf.Clamp(
            cam.orthographicSize,
            minZoom,
            GetMaxAllowedZoom()
        );
    }
    void ClampTargetPosition()
    {
        Bounds bounds = levelBounds.bounds;

        float vertExtent = cam.orthographicSize;
        float horzExtent = vertExtent * cam.aspect;

        float minX = bounds.min.x + horzExtent;
        float maxX = bounds.max.x - horzExtent;
        float minY = bounds.min.y + vertExtent;
        float maxY = bounds.max.y - vertExtent;

        targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);
        targetPosition.z = transform.position.z;
    }
    float GetMaxAllowedZoom()
    {
        Bounds bounds = levelBounds.bounds;

        float maxZoomY = bounds.size.y * 0.5f;
        float maxZoomX = bounds.size.x / (2f * cam.aspect);

        return Mathf.Min(maxZoom, maxZoomX, maxZoomY);
    }
}
