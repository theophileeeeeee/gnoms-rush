using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(AudioLowPassFilter))]
public class MusicManager : MonoBehaviour
{
    [Header("Setup Audio")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.5f;
    
    // Le multiplicateur de volume pendant la pause (ex: 1.5f = +50% de volume)
    [SerializeField] [Range(1f, 2f)] private float pauseVolumeMultiplier = 1.4f; 

    [Header("Configuration Filtre Pause")]
    [SerializeField] private float normalCutoffFrequency = 22000f;
    [SerializeField] private float pauseCutoffFrequency = 800f;   
    [SerializeField] [Tooltip("Durée de la transition en secondes")] private float transitionDuration = 2f;          

    public AudioSource audioSource;
    private AudioLowPassFilter lowPassFilter;
    
    private Coroutine transitionCoroutine;

    private void Awake()
    {
        lowPassFilter = GetComponent<AudioLowPassFilter>();

        audioSource.spatialBlend = 0f; 
        audioSource.playOnAwake = false;
        audioSource.loop = true;
        
        // Initialisation
        audioSource.volume = musicVolume;
        audioSource.ignoreListenerPause = true;

        lowPassFilter.cutoffFrequency = normalCutoffFrequency;
    }

    private void Start()
    {
        if (backgroundMusic != null)
        {
            audioSource.clip = backgroundMusic;
            audioSource.Play();
        }
    }

    public void SetPauseEffect(bool isPaused)
    {
        // Détermination des valeurs cibles
        float targetCutoff = isPaused ? pauseCutoffFrequency : normalCutoffFrequency;
        float targetVolume = isPaused ? Mathf.Min(musicVolume * pauseVolumeMultiplier, 1f) : musicVolume;

        // Si une transition est déjà en cours, on l'arrête pour éviter les conflits
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }

        // On lance la transition douce
        transitionCoroutine = StartCoroutine(TransitionRoutine(targetCutoff, targetVolume));
    }

    private IEnumerator TransitionRoutine(float targetCutoff, float targetVolume)
    {
        float startCutoff = lowPassFilter.cutoffFrequency;
        float startVolume = audioSource.volume;
        float time = 0f;

        while (time < transitionDuration)
        {
            // Utilisation de unscaledDeltaTime pour que ça fonctionne même quand le Time.timeScale vaut 0
            time += Time.unscaledDeltaTime;
            
            // Calcul du ratio de progression (0.0f au début, 1.0f à la fin)
            float progress = Mathf.Clamp01(time / transitionDuration);

            // Évolution fluide avec un lissage (SmoothStep) pour éviter un effet trop linéaire brusque
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            lowPassFilter.cutoffFrequency = Mathf.Lerp(startCutoff, targetCutoff, smoothProgress);
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, smoothProgress);

            yield return null;
        }

        // Sécurité pour s'assurer d'atteindre pile la valeur finale
        lowPassFilter.cutoffFrequency = targetCutoff;
        audioSource.volume = targetVolume;
    }
}