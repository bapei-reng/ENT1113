using System;
using UnityEngine;

public class RepairMachine : MonoBehaviour
{
    public const int StageCount = 3;

    [SerializeField] private float interactionRadius = 2.1f;
    [SerializeField] private float stageDuration = 15f;
    [SerializeField] private Renderer[] stageLights = new Renderer[StageCount];

    private int completedStages;
    private bool stageUnlocked;
    private float stageTime;

    public event Action StateChanged;

    public int CompletedStages => completedStages;
    public bool IsRepaired => completedStages >= StageCount;
    public bool StageUnlocked => stageUnlocked;
    public float StageDuration => stageDuration;
    public float StageTime => stageTime;

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
        message = "第 " + (completedStages + 1) + " 段已解锁，按住 E 破译";
        StateChanged?.Invoke();
        return true;
    }

    public bool Advance(float deltaTime)
    {
        if (!stageUnlocked || IsRepaired || deltaTime <= 0f)
            return false;

        stageTime = Mathf.Min(stageDuration, stageTime + deltaTime);
        if (stageTime < stageDuration)
        {
            StateChanged?.Invoke();
            return false;
        }

        stageUnlocked = false;
        stageTime = 0f;
        completedStages++;
        if (completedStages - 1 < stageLights.Length && stageLights[completedStages - 1] != null)
            stageLights[completedStages - 1].material.color = new Color(0.1f, 1f, 0.35f);
        StateChanged?.Invoke();
        return true;
    }
}
