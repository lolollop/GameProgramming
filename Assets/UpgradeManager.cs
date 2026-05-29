using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    [SerializeField] private UpgradePopupUI upgradePopupUI;

    private static UpgradeManager instance;
    private PlayerController player;
    private bool upgradePending;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreate()
    {
        if (FindObjectOfType<UpgradeManager>(true) != null)
        {
            return;
        }

        GameObject managerObject = new GameObject("UpgradeManager");
        managerObject.AddComponent<UpgradeManager>();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void Start()
    {
        player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.LevelUpOffered += ShowUpgradeChoices;
        }

        if (upgradePopupUI == null)
        {
            upgradePopupUI = FindObjectOfType<UpgradePopupUI>(true);
        }

        if (upgradePopupUI == null)
        {
            upgradePopupUI = UpgradePopupUI.CreateRuntimePopup();
        }

        upgradePopupUI.Hide();
    }

    private void OnDestroy()
    {
        if (player != null)
        {
            player.LevelUpOffered -= ShowUpgradeChoices;
        }
    }

    private void ShowUpgradeChoices(LevelUpOption[] options)
    {
        if (player == null || upgradePopupUI == null)
        {
            return;
        }

        upgradePending = true;
        player.SetInputLocked(true);
        Time.timeScale = 0f;
        upgradePopupUI.Show(options, ApplyUpgradeAndResume);
    }

    private void ApplyUpgradeAndResume(UpgradeType upgradeType)
    {
        if (player == null || !upgradePending)
        {
            return;
        }

        player.ApplyUpgrade(upgradeType);
        player.SetInputLocked(false);
        upgradePopupUI.Hide();
        upgradePending = false;
        Time.timeScale = 1f;
    }
}
