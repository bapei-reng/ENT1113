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

    private float messageUntil;
    private string currentPrompt;

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
            stageText.text = machine.IsRepaired
                ? "机器已修好，前往出口"
                : "破译 " + machine.CompletedStages + "/" + RepairMachine.StageCount +
                  (machine.StageUnlocked ? "  第 " + (machine.CompletedStages + 1) + " 段" : "");

        if (progressBar != null)
        {
            progressBar.value = machine.StageUnlocked
                ? machine.StageTime / machine.StageDuration
                : machine.IsRepaired ? 1f : 0f;
        }
    }
}
