using UnityEngine;
using AntiBullyingGame.Interfaces;

public class ItemPickup : MonoBehaviour, IInteractable
{
    public string itemName;
    public int healAmount;

    public void Interact()
    {
        if (InventoryManager.instance != null)
        {
            InventoryManager.instance.inventory.AddItem(new InventoryItem(itemName, healAmount));
            Debug.Log($"Recogiste: {itemName}");
            Destroy(gameObject);
        }
    }
}
