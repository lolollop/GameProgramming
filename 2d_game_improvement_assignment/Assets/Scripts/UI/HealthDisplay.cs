using TMPro;
using UnityEngine;

public class HealthDisplay : UIelement
{
    public TextMeshProUGUI displayText;
    public Health targetHealth;

    public override void UpdateUI()
    {
        base.UpdateUI();
        if (displayText == null)
        {
            return;
        }
        if (targetHealth == null && GameManager.instance != null && GameManager.instance.player != null)
        {
            targetHealth = GameManager.instance.player.GetComponent<Health>();
        }
        if (targetHealth != null)
        {
            displayText.text = "Health: " + targetHealth.currentHealth;
        }
    }
}
