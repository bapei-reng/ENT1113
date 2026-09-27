using UnityEngine;
using UnityEngine.UI;

public class GameHUD : MonoBehaviour
{
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private RepairMachine machine;
    [SerializeField] private Text[] slotTexts = new Text[PlayerInventory.Capacity];
    [SerializeField] private Image[] slotFrames = new Image[PlayerInventory.Capacity];
    [SerializeField] private Outline[] slotHighlights = new Outline[PlayerInventory.Capacity];
    [SerializeField] private Image[] slotIcons = new Image[PlayerInventory.Capacity];
    [SerializeField] private Text stageText;
    [SerializeField] private Text promptText;
    [SerializeField] private Text messageText;
    [SerializeField] private Slider progressBar;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Text sprintText;

    private float messageUntil;
    private string currentPrompt;
    private bool sprintReadyShown;
    private int sprintCooldownTenths = -1;
    private bool sprintActiveShown;
    private int sprintActiveTenths = -1;

    private GameObject upgradeCanvas;
    private GameObject upgradePanel;
    private Text upgradeHeaderText;
    private Text upgradeBodyText;
    private Text upgradeFooterText;
    private UpgradeOption[] upgradeOptions = new UpgradeOption[0];
    private int upgradeHighlight;

    public bool UpgradeChoiceVisible => upgradePanel != null && upgradePanel.activeSelf;

    private void OnEnable()
    {
        if (inventory != null)
            inventory.Changed += RefreshInventory;
        if (machine != null)
            machine.StateChanged += RefreshMachine;
        RefreshInventory();
        RefreshMachine();
    }

    private void OnDisable()
    {
        if (inventory != null)
            inventory.Changed -= RefreshInventory;
        if (machine != null)
            machine.StateChanged -= RefreshMachine;
    }

    private void Update()
    {
        if (messageText != null && messageText.enabled && Time.time >= messageUntil)
            messageText.enabled = false;

        RefreshSprint();
    }

    private void RefreshSprint()
    {
        if (playerMovement == null || sprintText == null)
            return;

        if (playerMovement.IsSprinting)
        {
            int activeTenths = Mathf.CeilToInt(playerMovement.SprintRemaining * 10f);
            if (sprintActiveShown && activeTenths == sprintActiveTenths)
                return;

            sprintActiveShown = true;
            sprintReadyShown = false;
            sprintCooldownTenths = -1;
            sprintActiveTenths = activeTenths;
            sprintText.text = "Shift 冲刺：×" + playerMovement.SpeedMultiplier.ToString("0.0") +
                              " 加速中 " + (activeTenths / 10f).ToString("0.0") + " 秒";
            sprintText.color = new Color(1f, 0.86f, 0.42f);
            return;
        }

        sprintActiveShown = false;

        if (playerMovement.IsSprintReady)
        {
            if (sprintReadyShown)
                return;

            sprintReadyShown = true;
            sprintCooldownTenths = -1;
            sprintText.text = "Shift 冲刺：就绪";
            sprintText.color = new Color(0.55f, 1f, 0.6f);
            return;
        }

        int tenths = Mathf.CeilToInt(playerMovement.SprintCooldownRemaining * 10f);
        if (!sprintReadyShown && tenths == sprintCooldownTenths)
            return;

        sprintReadyShown = false;
        sprintCooldownTenths = tenths;
        sprintText.text = "Shift 冲刺：冷却 " + (tenths / 10f).ToString("0.0") + " 秒";
        sprintText.color = new Color(1f, 1f, 1f, 0.8f);
    }

    public void SetPrompt(string prompt)
    {
        if (promptText == null || prompt == currentPrompt)
            return;

        currentPrompt = prompt;
        promptText.text = prompt;
    }

    public void ShowMessage(string message, float seconds = 2.5f)
    {
        if (messageText == null)
            return;

        messageText.text = message;
        messageText.enabled = true;
        messageUntil = Time.time + seconds;
    }

    public void ShowUpgradeChoice(UpgradeOption[] options)
    {
        upgradeOptions = options ?? new UpgradeOption[0];
        upgradeHighlight = 0;

        EnsureUpgradePanel();
        if (upgradePanel == null)
            return;

        upgradePanel.SetActive(true);
        if (upgradeHeaderText != null)
            upgradeHeaderText.text = "第 " + PlayerUpgrades.Level + " 关通过 — 选择一项加成";
        if (upgradeFooterText != null)
        {
            string hint = PlayerUpgrades.NextLevelHint();
            upgradeFooterText.text =
                (string.IsNullOrEmpty(hint) ? string.Empty : hint + "\n") +
                "本局加成：" + PlayerUpgrades.DescribeBonuses() + "\n" +
                "← / → 切换    J / K / L 选择    Enter 确认";
        }

        RefreshUpgradeBody();
    }

    public void ShowUpgradeResult(string chosenLabel, string enemyNote)
    {
        if (upgradePanel == null)
            return;

        if (upgradeHeaderText != null)
            upgradeHeaderText.text = "已选择加成";
        if (upgradeBodyText != null)
        {
            string text = "<color=#FFD24C>" + chosenLabel + "</color>";
            if (!string.IsNullOrEmpty(enemyNote))
                text += "\n\n<color=#FF7A6B>敌人加强：" + enemyNote + "</color>";
            upgradeBodyText.text = text;
        }
        if (upgradeFooterText != null)
            upgradeFooterText.text = "进入下一关…";
    }

    public void HideUpgradeChoice()
    {
        if (upgradePanel != null)
            upgradePanel.SetActive(false);
    }

