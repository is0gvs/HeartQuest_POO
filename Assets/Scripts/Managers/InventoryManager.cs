using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    public InventoryLinkedList inventory = new InventoryLinkedList();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Items iniciales
        if (inventory.Count == 0)
        {
            inventory.AddItem(new InventoryItem("Lapiz", 5));
            inventory.AddItem(new InventoryItem("Carta", 10));
            inventory.AddItem(new InventoryItem("Cuaderno", 15));
        }

        inventory.PrintInventory();
    }

    public void UseItem(int index)
    {
        InventoryItem item = inventory.GetItem(index);

        if (item != null)
        {
            Debug.Log($"Usaste {item.itemName} y curaste {item.healAmount} HP");

            if (PlayerVars.instance != null)
            {
                PlayerVars.instance.Heal(item.healAmount);
            }

            inventory.RemoveItem(item.itemName);
        }
    }

    //Convertir inventario a SaveData
    public List<InventorySaveData> GetInventoryData()
    {
        return inventory.ToSaveData();
    }

    //Importar inventario
    public void LoadInventory(List<InventorySaveData> items)
    {
        if (items == null)
        {
            inventory = new InventoryLinkedList();
            return;
        }

        inventory = new InventoryLinkedList();
        inventory.LoadFromSaveData(items);
    }
}