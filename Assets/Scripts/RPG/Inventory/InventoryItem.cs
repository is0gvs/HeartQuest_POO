using UnityEngine;

[System.Serializable]
public class InventoryItem
{
    public string itemName;
    public int healAmount;

    public InventoryItem(string name, int heal)
    {
        itemName = name;
        healAmount = heal;
    }
}