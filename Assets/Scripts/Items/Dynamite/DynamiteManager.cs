using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class DynamiteManager : MonoBehaviour
{
    public ItemManager itemManager;
    public Camera cam;
    public GameObject itemPrefab;
    public string itemName = "dynamite";

    [Header("Selection (Scale)")]
    public float scaleIncreasePercent = 15f;
    private Vector3 originalScale;

    [Header("Audio")]
    public AudioClip selectSound;
    [Range(0f, 1f)] public float selectVolume = 1f;

    [Header("Exclusive Selection")]
    public DynamiteManager otherDynamite;
    public ReinforcementManager reinforcementManager;

    public AudioSource audioSource;
    private bool isActive = false;

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

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        originalScale = transform.localScale;
    }

    void Update()
    {
        if (!isActive) return;

        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            if (IsPointerOverUI()) return;

            Vector2 clickPosition = Pointer.current.position.ReadValue();
            PlaceDynamite(clickPosition);
        }
    }

    public void SetActive(bool state)
    {
        isActive = state;

        if (state)
        {
            float multiplier = 1f + (scaleIncreasePercent / 100f);
            transform.localScale = originalScale * multiplier;

            if (selectSound != null)
                audioSource.PlayOneShot(selectSound, selectVolume);
        }
        else
        {
            transform.localScale = originalScale;
        }
    }

    public void Toggle()
    {
        if (!isActive)
        {
            if (otherDynamite != null) otherDynamite.SetActive(false);
            if (reinforcementManager != null) reinforcementManager.SetActive(false);
        }
        SetActive(!isActive);
    }

    void PlaceDynamite(Vector2 screenPos)
    {
        if (itemManager == null) return;

        if (itemManager.GetItemAmount(itemName) <= 0)
        {
            Debug.Log("Plus de " + itemName + " en stock !");
            SetActive(false);
            return;
        }

        Ray ray = cam.ScreenPointToRay(screenPos);
        Vector3 targetPos = Vector3.zero;
        bool hitValidPath = false;

        if (Physics.Raycast(ray, out RaycastHit hit3D))
        {
            if (hit3D.collider.CompareTag("Path"))
            {
                targetPos = hit3D.point;
                hitValidPath = true;
            }
        }
        else
        {
            RaycastHit2D hit2D = Physics2D.GetRayIntersection(ray);
            if (hit2D.collider != null && hit2D.collider.CompareTag("Path"))
            {
                targetPos = hit2D.point;
                hitValidPath = true;
            }
        }

        if (!hitValidPath)
        {
            SetActive(false);
            return;
        }

        Vector3 midPos = targetPos + Vector3.left * preDropOffsetX + Vector3.up * (entryOffsetY * 0.5f);
        Vector3 startPos = midPos + Vector3.left * entryOffsetX + Vector3.up * entryOffsetY;

        GameObject dyn = Instantiate(itemPrefab, startPos, Quaternion.identity);
        StartCoroutine(ArrivalSequence(dyn.transform, startPos, midPos, targetPos));

        itemManager.UseItem(itemName, 1);
        SetActive(false);
    }

    IEnumerator ArrivalSequence(Transform obj, Vector3 start, Vector3 mid, Vector3 target)
    {
        float t = 0f;

        while (t < entryDuration)
        {
            t += Time.deltaTime;
            float p = t / entryDuration;
            obj.position = Vector3.Lerp(start, mid, p);
            yield return null;
        }

        obj.position = mid;
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

    bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;
        return EventSystem.current.IsPointerOverGameObject();
    }
}