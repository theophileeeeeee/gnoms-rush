using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class DynamiteManager : MonoBehaviour
{
    public ItemManager itemManager;
    public Camera cam;
    public GameObject itemPrefab;
    public string itemName = "dynamite";

    public GameObject visualIndicator;

    private bool isActive = false;
    private bool hasPlacedThisFrame = false;

    [Header("Arrival Settings")]
    public float entryOffsetX = 2f;
    public float entryOffsetY = 1.5f;
    public float preDropOffsetX = 1.5f;
    public float entryDuration = 0.2f;

    [Header("Arc Settings")]
    public float arcDuration = 0.25f;
    public float arcHeight = 0.6f;

    void Awake()
    {
        if (cam == null)
            cam = Camera.main;
    }

    void Update()
    {
        if (!isActive) return;

        hasPlacedThisFrame = false;

        // PC
        if (Input.GetMouseButtonDown(0))
        {
            if (IsPointerOverUI()) return;
            PlaceOnce();
        }

        // Mobile
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                if (IsPointerOverUI(touch.fingerId)) return;
                PlaceOnce();
            }
        }
    }

    void PlaceOnce()
    {
        if (hasPlacedThisFrame) return;

        hasPlacedThisFrame = true;
        PlaceAtMouse();
    }

    public void SetActive(bool state)
    {
        isActive = state;

        if (visualIndicator != null)
            visualIndicator.SetActive(state);
    }

    public void Toggle()
    {
        SetActive(!isActive);
    }

    void PlaceAtMouse()
    {
        if (itemManager == null) return;

        if (itemManager.GetItemAmount(itemName) <= 0)
        {
            Debug.Log("Plus de "+ itemName+" en stock !");
            SetActive(false);
            return;
        }

        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10f;

        Vector3 targetPos = cam.ScreenToWorldPoint(mousePos);

        Vector3 midPos = targetPos 
        + Vector3.left * preDropOffsetX 
        + Vector3.up * (entryOffsetY * 0.5f); // léger offset vertical

        Vector3 startPos = midPos 
        + Vector3.left * entryOffsetX 
        + Vector3.up * entryOffsetY; // arrivée plus haute

        GameObject dyn = Instantiate(itemPrefab, startPos, Quaternion.identity);

        StartCoroutine(ArrivalSequence(dyn.transform, startPos, midPos, targetPos));

        itemManager.UseItem(itemName, 1);

        SetActive(false);
    }

    IEnumerator ArrivalSequence(Transform obj, Vector3 start, Vector3 mid, Vector3 target)
    {
        float t = 0f;

        // --- PHASE 1 : arrivée latérale ---
        while (t < entryDuration)
        {
            t += Time.deltaTime;
            float p = t / entryDuration;

            obj.position = Vector3.Lerp(start, mid, p);
            yield return null;
        }

        obj.position = mid;

        // --- PHASE 2 : petit arc ---
        t = 0f;

        while (t < arcDuration)
        {
            t += Time.deltaTime;
            float p = t / arcDuration;

            Vector3 pos = Vector3.Lerp(mid, target, p);

            float arc = arcHeight * 4 * (p - p * p);
            pos.y += arc;

            obj.position = pos;

            yield return null;
        }

        obj.position = target;
    }

    bool IsPointerOverUI(int fingerId = -1)
    {
        if (EventSystem.current == null) return false;

#if UNITY_ANDROID || UNITY_IOS
        if (fingerId >= 0)
            return EventSystem.current.IsPointerOverGameObject(fingerId);
#endif

        return EventSystem.current.IsPointerOverGameObject();
    }
}