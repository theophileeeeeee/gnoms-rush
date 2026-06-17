using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

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
    private bool isDragging;
    private float lastTouchDistance;
    private bool isPinching;

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

    void HandleInput()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            isDragging = false;
            return;
        }

        if (Touchscreen.current != null && Touchscreen.current.touches.Count > 0)
        {
            HandleDragAndZoomMobile();
        }
        
        if (!isPinching)
        {
            HandleDragPC();
            HandleZoomPC();
        }
    }

    void HandleDragAndZoomMobile()
    {
        var touches = Touchscreen.current.touches;
        int activeTouches = 0;

        for (int i = 0; i < touches.Count; i++)
        {
            if (touches[i].press.isPressed)
            {
                activeTouches++;
            }
        }

        if (activeTouches == 1)
        {
            isPinching = false;
            UnityEngine.InputSystem.Controls.TouchControl activeTouch = null;

            for (int i = 0; i < touches.Count; i++)
            {
                if (touches[i].press.isPressed)
                {
                    activeTouch = touches[i];
                    break;
                }
            }

            if (activeTouch != null)
            {
                if (activeTouch.press.wasPressedThisFrame)
                {
                    if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(activeTouch.touchId.ReadValue()))
                    {
                        isDragging = false;
                        return;
                    }
                    lastInputPos = activeTouch.position.ReadValue();
                    isDragging = true;
                }
                else if (activeTouch.press.isPressed && isDragging)
                {
                    Vector2 currentPos = activeTouch.position.ReadValue();
                    ApplyMove(currentPos - lastInputPos);
                    lastInputPos = currentPos;
                }
            }
        }
        else if (activeTouches >= 2)
        {
            isDragging = false;
            
            UnityEngine.InputSystem.Controls.TouchControl touch0 = null;
            UnityEngine.InputSystem.Controls.TouchControl touch1 = null;

            for (int i = 0; i < touches.Count; i++)
            {
                if (touches[i].press.isPressed)
                {
                    if (touch0 == null) touch0 = touches[i];
                    else if (touch1 == null) { touch1 = touches[i]; break; }
                }
            }

            if (touch0 != null && touch1 != null)
            {
                Vector2 t0 = touch0.position.ReadValue();
                Vector2 t1 = touch1.position.ReadValue();
                float currentDistance = Vector2.Distance(t0, t1);

                if (!isPinching)
                {
                    lastTouchDistance = currentDistance;
                    isPinching = true;
                }
                else
                {
                    float zoomDelta = (lastTouchDistance - currentDistance) * 0.01f * zoomSensitivity;
                    Vector3 zoomCenter = (t0 + t1) * 0.5f;
                    ApplyZoom(zoomDelta, zoomCenter);
                    lastTouchDistance = currentDistance;
                }
            }
        }
        else
        {
            isDragging = false;
            isPinching = false;
        }
    }

    void HandleDragPC()
    {
        if (Mouse.current == null) return;

        bool isLeftPressed = Mouse.current.leftButton.isPressed;
        bool isMiddlePressed = Mouse.current.middleButton.isPressed;

        if (isLeftPressed || isMiddlePressed)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();

            if (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.middleButton.wasPressedThisFrame)
            {
                lastInputPos = mousePos;
                isDragging = true;
            }
            else if (isDragging)
            {
                Vector2 delta = mousePos - lastInputPos;
                if (delta.sqrMagnitude > 1f)
                {
                    ApplyMove(delta);
                    lastInputPos = mousePos;
                }
            }
        }
        else
        {
            isDragging = false;
        }
    }

    void HandleZoomPC()
    {
        if (Mouse.current == null) return;

        Vector2 scrollValue = Mouse.current.scroll.ReadValue();
        if (Mathf.Abs(scrollValue.y) > 0.001f)
        {
            float zoomDelta = -scrollValue.y * 0.001f * zoomSensitivity * 2f;
            Vector3 zoomCenter = Mouse.current.position.ReadValue();
            ApplyZoom(zoomDelta, zoomCenter);
        }
    }

    void ApplyMove(Vector2 delta)
    {
        Vector3 move = new Vector3(-delta.x, -delta.y, 0f)
                       * dragSpeed
                       * targetZoom
                       * 2f
                       / Screen.height;

        dragVelocity += move * inertia;
    }

    void ApplyZoom(float zoomDelta, Vector3 zoomCenter)
    {
        if (Mathf.Abs(zoomDelta) > 0.0001f)
        {
            float previousZoom = targetZoom;
            targetZoom += zoomDelta;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, GetMaxAllowedZoom());

            float currentZoomRatio = targetZoom / previousZoom;
            Vector3 rayStart = cam.ScreenToWorldPoint(zoomCenter);
            rayStart.z = targetPosition.z;

            targetPosition = rayStart + (targetPosition - rayStart) * currentZoomRatio;
        }
    }

    void ApplyInertia()
    {
        targetPosition += dragVelocity * Time.deltaTime;

        dragVelocity = Vector3.Lerp(
            dragVelocity,
            Vector3.zero,
            damping * Time.deltaTime
        );
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

        Vector3 clamped = targetPosition;

        if (maxX > minX)
            clamped.x = Mathf.Clamp(targetPosition.x, minX, maxX);
        else
            clamped.x = bounds.center.x;

        if (maxY > minY)
            clamped.y = Mathf.Clamp(targetPosition.y, minY, maxY);
        else
            clamped.y = bounds.center.y;

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