using UnityEngine;

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
    [SerializeField] private float transitionSpeed = 5f;          

    private AudioSource audioSource;
    private AudioLowPassFilter lowPassFilter;
    
    private float targetCutoff;
    private float targetVolume;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        lowPassFilter = GetComponent<AudioLowPassFilter>();

        audioSource.spatialBlend = 0f; 
        audioSource.playOnAwake = false;
        audioSource.loop = true;
        
        // Initialisation des volumes cibles
        audioSource.volume = musicVolume;
        targetVolume = musicVolume;

        audioSource.ignoreListenerPause = true;

        targetCutoff = normalCutoffFrequency;
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

    private void Update()
    {
        // DeltaTime indépendant de la pause
        float dt = Time.unscaledDeltaTime;

        // 1. Transition fluide du filtre
        lowPassFilter.cutoffFrequency = Mathf.MoveTowards(
            lowPassFilter.cutoffFrequency, 
            targetCutoff, 
            transitionSpeed * 2000f * dt
        );

        // 2. Transition fluide du volume pour compenser la perte des aigus
        audioSource.volume = Mathf.MoveTowards(
            audioSource.volume, 
            targetVolume, 
            transitionSpeed * dt
        );
    }

    public void SetPauseEffect(bool isPaused)
    {
        if (isPaused)
        {
            targetCutoff = pauseCutoffFrequency;
            // On booste le volume pendant la pause (bridé à 1.0f max pour éviter la saturation d'Unity)
            targetVolume = Mathf.Min(musicVolume * pauseVolumeMultiplier, 1f);
        }
        else
        {
            targetCutoff = normalCutoffFrequency;
            // On revient au volume initial du jeu
            targetVolume = musicVolume;
        }
    }
}