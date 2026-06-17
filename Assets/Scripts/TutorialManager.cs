using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

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
    public Text nextButtonText;
    public GameObject bubblePhase1Object; 
    public List<TutorialStep> steps;

    [Header("Phase 2 - World Space")]
    public Canvas worldSpaceCanvas;
    public GameObject bubblePrefab;
    public GameObject reinforcementsBubblePrefab;
    public Node tutorialNode;
    public Transform waveButtonTarget;
    public Transform reinforcementsTarget;

    [Header("Skip Option")]
    public Button skipButton;

    [Header("Camera Control")]
    public CameraController2D cameraMovementScript;

    int currentStep = 0;
    
    public bool Phase1Done { get; private set; } = false;
    public bool TurretPlaced { get; private set; } = false;
    public bool ReinforcementsPlaced { get; private set; } = false;
    
    bool waveLaunched = false;
    bool isSkipped = false;
    GameObject currentWorldBubble;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        BuildManager.OnTurretBuilt += OnTurretPlaced;
        ReinforcementManager.OnReinforcementsPlaced += OnReinforcementsDeployed;
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
        ReinforcementManager.OnReinforcementsPlaced -= OnReinforcementsDeployed;
    }

    void ShowStep(int index)
    {
        if (isSkipped) return;

        if (index >= steps.Count)
        {
            EndPhase1();
            return;
        }

        currentStep = index;
        TutorialStep step = steps[index];
        
        bubbleText.text = step.message;
        bubbleText.fontSize = step.fontSize;

        if (nextButtonText != null)
        {
            if (index == steps.Count - 1)
            {
                nextButtonText.text = "TERMINER";
            }
            else
            {
                nextButtonText.text = "SUIVANT";
            }
        }

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
        if (isSkipped) return;

        Phase1Done = true;

        if (overlayImage != null) overlayImage.gameObject.SetActive(false);
        if (highlightImage != null) highlightImage.gameObject.SetActive(false);
        if (nextButton != null) nextButton.gameObject.SetActive(false);
        if (bubblePhase1Object != null) bubblePhase1Object.gameObject.SetActive(false);

        SetCameraMovement(true);

        ShowWorldBubble(tutorialNode.transform.position, "Construis ta première tour ici !", bubblePrefab);
    }

    void OnTurretPlaced(Node node)
    {
        if (isSkipped || !Phase1Done) return;
        if (node != tutorialNode) return;

        TurretPlaced = true;

        if (currentWorldBubble != null)
            Destroy(currentWorldBubble);

        GameObject selectedPrefab = reinforcementsBubblePrefab != null ? reinforcementsBubblePrefab : bubblePrefab;

        if (reinforcementsTarget != null)
        {
            ShowWorldBubble(reinforcementsTarget.position, "Clique ici puis sur le chemin pour déployer des renforts!", selectedPrefab);
        }
        else
        {
            ShowWorldBubble(Vector3.zero, "Déploie tes renforts sur le chemin !", selectedPrefab);
        }
    }

    void OnReinforcementsDeployed()
    {
        if (isSkipped || !Phase1Done || ReinforcementsPlaced) return;

        ReinforcementsPlaced = true;

        if (currentWorldBubble != null)
            Destroy(currentWorldBubble);

        ShowWorldBubble(waveButtonTarget.position, "Parfait ! Lance maintenant la première vague !", bubblePrefab);
    }

    public void OnFirstWaveLaunched()
    {
        if (isSkipped || !Phase1Done || !ReinforcementsPlaced || waveLaunched) return;

        waveLaunched = true;

        if (currentWorldBubble != null)
        {
            Destroy(currentWorldBubble);
        }

        if (skipButton != null)
        {
            skipButton.gameObject.SetActive(false);
        }

        SetCameraMovement(true);

        Debug.Log("Tutoriel complété : Renforts et première vague validés !");
    }

    public void SkipTutorial()
    {
        isSkipped = true;
        Phase1Done = true;
        TurretPlaced = true;
        ReinforcementsPlaced = true;
        waveLaunched = true;

        BuildManager.OnTurretBuilt -= OnTurretPlaced;
        ReinforcementManager.OnReinforcementsPlaced -= OnReinforcementsDeployed;

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

    void ShowWorldBubble(Vector3 worldPosition, string message, GameObject prefabToUse)
    {
        if (isSkipped) return;

        if (currentWorldBubble != null)
            Destroy(currentWorldBubble);

        Vector3 originalScale = prefabToUse.transform.localScale;

        currentWorldBubble = Instantiate(prefabToUse, worldPosition + Vector3.up * 0.5f, Quaternion.identity, worldSpaceCanvas.transform);

        currentWorldBubble.transform.localScale = originalScale;

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