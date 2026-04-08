using UnityEngine;
using UnityEngine.UI;

public class KnightHealth : MonoBehaviour
{
    public Slider healthSlider;
    public KnightManager knightManager;

    void Start()
    {
        healthSlider.maxValue = knightManager.maxHealth;
        healthSlider.value = knightManager.maxHealth;
    }

    void Update()
    {
        healthSlider.value = knightManager.currentHealth;
    }
}