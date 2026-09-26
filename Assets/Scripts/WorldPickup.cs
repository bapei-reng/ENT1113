using UnityEngine;

public class WorldPickup : MonoBehaviour
{
    [SerializeField] private InventoryItem item;
    [SerializeField] private float turnSpeed = 55f;

    public InventoryItem Item => item;

    public bool TryPickup(PlayerInventory inventory)
    {
        if (inventory == null || !inventory.TryAdd(item))
            return false;

        Destroy(gameObject);
        return true;
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, turnSpeed * Time.deltaTime, Space.World);
    }
}
