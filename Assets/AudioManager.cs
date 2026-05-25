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

    public void ToggleMusic()
    {
        isMusicMuted = !isMusicMuted;

        if (isMusicMuted)
        {
            mainMixer.SetFloat("MusicVol", mutedVolume);
            if (musicButtonImage != null && musicOffSprite != null)
                musicButtonImage.sprite = musicOffSprite;
        }
        else
        {
            mainMixer.SetFloat("MusicVol", normalVolume);
            if (musicButtonImage != null && musicOnSprite != null)
                musicButtonImage.sprite = musicOnSprite;
        }
    }

    public void ToggleSFX()
    {
        isSFXMuted = !isSFXMuted;

        if (isSFXMuted)
        {
            mainMixer.SetFloat("SFXVol", mutedVolume);
            if (sfxButtonImage != null && sfxOffSprite != null)
                sfxButtonImage.sprite = sfxOffSprite;
        }
        else
        {
            mainMixer.SetFloat("SFXVol", normalVolume);
            if (sfxButtonImage != null && sfxOnSprite != null)
                sfxButtonImage.sprite = sfxOnSprite;
        }
    }
}