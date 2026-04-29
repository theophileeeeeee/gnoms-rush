using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController2D : MonoBehaviour
{
    [Header("Références")]
    public BoxCollider2D levelBounds;

    [Header("Déplacement")]
    public float dragSpeed = 1.5f;
    public float inertia = 5f;
    public float damping = 10f;

    [Header("Zoom")]
    public float minZoom = 3f;
    public float maxZoom = 8f;
    public float zoomSensitivity = 1f;
    public float zoomLerpSpeed = 8f;

    [Header("Limites")]
    public float edgeSoftness = 8f;

    private Camera cam;

    private Vector3 targetPosition;
    private Vector3 dragVelocity;

    private float targetZoom;

    private Vector2 lastInputPos;

    void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;

        targetPosition = transform.position;
        targetZoom = cam.orthographicSize;
    }

    void Update()
    {
        HandleInput();
        ApplyInertia();
        ClampTargetPosition();
        ApplyFinalTransform();
    }

    // ================= INPUT =================

    void HandleInput()
    {
        HandleDrag();
        HandleZoom();
    }

    void HandleDrag()
    {
        // MOBILE
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
                lastInputPos = touch.position;

            if (touch.phase == TouchPhase.Moved)
            {
                ApplyMove(touch.position - lastInputPos);
                lastInputPos = touch.position;
            }
        }
        // PC
        else if (Input.GetMouseButton(0) || Input.GetMouseButton(2))
        {
            Vector2 mousePos = Input.mousePosition;

            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(2))
                lastInputPos = mousePos;

            ApplyMove(mousePos - lastInputPos);
            lastInputPos = mousePos;
        }
    }

    void ApplyMove(Vector2 delta)
    {
        Vector3 move = new Vector3(-delta.x, -delta.y, 0f)
                       * dragSpeed
                       * cam.orthographicSize
                       / Screen.height;

        dragVelocity += move * inertia;
    }

    void HandleZoom()
    {
        float zoomDelta = 0f;
        Vector3 zoomCenter = Input.mousePosition;

        // MOBILE PINCH
        if (Input.touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            Vector2 t0Prev = t0.position - t0.deltaPosition;
            Vector2 t1Prev = t1.position - t1.deltaPosition;

            float prevDist = Vector2.Distance(t0Prev, t1Prev);
            float currDist = Vector2.Distance(t0.position, t1.position);

            zoomDelta = (prevDist - currDist) * 0.01f * zoomSensitivity;

            zoomCenter = (t0.position + t1.position) * 0.5f;
        }
        // PC SCROLL
        else
        {
            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.001f)
            {
                zoomDelta = -scroll * zoomSensitivity * 2f;
                zoomCenter = Input.mousePosition;
            }
        }

        if (Mathf.Abs(zoomDelta) > 0.0001f)
        {
            Vector3 beforeZoom = cam.ScreenToWorldPoint(zoomCenter);

            targetZoom += zoomDelta;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, GetMaxAllowedZoom());

            Vector3 afterZoom = cam.ScreenToWorldPoint(zoomCenter);
            Vector3 offset = beforeZoom - afterZoom;

            targetPosition += offset;
        }
    }

    // ================= PHYSICS FEEL =================

    void ApplyInertia()
    {
        targetPosition += dragVelocity * Time.deltaTime;

        dragVelocity = Vector3.Lerp(
            dragVelocity,
            Vector3.zero,
            damping * Time.deltaTime
        );
    }

    // ================= LIMITES =================

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

        Vector3 clamped = targetPosition;

        if (maxX > minX)
            clamped.x = Mathf.Clamp(targetPosition.x, minX, maxX);
        else
            clamped.x = bounds.center.x;

        if (maxY > minY)
            clamped.y = Mathf.Clamp(targetPosition.y, minY, maxY);
        else
            clamped.y = bounds.center.y;

        // Soft clamp (évite effet mur)
        targetPosition = Vector3.Lerp(
            targetPosition,
            clamped,
            1f - Mathf.Exp(-edgeSoftness * Time.deltaTime)
        );

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

    // ================= RENDER =================

    void ApplyFinalTransform()
    {
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            1f - Mathf.Exp(-12f * Time.deltaTime)
        );

        cam.orthographicSize = Mathf.Lerp(
            cam.orthographicSize,
            targetZoom,
            1f - Mathf.Exp(-zoomLerpSpeed * Time.deltaTime)
        );
    }
}