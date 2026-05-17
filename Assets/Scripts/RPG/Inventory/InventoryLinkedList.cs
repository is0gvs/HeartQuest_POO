using System.Collections.Generic;
using UnityEngine;

public class InventoryLinkedList
{
    public InventoryNode head;
    public InventoryNode tail;

    public int Count { get; private set; }

    // Agregar item al final
    public void AddItem(InventoryItem item)
    {
        InventoryNode newNode = new InventoryNode(item);

        if (head == null)
        {
            head = newNode;
            tail = newNode;
        }
        else
        {
            tail.next = newNode;
            newNode.previous = tail;
            tail = newNode;
        }

        Count++;
    }

    // Eliminar item por nombre
    public void RemoveItem(string itemName)
    {
        InventoryNode current = head;

        while (current != null)
        {
            if (current.data.itemName == itemName)
            {
                if (current.previous != null)
                    current.previous.next = current.next;
                else
                    head = current.next;

                if (current.next != null)
                    current.next.previous = current.previous;
                else
                    tail = current.previous;

                Count--;
                return;
            }

            current = current.next;
        }
    }

    // Obtener item por índice
    public InventoryItem GetItem(int index)
    {
        if (index < 0 || index >= Count)
            return null;

        InventoryNode current = head;
        int currentIndex = 0;

        while (current != null)
        {
            if (currentIndex == index)
                return current.data;

            current = current.next;
            currentIndex++;
        }

        return null;
    }

    // Mostrar inventario en consola
    public void PrintInventory()
    {
        InventoryNode current = head;

        while (current != null)
        {
            Debug.Log($"Item: {current.data.itemName} | Heal: {current.data.healAmount}");
            current = current.next;
        }
    }

    // Convertir a datos de guardado
    public List<InventorySaveData> ToSaveData()
    {
        List<InventorySaveData> data = new List<InventorySaveData>();

        InventoryNode current = head;

        while (current != null)
        {
            data.Add(new InventorySaveData
            {
                itemName = current.data.itemName,
                healAmount = current.data.healAmount
            });

            current = current.next;
        }

        return data;
    }

    // Cargar desde datos de guardado (FIX aplicado)
    public void LoadFromSaveData(List<InventorySaveData> data)
    {
        head = null;
        tail = null;
        Count = 0;

        if (data == null)
            return;

        foreach (var item in data)
        {
            AddItem(new InventoryItem(item.itemName, item.healAmount));
        }
    }
}