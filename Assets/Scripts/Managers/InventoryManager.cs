using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    private InventoryItem head;
    private InventoryItem tail;
    private InventoryItem current;

    // Agregar item
    public void AddItem(string itemName)
    {
        InventoryItem newItem = new InventoryItem(itemName);

        if (head == null)
        {
            head = newItem;
            tail = newItem;
            current = newItem;
        }
        else
        {
            tail.next = newItem;
            newItem.previous = tail;
            tail = newItem;
        }

        Debug.Log("Item agregado: " + itemName);
    }

    // Mostrar item actual
    public void ShowCurrentItem()
    {
        if (current != null)
        {
            Debug.Log("Item actual: " + current.itemName);
        }
    }

    // Siguiente item
    public void NextItem()
    {
        if (current != null && current.next != null)
        {
            current = current.next;
            ShowCurrentItem();
        }
    }

    // Item anterior
    public void PreviousItem()
    {
        if (current != null && current.previous != null)
        {
            current = current.previous;
            ShowCurrentItem();
        }
    }
}