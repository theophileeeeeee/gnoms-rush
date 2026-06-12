using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class TutorialManager : MonoBehaviour
{
    [System.Serializable]
    public class TutorialStep
    {
        [TextArea(3, 5)]
        public string message;
        public RectTransform highlightTarget;
        public float fontSize = 36f;
    }

    [Header("Phase 1 - Screen Space")]
    public Canvas screenSpaceCanvas;
    public Image overlayImage;
    public Image highlightImage;
    public TextMeshProUGUI bubbleText;
    public Button nextButton;
    public GameObject bubblePhase1Object; 
    public List<TutorialStep> steps;

    [Header("Phase 2 - World Space")]
    public Canvas worldSpaceCanvas;
    public GameObject bubblePrefab;
    public Node tutorialNode;
    public Transform waveButtonTarget;
    public Transform reinforcementsTarget;

    [Header("Skip Option")]
    public Button skipButton;

    [Header("Camera Control")]
    public CameraController2D cameraMovementScript;

    int currentStep = 0;
    bool phase1Done = false;
    bool waveLaunched = false;
    GameObject currentWorldBubble;

    void Start()
    {
        BuildManager.OnTurretBuilt += OnTurretPlaced;
        nextButton.onClick.AddListener(NextStep);
        
        if (skipButton != null)
        {
            skipButton.onClick.AddListener(SkipTutorial);
        }

        SetCameraMovement(false);
        ShowStep(0);
    }

    void OnDestroy()
    {
        BuildManager.OnTurretBuilt -= OnTurretPlaced;
    }

    void ShowStep(int index)
    {
        if (index >= steps.Count)
        {
            EndPhase1();
            return;
        }

        currentStep = index;
        TutorialStep step = steps[index];
        
        bubbleText.text = step.message;
        bubbleText.fontSize = step.fontSize;

        if (step.highlightTarget != null)
        {
            highlightImage.gameObject.SetActive(true);
            highlightImage.rectTransform.position = step.highlightTarget.position;
            highlightImage.rectTransform.sizeDelta = step.highlightTarget.sizeDelta + new Vector2(20f, 20f);
        }
        else
        {
            highlightImage.gameObject.SetActive(false);
        }
    }

    void NextStep()
    {
        ShowStep(currentStep + 1);
    }

    void EndPhase1()
    {
        phase1Done = true;

        if (overlayImage != null) overlayImage.gameObject.SetActive(false);
        if (highlightImage != null) highlightImage.gameObject.SetActive(false);
        if (nextButton != null) nextButton.gameObject.SetActive(false);
        if (bubblePhase1Object != null) bubblePhase1Object.gameObject.SetActive(false);

        // La Phase 1 est finie, on redonne IMMÉDIATEMENT le contrôle de la caméra au joueur
        SetCameraMovement(true);

        ShowWorldBubble(tutorialNode.transform.position, "Construisez votre première tour ici !");
    }

    void OnTurretPlaced(Node node)
    {
        if (!phase1Done) return;
        if (node != tutorialNode) return;

        if (currentWorldBubble != null)
            Destroy(currentWorldBubble);

        ShowWorldBubble(waveButtonTarget.position, "Lancez la première vague !");
    }

    public void OnFirstWaveLaunched()
    {
        if (!phase1Done || waveLaunched) return;

        waveLaunched = true;

        if (currentWorldBubble != null)
        {
            Destroy(currentWorldBubble);
        }

        if (skipButton != null) skipButton.gameObject.SetActive(false);

        // Sécurité : On s'assure qu'elle est bien active ici aussi
        SetCameraMovement(true);

        Debug.Log("Tutoriel complété : Première vague validée !");
    }

    public void SkipTutorial()
    {
        phase1Done = true;
        waveLaunched = true;

        if (overlayImage != null) overlayImage.gameObject.SetActive(false);
        if (highlightImage != null) highlightImage.gameObject.SetActive(false);
        if (nextButton != null) nextButton.gameObject.SetActive(false);
        if (bubblePhase1Object != null) bubblePhase1Object.gameObject.SetActive(false);

        if (currentWorldBubble != null)
        {
            Destroy(currentWorldBubble);
        }

        if (skipButton != null)
        {
            skipButton.gameObject.SetActive(false);
        }

        SetCameraMovement(true);

        Debug.Log("Tutoriel passé par le joueur.");
    }

    void ShowWorldBubble(Vector3 worldPosition, string message)
    {
        if (currentWorldBubble != null)
            Destroy(currentWorldBubble);

        currentWorldBubble = Instantiate(bubblePrefab, worldPosition + Vector3.up * 0.5f, Quaternion.identity, worldSpaceCanvas.transform);

        TextMeshProUGUI tmp = currentWorldBubble.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null)
            tmp.text = message;
    }

    void SetCameraMovement(bool state)
    {
        if (cameraMovementScript != null)
        {
            cameraMovementScript.enabled = state;
        }
    }
}