using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MainMenuTutorial : MonoBehaviour
{
    [System.Serializable]
    public class TutorialStep
    {
        [TextArea(3, 5)]
        public string message;
        public RectTransform highlightTarget;
        public float fontSize = 36f;
    }

    [Header("UI Elements")]
    public GameObject tutorialPanel;
    public Image overlayImage;
    public TextMeshProUGUI bubbleText;
    public TextMeshProUGUI progressText;
    public Button nextButton;
    public TextMeshProUGUI nextButtonText;

    [Header("Steps")]
    public List<TutorialStep> steps;

    [Header("Camera Control (Optional)")]
    public CameraController2D cameraMovementScript;

    int currentStep = 0;
    Transform lastTargetOriginalParent;
    int lastTargetOriginalIndex;
    RectTransform currentActiveTarget;

    void Start()
    {
        if (PlayerPrefs.GetInt("TutorialDone", 0) == 1)
        {
            if (tutorialPanel != null) tutorialPanel.SetActive(false);
            SetCameraMovement(true);
            return;
        }

        if (nextButton != null) nextButton.onClick.AddListener(NextStep);

        if (tutorialPanel != null) tutorialPanel.SetActive(true);
        SetCameraMovement(false);

        ShowStep(0);
    }

    void ShowStep(int index)
    {
        ResetLastTargetParent();

        if (steps == null || steps.Count == 0 || index >= steps.Count)
        {
            EndTutorial();
            return;
        }

        currentStep = index;
        TutorialStep step = steps[index];

        if (bubbleText != null)
        {
            bubbleText.text = step.message;
            bubbleText.fontSize = step.fontSize;
        }

     
        if (progressText != null)
        {
            progressText.text = (index + 1) + "/" + steps.Count;
        }

        if (nextButtonText != null)
        {
            nextButtonText.text = (index == steps.Count - 1) ? "TERMINER" : "SUIVANT";
        }

        if (step.highlightTarget != null)
        {
            currentActiveTarget = step.highlightTarget;
            lastTargetOriginalParent = currentActiveTarget.parent;
            lastTargetOriginalIndex = currentActiveTarget.GetSiblingIndex();

            Vector3 worldPos = currentActiveTarget.position;
            currentActiveTarget.SetParent(tutorialPanel.transform, true);
            currentActiveTarget.position = worldPos;
        }
    }

    void NextStep()
    {
        ShowStep(currentStep + 1);
    }

    void ResetLastTargetParent()
    {
        if (currentActiveTarget != null && lastTargetOriginalParent != null)
        {
            Vector3 worldPos = currentActiveTarget.position;
            currentActiveTarget.SetParent(lastTargetOriginalParent, true);
            currentActiveTarget.SetSiblingIndex(lastTargetOriginalIndex);
            currentActiveTarget.position = worldPos;
        }
        currentActiveTarget = null;
        lastTargetOriginalParent = null;
    }

    void EndTutorial()
    {
        ResetLastTargetParent();

        if (steps != null)
        {
            foreach (var step in steps)
            {
                if (step.highlightTarget != null && step.highlightTarget.parent == tutorialPanel.transform)
                {
                    currentActiveTarget = step.highlightTarget;
                    ResetLastTargetParent();
                }
            }
        }

        PlayerPrefs.SetInt("TutorialDone", 1);
        PlayerPrefs.Save();

        if (tutorialPanel != null) tutorialPanel.SetActive(false);
        if (overlayImage != null) overlayImage.gameObject.SetActive(false);
        if (nextButton != null) nextButton.gameObject.SetActive(false);
        if (progressText != null) progressText.gameObject.SetActive(false);
        SetCameraMovement(true);
    }

    void SetCameraMovement(bool state)
    {
        if (cameraMovementScript != null)
        {
            cameraMovementScript.enabled = state;
        }
    }
}