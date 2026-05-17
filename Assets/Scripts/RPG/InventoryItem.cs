[System.Serializable]
public class InventoryItem
{
    public string itemName;

    public InventoryItem next;
    public InventoryItem previous;

    public InventoryItem(string name)
    {
        itemName = name;
        next = null;
        previous = null;
    }
}