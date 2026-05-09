using TMPro;
using UnityEngine;

public class DifficultyDisplay : UIelement
{
    public TextMeshProUGUI displayText;

    public override void UpdateUI()
    {
        base.UpdateUI();
        if (displayText != null)
        {
            displayText.text = "Difficulty: " + DifficultyDirector.CurrentDifficultyLevel;
        }
    }

    private void Update()
    {
        UpdateUI();
    }
}