    public int PollUpgradeChoice()
    {
        if (!UpgradeChoiceVisible || upgradeOptions.Length == 0)
            return -1;

        int chosen = -1;
        if (Input.GetKeyDown(KeyCode.J))
            chosen = 0;
        else if (Input.GetKeyDown(KeyCode.K))
            chosen = 1;
        else if (Input.GetKeyDown(KeyCode.L))
            chosen = 2;

        if (chosen >= 0)
        {
            upgradeHighlight = Mathf.Clamp(chosen, 0, upgradeOptions.Length - 1);
            RefreshUpgradeBody();
            return upgradeHighlight;
        }

        int previous = upgradeHighlight;
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            upgradeHighlight = (upgradeHighlight + upgradeOptions.Length - 1) % upgradeOptions.Length;
        else if (Input.GetKeyDown(KeyCode.RightArrow))
            upgradeHighlight = (upgradeHighlight + 1) % upgradeOptions.Length;

        if (upgradeHighlight != previous)
            RefreshUpgradeBody();

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            return upgradeHighlight;

        return -1;
    }

    private void RefreshUpgradeBody()
    {
        if (upgradeBodyText == null)
            return;

        string[] keys = { "J", "K", "L" };
        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        for (int i = 0; i < upgradeOptions.Length; i++)
        {
            string key = i < keys.Length ? keys[i] : (i + 1).ToString();
            string line = (i == upgradeHighlight ? "> " : "   ") + "[" + key + "]  " + upgradeOptions[i].Label;
            builder.Append(i == upgradeHighlight ? "<color=#FFD24C>" + line + "</color>" : line);
            if (i < upgradeOptions.Length - 1)
                builder.Append('\n');
        }

        upgradeBodyText.text = builder.ToString();
    }

    private void EnsureUpgradePanel()
    {
        if (upgradePanel != null)
            return;

        Font font = ResolveFont();

        upgradeCanvas = new GameObject("UpgradeCanvas", typeof(Canvas), typeof(CanvasScaler));
        Canvas canvas = upgradeCanvas.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        CanvasScaler scaler = upgradeCanvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0f;

        upgradePanel = CreatePanelObject("UpgradePanel", upgradeCanvas.transform,
            new Vector2(1180f, 470f), Vector2.zero);
        Image background = upgradePanel.AddComponent<Image>();
        background.color = new Color(0.03f, 0.04f, 0.06f, 0.9f);
        background.raycastTarget = false;

        upgradeHeaderText = CreatePanelText("Header", upgradePanel.transform, font, 36,
            new Vector2(1080f, 60f), new Vector2(0f, 186f), new Color(1f, 0.83f, 0.32f));
        upgradeBodyText = CreatePanelText("Body", upgradePanel.transform, font, 30,
            new Vector2(1080f, 220f), new Vector2(0f, 40f), Color.white);
        upgradeFooterText = CreatePanelText("Footer", upgradePanel.transform, font, 21,
            new Vector2(1100f, 150f), new Vector2(0f, -168f), new Color(0.85f, 0.88f, 0.92f));

        upgradePanel.SetActive(false);
    }

    private Font ResolveFont()
    {
        if (messageText != null && messageText.font != null)
            return messageText.font;
        if (sprintText != null && sprintText.font != null)
            return sprintText.font;
        if (stageText != null && stageText.font != null)
            return stageText.font;
        if (promptText != null && promptText.font != null)
            return promptText.font;

        return null;
    }

    private static GameObject CreatePanelObject(string name, Transform parent, Vector2 size, Vector2 position)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform));
        panel.transform.SetParent(parent, false);

        RectTransform rect = (RectTransform)panel.transform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        return panel;
    }

    private static Text CreatePanelText(string name, Transform parent, Font font, int fontSize,
        Vector2 size, Vector2 position, Color color)
    {
        GameObject obj = CreatePanelObject(name, parent, size, position);
        Text text = obj.AddComponent<Text>();
        text.font = font;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAnchor.MiddleCenter;
        text.supportRichText = true;
        text.raycastTarget = false;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.lineSpacing = 1.15f;
        return text;
    }

    private void RefreshInventory()
    {
        if (inventory == null)
            return;

        for (int i = 0; i < PlayerInventory.Capacity; i++)
        {
            InventoryItem item = inventory.GetItem(i);
            bool selected = i == inventory.SelectedSlot;
            if (i < slotTexts.Length && slotTexts[i] != null)
            {
                slotTexts[i].text = (i + 1).ToString();
                slotTexts[i].color = selected ? new Color(1f, 0.86f, 0.42f) : Color.white;
            }
            if (i < slotFrames.Length && slotFrames[i] != null)
                slotFrames[i].color = selected
                    ? new Color(0.91f, 0.55f, 0.13f, 1f)
                    : new Color(0.27f, 0.29f, 0.31f, 0.93f);
            if (i < slotHighlights.Length && slotHighlights[i] != null)
                slotHighlights[i].enabled = selected;
            if (i < slotIcons.Length && slotIcons[i] != null)
            {
                slotIcons[i].sprite = item != null ? item.Icon : null;
                slotIcons[i].enabled = slotIcons[i].sprite != null;
            }
        }
    }

    private void RefreshMachine()
    {
        if (machine == null)
            return;

        if (stageText != null)
            stageText.text = "第 " + PlayerUpgrades.Level + " 关｜" + (machine.IsRepaired
                ? "机器已修好，前往出口"
                : "破译 " + machine.CompletedStages + "/" + RepairMachine.StageCount +
                  (machine.StageUnlocked ? "  第 " + (machine.CompletedStages + 1) + " 段" : ""));

        if (progressBar != null)
        {
            progressBar.value = machine.StageUnlocked
                ? machine.StageTime / machine.StageDuration
                : machine.IsRepaired ? 1f : 0f;
        }
    }
}
