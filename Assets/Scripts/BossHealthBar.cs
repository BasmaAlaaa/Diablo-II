using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealthBar : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text healthText; // Text to display the numerical value

    // Updates both the slider and the overlaid text
    public void UpdateHealthBar(float currentValue, float maxValue)
    {
        slider.value = currentValue / maxValue;
        healthText.text = $"{Mathf.RoundToInt(currentValue)}/{Mathf.RoundToInt(maxValue)}"; // Update the numerical text
    }
}
