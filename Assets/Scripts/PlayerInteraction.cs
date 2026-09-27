using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private RepairMachine machine;
    [SerializeField] private GameHUD hud;
    [SerializeField] private float pickupRadius = 1.4f;

    private readonly Collider[] nearbyColliders = new Collider[128];

    private bool isRepairing;

    public bool IsRepairing => isRepairing;

    private void Start()
    {
        pickupRadius += PlayerUpgrades.PickupRangeBonus;
    }

    private void Update()
    {
        isRepairing = false;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
            return;
        }

        if (inventory == null)
            return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) inventory.SelectSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) inventory.SelectSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) inventory.SelectSlot(2);

        bool atMachine = machine != null && machine.IsInRange(transform);
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (atMachine)
            {
                machine.TryInsertKey(inventory, out string message);
                hud?.ShowMessage(message);
            }
            else if (inventory.SelectedItem != null && inventory.SelectedItem.IsKey)
            {
                hud?.ShowMessage("靠近机器才能使用钥匙");
            }
        }

        WorldPickup pickup = FindNearestPickup();
        if (pickup != null && Input.GetKeyDown(KeyCode.E))
        {
            if (pickup.TryPickup(inventory))
                hud?.ShowMessage("拾取了 " + pickup.Item.DisplayName);
            else
                hud?.ShowMessage("背包已满");
            return;
        }

        if (atMachine && machine.StageUnlocked && Input.GetKey(KeyCode.E))
        {
            isRepairing = true;
            ReadRepairKeyInput();
            if (machine.Advance(Time.deltaTime))
                hud?.ShowMessage(machine.IsRepaired ? "机器已修好，出口已解锁" : "本段破译完成，可以插入下一把钥匙");
        }

        if (hud == null)
            return;

        if (pickup != null)
            hud.SetPrompt("E  拾取 " + pickup.Item.DisplayName);
        else if (isRepairing)
            hud.SetPrompt("按住 E 破译｜按 " + KeyLabel(machine.RequiredKey) + " 按键 ×" +
                          FormatMultiplier(machine.RepairSpeed) + "（" +
                          FormatMultiplier(machine.KeyMultiplierMin) + "~" +
                          FormatMultiplier(machine.KeyMultiplierMax) + "）｜按错减速");
        else if (atMachine && machine.StageUnlocked)
            hud.SetPrompt("按住 E 破译，按提示方向键加速，按错减速");
        else if (atMachine && !machine.IsRepaired)
            hud.SetPrompt("切换到钥匙，按 R 插入");
        else
            hud.SetPrompt(string.Empty);
    }

    private WorldPickup FindNearestPickup()
    {
        Vector3 center = transform.position + Vector3.up * 0.7f;
        int count = Physics.OverlapSphereNonAlloc(center, pickupRadius, nearbyColliders,
            ~0, QueryTriggerInteraction.Collide);
        WorldPickup nearest = null;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            WorldPickup pickup = nearbyColliders[i].GetComponentInParent<WorldPickup>();
            if (pickup == null || pickup.Item == null)
                continue;

            float distance = (pickup.transform.position - transform.position).sqrMagnitude;
            if (distance >= bestDistance)
                continue;

            nearest = pickup;
            bestDistance = distance;
        }

        return nearest;
    }

    private void ReadRepairKeyInput()
    {
        for (int i = 0; i < RepairMachine.RepairKeys.Length; i++)
        {
            if (Input.GetKeyDown(RepairMachine.RepairKeys[i]))
            {
                machine.SubmitRepairKey(RepairMachine.RepairKeys[i]);
                return;
            }
        }
    }

    private static string KeyLabel(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.UpArrow: return "↑";
            case KeyCode.DownArrow: return "↓";
            case KeyCode.LeftArrow: return "←";
            default: return "→";
        }
    }

    private static string FormatMultiplier(float value)
    {
        if (value >= 100f)
            return value.ToString("0");
        if (value >= 10f)
            return value.ToString("0.0");

        return value.ToString("0.00");
    }
}
