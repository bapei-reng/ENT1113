using UnityEngine;

[CreateAssetMenu(menuName = "John Lemon/Inventory Item")]
public class InventoryItem : ScriptableObject
{
    [SerializeField] private string displayName = "物品";
    [SerializeField] private bool isKey;
    [SerializeField] private GameObject handPrefab;
    [SerializeField] private Sprite icon;

    public string DisplayName => displayName;
    public bool IsKey => isKey;
    public GameObject HandPrefab => handPrefab;
    public Sprite Icon => icon;
}
