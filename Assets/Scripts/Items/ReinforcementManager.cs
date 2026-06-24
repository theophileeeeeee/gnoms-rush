using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;

[RequireComponent(typeof(AudioSource))]
public class ReinforcementManager : MonoBehaviour
{
    public static Action OnReinforcementsPlaced;
    public Camera cam;

    [Header("Units")]
    public GameObject knightPrefab;
    public int count = 2;
    public float spacing = 0.5f;

    [Header("Cooldown")]
    public float cooldown = 30f;
    public float cooldownTimer;
    public Image cooldownImage;

    [Header("Selection (Scale)")]
    public float scaleIncreasePercent = 15f;
    private Vector3 originalScale;

    [Header("Audio")]
    public AudioClip selectSound;
    [Range(0f, 1f)] public float selectVolume = 1f;
    public AudioClip placeSound;
    [Range(0f, 1f)] public float placeVolume = 1f;

    [Header("Exclusive Selection")]
    public DynamiteManager dynamite1;
    public DynamiteManager dynamite2;

    public AudioSource audioSource;
    private bool isActive = false;

    void Awake()
    {
        if (cam == null)
            cam = Camera.main;

        cooldownTimer = cooldown;

        if (cooldownImage != null)
            cooldownImage.fillAmount = 0f;

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        originalScale = transform.localScale;
    }

    void Update()
    {
        if (cooldownTimer < cooldown)
        {
            cooldownTimer += Time.deltaTime;

            if (cooldownImage != null)
                cooldownImage.fillAmount = 1f - (cooldownTimer / cooldown);
        }

        if (!isActive) return;
        if (cooldownTimer < cooldown) return;

        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            if (IsPointerOverUI()) return;

            Vector2 clickPosition = Pointer.current.position.ReadValue();
            SpawnReinforcements(clickPosition);
        }
    }

    public void OnClickButton()
    {
        if (TutorialManager.Instance != null && TutorialManager.Instance.Phase1Done && !TutorialManager.Instance.TurretPlaced)
        {
            Debug.Log("Action impossible : Vous devez d'abord construire la tour !");
            return;
        }

        if (cooldownTimer < cooldown)
            return;

        if (!isActive)
        {
            if (dynamite1 != null) dynamite1.SetActive(false);
            if (dynamite2 != null) dynamite2.SetActive(false);
        }

        SetActive(!isActive);
    }

    void SpawnReinforcements(Vector2 screenPos)
    {
        Ray ray = cam.ScreenPointToRay(screenPos);
        Vector3 center = Vector3.zero;
        bool hitValidPath = false;

        if (Physics.Raycast(ray, out RaycastHit hit3D))
        {
            if (hit3D.collider.CompareTag("Path") || hit3D.collider.CompareTag("Ennemy"))
            {
                center = hit3D.point;
                hitValidPath = true;
            }
        }
        else
        {
            RaycastHit2D hit2D = Physics2D.GetRayIntersection(ray);
            if (hit2D.collider != null && (hit2D.collider.CompareTag("Path") || hit2D.collider.CompareTag("Ennemy")))
            {
                center = hit2D.point;
                hitValidPath = true;
            }
        }

        if (!hitValidPath)
        {
            SetActive(false);
            return;
        }

        for (int i = 0; i < count; i++)
        {
            Vector3 offset = new Vector3((i - (count - 1) / 2f) * spacing, 0, 0);
            GameObject knight = Instantiate(knightPrefab, center + offset, Quaternion.identity);
            knight.transform.localScale = new Vector3(0.10f, 0.10f, 0.10f);
        }

        OnReinforcementsPlaced?.Invoke();

        if (placeSound != null)
            audioSource.PlayOneShot(placeSound, placeVolume);

        cooldownTimer = 0f;

        if (cooldownImage != null)
            cooldownImage.fillAmount = 1f;

        SetActive(false);
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

    bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;
        return EventSystem.current.IsPointerOverGameObject();
    }
}