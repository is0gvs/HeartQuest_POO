public class InventoryNode
{
    public InventoryItem data;

    public InventoryNode next;
    public InventoryNode previous;

    public InventoryNode(InventoryItem item)
    {
        data = item;
        next = null;
        previous = null;
    }
}