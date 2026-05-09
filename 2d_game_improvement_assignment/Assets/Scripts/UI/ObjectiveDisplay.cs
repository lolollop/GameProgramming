using TMPro;
using UnityEngine;

public class ObjectiveDisplay : UIelement
{
    [TextArea]
    public string objectivePrefix = "Objective";
    public TextMeshProUGUI displayText;

    public override void UpdateUI()
    {
        base.UpdateUI();
        if (displayText == null)
        {
            return;
        }
        if (GameManager.instance == null || !GameManager.instance.gameIsWinnable)
        {
            displayText.text = objectivePrefix + ": Survive and score points.";
            return;
        }
        displayText.text = objectivePrefix + ": Defeat " + GameManager.instance.EnemiesRemainingToWin + " more enemies.";
    }
}
