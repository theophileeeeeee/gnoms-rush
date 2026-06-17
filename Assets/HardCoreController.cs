using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HardcoreToggleController : MonoBehaviour
{
    [Header("UI Elements")]
    public Button toggleButton;
    public Image backgroundImage;
    public RectTransform handleRect;
    public GameObject fullToggleObject;

    [Header("Colors")]
    public Color offColor = new Color(0.7f, 0.7f, 0.7f, 1f);
    public Color onColor = new Color(0.85f, 0.1f, 0.1f, 1f);

    [Header("Positions (X Axis)")]
    public float offHandleX = -40f;
    public float onHandleX = 40f;

    [Header("Animation Settings")]
    public float duration = 0.2f;
    public Vector3 punchScale = new Vector3(1.15f, 0.85f, 1f);

    [Header("Debug / Test option")]
    public bool debugForceUnlock = false;

    private Coroutine animationCoroutine;
    private Vector3 originalScale;

    private bool IsHardcoreActiveReal
    {
        get => PlayerPrefs.GetInt("HardcoreMode", 0) == 1;
        set
        {
            PlayerPrefs.SetInt("HardcoreMode", value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    void Start()
    {
        originalScale = transform.localScale;

        if (debugForceUnlock)
        {
            PlayerPrefs.SetInt("HardcoreUnlocked", 1);
            PlayerPrefs.Save();
        }

        if (toggleButton != null)
        {
            toggleButton.onClick.RemoveAllListeners();
            toggleButton.onClick.AddListener(OnToggleClicked);
        }

        CheckVisibility();
    }

    void OnEnable()
    {
        CheckVisibility();
    }

    void CheckVisibility()
    {
        bool isUnlocked = PlayerPrefs.GetInt("HardcoreUnlocked", 0) == 1;
        GameObject target = fullToggleObject != null ? fullToggleObject : gameObject;

        target.SetActive(isUnlocked);

        if (isUnlocked)
        {
            RefreshToggleState();
        }
    }

    public void NotifyUnlockAndRefresh()
    {
        GameObject target = fullToggleObject != null ? fullToggleObject : gameObject;
        target.SetActive(true);
        RefreshToggleState();
    }

    public void RefreshToggleState()
    {
        UpdateToggleVisualsInstant();
    }

    void OnToggleClicked()
    {
        bool newState = !IsHardcoreActiveReal;
        IsHardcoreActiveReal = newState;

        if (MainMenuController.Instance != null)
        {
            MainMenuController.Instance.SetHardcoreModeDirect(newState);
        }

        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        animationCoroutine = StartCoroutine(AnimateToggle(newState));
    }

    void UpdateToggleVisualsInstant()
    {
        bool currentRealState = IsHardcoreActiveReal;

        if (backgroundImage != null)
            backgroundImage.color = currentRealState ? onColor : offColor;

        if (handleRect != null)
        {
            Vector2 pos = handleRect.anchoredPosition;
            pos.x = currentRealState ? onHandleX : offHandleX;
            handleRect.anchoredPosition = pos;
        }
    }

    IEnumerator AnimateToggle(bool targetState)
    {
        float elapsed = 0f;
        Color startColor = backgroundImage != null ? backgroundImage.color : offColor;
        Color endColor = targetState ? onColor : offColor;

        float startX = handleRect != null ? handleRect.anchoredPosition.x : offHandleX;
        float endX = targetState ? onHandleX : offHandleX;

        float halfDuration = duration * 0.5f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            transform.localScale = Vector3.Lerp(originalScale, Vector3.Scale(originalScale, punchScale), t);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;

            if (backgroundImage != null)
                backgroundImage.color = Color.Lerp(startColor, endColor, t);

            if (handleRect != null)
            {
                Vector2 pos = handleRect.anchoredPosition;
                pos.x = Mathf.Lerp(startX, endX, t);
                handleRect.anchoredPosition = pos;
            }

            transform.localScale = Vector3.Lerp(Vector3.Scale(originalScale, punchScale), originalScale, t);
            yield return null;
        }

        UpdateToggleVisualsInstant();
        transform.localScale = originalScale;
    }
}