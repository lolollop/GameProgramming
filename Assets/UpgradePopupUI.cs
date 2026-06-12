using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Displays the three level-up choices and reports the selected upgrade type.
public class UpgradePopupUI : MonoBehaviour
{
    [SerializeField] private GameObject popupRoot;
    [SerializeField] private Button[] optionButtons = new Button[3];
    [SerializeField] private Text[] optionLabels = new Text[3];
    [SerializeField] private TMP_Text[] optionTmpLabels = new TMP_Text[3];

    private Action<UpgradeType> onSelected;
    private LevelUpOption[] currentOptions;
    private bool initialized;

    public static UpgradePopupUI CreateRuntimePopup()
    {
        // Runtime fallback lets the upgrade system work even if no UI was placed in the scene.
        EnsureEventSystem();

        GameObject canvasObject = new GameObject("RuntimeUpgradeCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject panelObject = new GameObject("UpgradePopup");
        panelObject.transform.SetParent(canvasObject.transform, false);
        Image panelImage = panelObject.AddComponent<Image>();
        panelImage.color = new Color(0.08f, 0.1f, 0.12f, 0.92f);

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(520f, 300f);

        Text title = CreateText("Title", panelObject.transform, "LEVEL UP - CHOOSE UPGRADE", 30, TextAnchor.MiddleCenter);
        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -20f);
        titleRect.sizeDelta = new Vector2(-40f, 48f);

        Button[] buttons = new Button[3];
        Text[] labels = new Text[3];
        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = CreateButton("UpgradeOption" + (i + 1), panelObject.transform);
            RectTransform buttonRect = button.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 1f);
            buttonRect.anchorMax = new Vector2(0.5f, 1f);
            buttonRect.pivot = new Vector2(0.5f, 1f);
            buttonRect.anchoredPosition = new Vector2(0f, -82f - i * 68f);
            buttonRect.sizeDelta = new Vector2(440f, 54f);

            buttons[i] = button;
            labels[i] = button.GetComponentInChildren<Text>(true);
        }

        UpgradePopupUI popup = panelObject.AddComponent<UpgradePopupUI>();
        popup.popupRoot = panelObject;
        popup.optionButtons = buttons;
        popup.optionLabels = labels;
        popup.Hide();
        return popup;
    }

    private void Awake()
    {
        EnsureInitialized();
        Hide();
    }

    public void Show(LevelUpOption[] options, Action<UpgradeType> selectedCallback)
    {
        // Rebuild button labels and callbacks each time a new level-up is offered.
        EnsureInitialized();
        currentOptions = options;
        onSelected = selectedCallback;

        AutoFillButtons();

        for (int i = 0; i < optionButtons.Length; i++)
        {
            int optionIndex = i;
            Button button = optionButtons[i];
            bool hasOption = currentOptions != null && optionIndex < currentOptions.Length;

            if (button == null)
            {
                continue;
            }

            button.gameObject.SetActive(hasOption);
            button.onClick.RemoveAllListeners();

            if (!hasOption)
            {
                continue;
            }

            SetButtonLabel(optionIndex, currentOptions[optionIndex]);
            button.onClick.AddListener(() => SelectOption(optionIndex));
        }

        popupRoot.SetActive(true);
    }

    public void Hide()
    {
        if (popupRoot == null)
        {
            popupRoot = gameObject;
        }

        popupRoot.SetActive(false);
    }

    private void EnsureInitialized()
    {
        if (initialized)
        {
            return;
        }

        if (popupRoot == null)
        {
            popupRoot = gameObject;
        }

        AutoFillButtons();
        initialized = true;
    }

    private void SelectOption(int index)
    {
        if (currentOptions == null || index < 0 || index >= currentOptions.Length)
        {
            return;
        }

        UpgradeType selectedType = currentOptions[index].Type;
        Hide();
        // The callback is owned by UpgradeManager, which applies the actual stat change.
        onSelected?.Invoke(selectedType);
    }

    private void AutoFillButtons()
    {
        // Supports both manually assigned UI buttons and the runtime-created popup.
        if (HasAssignedButton())
        {
            return;
        }

        Button[] foundButtons = GetComponentsInChildren<Button>(true);
        for (int i = 0; i < optionButtons.Length && i < foundButtons.Length; i++)
        {
            optionButtons[i] = foundButtons[i];
        }
    }

    private bool HasAssignedButton()
    {
        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (optionButtons[i] != null)
            {
                return true;
            }
        }

        return false;
    }

    private void SetButtonLabel(int index, LevelUpOption option)
    {
        string label = option.Title + "\n" + option.Description;

        Text uiText = GetLabel(optionLabels, optionButtons[index], index);
        if (uiText != null)
        {
            uiText.text = label;
        }

        TMP_Text tmpText = GetLabel(optionTmpLabels, optionButtons[index], index);
        if (tmpText != null)
        {
            tmpText.text = label;
        }
    }

    private static T GetLabel<T>(T[] assignedLabels, Button button, int index) where T : Component
    {
        if (assignedLabels != null && index < assignedLabels.Length && assignedLabels[index] != null)
        {
            return assignedLabels[index];
        }

        return button != null ? button.GetComponentInChildren<T>(true) : null;
    }

    private static Button CreateButton(string name, Transform parent)
    {
        GameObject buttonObject = new GameObject(name);
        buttonObject.transform.SetParent(parent, false);

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.22f, 0.72f, 0.42f, 1f);

        Button button = buttonObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.33f, 0.85f, 0.55f, 1f);
        colors.pressedColor = new Color(0.15f, 0.55f, 0.32f, 1f);
        button.colors = colors;

        Text label = CreateText("Label", buttonObject.transform, "", 18, TextAnchor.MiddleCenter);
        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(12f, 4f);
        labelRect.offsetMax = new Vector2(-12f, -4f);

        return button;
    }

    private static Text CreateText(string name, Transform parent, string text, int fontSize, TextAnchor alignment)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);

        Text label = textObject.AddComponent<Text>();
        label.text = text;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = fontSize;
        label.alignment = alignment;
        label.color = Color.white;
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Overflow;
        return label;
    }

    private static void EnsureEventSystem()
    {
        // Unity UI buttons need an EventSystem to receive clicks.
        if (FindObjectOfType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();
    }
}
