using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ReinforcementManager : MonoBehaviour
{
    public Camera cam;

    [Header("Units")]
    public GameObject knightPrefab;
    public int count = 2;
    public float spacing = 0.5f;

    [Header("Cooldown")]
    public float cooldown = 20f;
    private float cooldownTimer;

    public Image cooldownImage;

    [Header("Selection")]
    public GameObject visualIndicator;

    private bool isActive = false;
    private bool hasPlacedThisFrame = false;

    void Awake()
    {
        if (cam == null)
            cam = Camera.main;

        cooldownTimer = cooldown;

        if (cooldownImage != null)
            cooldownImage.fillAmount = 1f;
    }

    void Update()
    {
        // --- cooldown ---
        if (cooldownTimer < cooldown)
        {
            cooldownTimer += Time.deltaTime;

            if (cooldownImage != null)
                cooldownImage.fillAmount = cooldownTimer / cooldown;
        }

        if (!isActive) return;
        if (cooldownTimer < cooldown) return;

        hasPlacedThisFrame = false;

        // PC
        if (Input.GetMouseButtonDown(0))
        {
            if (IsPointerOverUI()) return;
            SpawnOnce();
        }

        // Mobile
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                if (IsPointerOverUI(touch.fingerId)) return;
                SpawnOnce();
            }
        }
    }

    // --- appelé par bouton UI ---
    public void OnClickButton()
    {
        if (cooldownTimer < cooldown)
            return;

        SetActive(!isActive);
    }

    void SpawnOnce()
    {
        if (hasPlacedThisFrame) return;

        hasPlacedThisFrame = true;
        SpawnReinforcements();
    }

void SpawnReinforcements()
{
    Vector3 screenPos;

    // PC
    if (Input.mousePresent)
    {
        screenPos = Input.mousePosition;
    }
    // Mobile
    else if (Input.touchCount > 0)
    {
        screenPos = Input.GetTouch(0).position;
    }
    else
    {
        return;
    }

    screenPos.z = 10f;
    Vector3 center = cam.ScreenToWorldPoint(screenPos);

    for (int i = 0; i < count; i++)
    {
        Vector3 offset = new Vector3((i - (count - 1) / 2f) * spacing, 0, 0);

        GameObject knight = Instantiate(knightPrefab, center + offset, Quaternion.identity);

       knight.transform.localScale = new Vector3(0.14f, 0.14f, 0.14f);
       Destroy(knight, 20f);
    }

    cooldownTimer = 0f;

    if (cooldownImage != null)
        cooldownImage.fillAmount = 0f;

    SetActive(false);
}

    public void SetActive(bool state)
    {
        isActive = state;

        if (visualIndicator != null)
            visualIndicator.SetActive(state);
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