using UnityEngine;
using UnityEngine.UI;

public class EnnemyHealth : MonoBehaviour
{
    public Slider healthSlider;
    public EnemyMovement enemyMovement;

    void Start()
    {
        healthSlider.maxValue = enemyMovement.maxHealth;
        healthSlider.value = enemyMovement.maxHealth;
    }

    void Update()
    {
        healthSlider.value = enemyMovement.currentHealth;
    }
}