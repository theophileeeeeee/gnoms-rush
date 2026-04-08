using UnityEngine;

[RequireComponent(typeof(Camera))]
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
    public float zoomSensitivity = 1.0f;
    public float zoomSmoothTime = 0.1f;

    private Camera cam;
    private Vector3 targetPosition;
    private Vector3 velocityPos;
    private float targetZoom;
    private float velocityZoom;

    private Vector2 lastTouchPos;

    void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
        targetPosition = transform.position;
        targetZoom = cam.orthographicSize;
    }

    void Update()
    {
        HandleDrag();
        HandleZoom();

        ClampTargetPosition();
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocityPos,
            smoothTime
        );
        cam.orthographicSize = Mathf.SmoothDamp(
            cam.orthographicSize,
            targetZoom,
            ref velocityZoom,
            zoomSmoothTime
        );
    }

    void HandleDrag()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began) lastTouchPos = touch.position;
            if (touch.phase == TouchPhase.Moved)
            {
                ApplyMove(touch.position - lastTouchPos);
                lastTouchPos = touch.position;
            }
        }
        else if (Input.GetMouseButton(0) || Input.GetMouseButton(2))
        {
            Vector2 mousePos = Input.mousePosition;
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(2)) lastTouchPos = mousePos;
            
            ApplyMove(mousePos - lastTouchPos);
            lastTouchPos = mousePos;
        }
    }

    void ApplyMove(Vector2 delta)
    {
        Vector3 move = new Vector3(-delta.x, -delta.y, 0f) 
                       * dragSpeed 
                       * cam.orthographicSize 
                       / Screen.height;

        targetPosition += move;
    }

void HandleZoom()
{
    float zoomDelta = 0f;

    if (Input.touchCount == 2)
    {
        Touch t0 = Input.GetTouch(0);
        Touch t1 = Input.GetTouch(1);

        Vector2 t0Prev = t0.position - t0.deltaPosition;
        Vector2 t1Prev = t1.position - t1.deltaPosition;

        float prevDist = Vector2.Distance(t0Prev, t1Prev);
        float currDist = Vector2.Distance(t0.position, t1.position);

        zoomDelta = (prevDist - currDist) * 0.01f;
    }
    else
    {
        // Utilise mouseScrollDelta.y au lieu de GetAxis, plus fiable
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.001f)
        {
            zoomDelta = -scroll * zoomSensitivity;
        }
    }

    if (Mathf.Abs(zoomDelta) > 0.0001f)
    {
        targetZoom += zoomDelta;
        targetZoom = Mathf.Clamp(targetZoom, minZoom, GetMaxAllowedZoom());
    }
}

    void ClampTargetPosition()
    {
        if (levelBounds == null) return;

        Bounds bounds = levelBounds.bounds;

        float vertExtent = targetZoom;
        float horzExtent = vertExtent * cam.aspect;

        float minX = bounds.min.x + horzExtent;
        float maxX = bounds.max.x - horzExtent;
        float minY = bounds.min.y + vertExtent;
        float maxY = bounds.max.y - vertExtent;

        targetPosition.x = (maxX > minX) ? Mathf.Clamp(targetPosition.x, minX, maxX) : bounds.center.x;
        targetPosition.y = (maxY > minY) ? Mathf.Clamp(targetPosition.y, minY, maxY) : bounds.center.y;
        targetPosition.z = transform.position.z;
    }

    float GetMaxAllowedZoom()
    {
        if (levelBounds == null) return maxZoom;
        Bounds bounds = levelBounds.bounds;

        float maxZoomY = bounds.size.y * 0.5f;
        float maxZoomX = bounds.size.x / (2f * cam.aspect);

        return Mathf.Min(maxZoom, maxZoomX, maxZoomY);
    }
}