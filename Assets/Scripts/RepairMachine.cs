using System;
using UnityEngine;

public class RepairMachine : MonoBehaviour
{
    public const int StageCount = 3;

    public static readonly KeyCode[] RepairKeys =
    {
        KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow
    };

    [SerializeField] private float interactionRadius = 1.5f;
    [SerializeField] private float stageDuration = 15f;
    [SerializeField] private float correctPressMultiplier = 1.35f;
    [SerializeField] private float wrongPressMultiplier = 0.78f;
    [SerializeField] private float minSpeedMultiplier = 0.5f;
    [SerializeField] private float maxSpeedMultiplier = 4f;
    [SerializeField] private float speedReturnPerSecond = 1.2f;
    [SerializeField] private Renderer[] stageLights = new Renderer[StageCount];

    private int completedStages;
    private bool stageUnlocked;
    private float stageTime;
    private float repairSpeed = 1f;
    private KeyCode requiredKey = KeyCode.UpArrow;

    public event Action StateChanged;

    public int CompletedStages => completedStages;
    public bool IsRepaired => completedStages >= StageCount;
    public bool StageUnlocked => stageUnlocked;
    public float StageDuration => stageDuration;
    public float StageTime => stageTime;
    public float RepairSpeed => repairSpeed;
    public KeyCode RequiredKey => requiredKey;

    private void Update()
    {
        if (Mathf.Approximately(repairSpeed, 1f))
        {
            repairSpeed = 1f;
            return;
        }

        repairSpeed = Mathf.Lerp(repairSpeed, 1f, 1f - Mathf.Exp(-speedReturnPerSecond * Time.deltaTime));
    }

    public bool IsInRange(Transform player)
    {
        return player != null &&
               (player.position - transform.position).sqrMagnitude <= interactionRadius * interactionRadius;
    }

    public bool TryInsertKey(PlayerInventory inventory, out string message)
    {
        if (IsRepaired)
        {
            message = "机器已经修好了";
            return false;
        }

        if (stageUnlocked)
        {
            message = "请先完成当前一段破译";
            return false;
        }

        if (inventory == null || !inventory.TryConsumeSelectedKey())
        {
            message = "请先把钥匙切换到主手";
            return false;
        }

        stageUnlocked = true;
        stageTime = 0f;
        repairSpeed = 1f;
        requiredKey = PickNextRepairKey();
        message = "第 " + (completedStages + 1) + " 段已解锁，按住 E 破译，按提示方向键加速，按错减速";
        StateChanged?.Invoke();
        return true;
    }

    public bool SubmitRepairKey(KeyCode key)
    {
        if (!stageUnlocked || IsRepaired)
            return false;

        if (key == requiredKey)
        {
            repairSpeed = Mathf.Min(maxSpeedMultiplier, repairSpeed * correctPressMultiplier);
            requiredKey = PickNextRepairKey();
            return true;
        }

        repairSpeed = Mathf.Max(minSpeedMultiplier, repairSpeed * wrongPressMultiplier);
        return false;
    }

    private KeyCode PickNextRepairKey()
    {
        int current = Array.IndexOf(RepairKeys, requiredKey);
        int index = UnityEngine.Random.Range(0, RepairKeys.Length - 1);
        if (index >= current)
            index++;
        return RepairKeys[index];
    }

    public bool Advance(float deltaTime)
    {
        if (!stageUnlocked || IsRepaired || deltaTime <= 0f)
            return false;

        stageTime = Mathf.Min(stageDuration, stageTime + deltaTime * repairSpeed);
        if (stageTime < stageDuration)
        {
            StateChanged?.Invoke();
            return false;
        }

        stageUnlocked = false;
        stageTime = 0f;
        repairSpeed = 1f;
        completedStages++;
        if (completedStages - 1 < stageLights.Length && stageLights[completedStages - 1] != null)
            stageLights[completedStages - 1].material.color = new Color(0.1f, 1f, 0.35f);
        StateChanged?.Invoke();
        return true;
    }
}
