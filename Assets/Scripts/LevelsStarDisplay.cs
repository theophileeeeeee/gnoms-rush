using UnityEngine;
using UnityEngine.UI;

public class LevelStarDisplay : MonoBehaviour
{
    public string sceneName;
    public Sprite zeroStarsSprite;
    public Sprite oneStarSprite;
    public Sprite twoStarsSprite;
    public Sprite threeStarsSprite;

    void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        int stars = PlayerPrefs.GetInt("Stars_" + sceneName, 0);
        Image img = GetComponent<Image>();
        if (img == null) return;

        img.sprite = stars switch
        {
            1 => oneStarSprite,
            2 => twoStarsSprite,
            3 => threeStarsSprite,
            _ => zeroStarsSprite
        };
    }
}