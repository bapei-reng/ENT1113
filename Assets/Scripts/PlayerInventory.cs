using System;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public const int Capacity = 3;

    [SerializeField] private Transform handMount;

    private readonly InventoryItem[] slots = new InventoryItem[Capacity];
    private GameObject handVisual;
    private int selectedSlot;

    public event Action Changed;

    public int SelectedSlot => selectedSlot;
    public InventoryItem SelectedItem => slots[selectedSlot];

    public InventoryItem GetItem(int index)
    {
        return index >= 0 && index < Capacity ? slots[index] : null;
    }

    public bool TryAdd(InventoryItem item)
    {
        if (item == null)
            return false;

        for (int i = 0; i < Capacity; i++)
        {
            if (slots[i] != null)
                continue;

            bool hadSelectedItem = SelectedItem != null;
            slots[i] = item;
            if (!hadSelectedItem)
                selectedSlot = i;
            RefreshHand();
            Changed?.Invoke();
            return true;
        }

        return false;
    }

    public void SelectSlot(int index)
    {
        if (index < 0 || index >= Capacity || index == selectedSlot)
            return;

        selectedSlot = index;
        RefreshHand();
        Changed?.Invoke();
    }

    public bool TryConsumeSelectedKey()
    {
        if (SelectedItem == null || !SelectedItem.IsKey)
            return false;

        slots[selectedSlot] = null;
        RefreshHand();
        Changed?.Invoke();
        return true;
    }

    private void RefreshHand()
    {
        if (handVisual != null)
            Destroy(handVisual);

        InventoryItem item = SelectedItem;
        if (item == null || item.HandPrefab == null || handMount == null)
            return;

        handVisual = Instantiate(item.HandPrefab, handMount);
        handVisual.name = "Held " + item.DisplayName;
        handVisual.transform.localPosition = new Vector3(0f, 0f, 0.08f);
        handVisual.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        handVisual.transform.localScale = Vector3.one * 0.45f;
    }
}
