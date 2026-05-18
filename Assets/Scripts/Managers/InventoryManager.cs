using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    public InventoryLinkedList inventory = new InventoryLinkedList();

    [Header("UI del Inventario")]
    [Tooltip("Asigna aquí el Panel de UI que representa el inventario")]
    public GameObject panelInventario; 
    private bool estaAbierto = false;

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

        if (panelInventario != null)
        {
            panelInventario.SetActive(false);
        }

        inventory.PrintInventory();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            ToggleInventory();
        }
    }

    private void ToggleInventory()
    {
        estaAbierto = !estaAbierto;

        if (panelInventario != null)
        {
            panelInventario.SetActive(estaAbierto);
        }

        if (estaAbierto)
        {
            Debug.Log("Inventario Abierto");
            inventory.PrintInventory();
        }
        else
        {
            Debug.Log("Inventario Cerrado");
        }
    }

    // Esta función dibuja una interfaz básica en la pantalla automáticamente.
    // Así no tienes que configurar nada en el editor de Unity para que funcione.
    private void OnGUI()
    {
        if (estaAbierto && panelInventario == null)
        {
            // Dibujar un rectangulo centrado en la pantalla
            float width = 400;
            float height = 300;
            float x = (Screen.width - width) / 2;
            float y = (Screen.height - height) / 2;

            // Caja de fondo
            GUI.Box(new Rect(x, y, width, height), "INVENTARIO (Presiona Y para cerrar)");

            // Mostrar los items que están en tu LinkedList
            int line = 0;
            for (int i = 0; i < inventory.Count; i++)
            {
                InventoryItem item = inventory.GetItem(i);
                if (item != null)
                {
                    GUI.Label(new Rect(x + 20, y + 40 + (line * 30), 360, 25), $"- {item.itemName} (Cura: {item.healAmount} HP)");
                    line++;
                }
            }

            if (inventory.Count == 0)
            {
                GUI.Label(new Rect(x + 20, y + 40, 360, 25), "El inventario está vacío.");
            }
        }
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