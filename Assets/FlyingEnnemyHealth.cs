using UnityEngine;
using UnityEngine.UI;

public class FlyingEnnemyHealth : MonoBehaviour
{
    public Slider healthSlider;
    public FlyingEnemyMovement enemyMovement;

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