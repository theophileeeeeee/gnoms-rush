using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioMenuManager : MonoBehaviour
{
    [Header("References")]
    public AudioMixer mainMixer;

    [Header("Music UI")]
    public Image musicButtonImage;
    public Sprite musicOnSprite;
    public Sprite musicOffSprite;

    [Header("SFX UI")]
    public Image sfxButtonImage;
    public Sprite sfxOnSprite;
    public Sprite sfxOffSprite;

    private bool isMusicMuted = false;
    private bool isSFXMuted = false;

    private const float normalVolume = 0f;
    private const float mutedVolume = -80f;

    void Start()
    {
        isMusicMuted = PlayerPrefs.GetInt("MusicMuted", 0) == 1;
        isSFXMuted = PlayerPrefs.GetInt("SFXMuted", 0) == 1;

        mainMixer.SetFloat("MusicVol", isMusicMuted ? mutedVolume : normalVolume);
        mainMixer.SetFloat("SFXVol", isSFXMuted ? mutedVolume : normalVolume);

        if (musicButtonImage != null)
            musicButtonImage.sprite = isMusicMuted ? musicOffSprite : musicOnSprite;
        if (sfxButtonImage != null)
            sfxButtonImage.sprite = isSFXMuted ? sfxOffSprite : sfxOnSprite;
    }

    public void ToggleMusic()
    {
        isMusicMuted = !isMusicMuted;
        mainMixer.SetFloat("MusicVol", isMusicMuted ? mutedVolume : normalVolume);
        if (musicButtonImage != null)
            musicButtonImage.sprite = isMusicMuted ? musicOffSprite : musicOnSprite;
        PlayerPrefs.SetInt("MusicMuted", isMusicMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ToggleSFX()
    {
        isSFXMuted = !isSFXMuted;
        mainMixer.SetFloat("SFXVol", isSFXMuted ? mutedVolume : normalVolume);
        if (sfxButtonImage != null)
            sfxButtonImage.sprite = isSFXMuted ? sfxOffSprite : sfxOnSprite;
        PlayerPrefs.SetInt("SFXMuted", isSFXMuted ? 1 : 0);
        PlayerPrefs.Save();
    }
}