using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public List<ItemButtons> buttons;
    [HideInInspector]
    public static ItemManager instance;
    void Awake() => instance = this;
    int maxSelectionInt;
    int minSelectionInt;
    public int selectionInt;
    public SpriteRenderer soul;
    public TextMeshPro useText;
    public GameObject itemObjects;
    public float time;
    public bool isMenu;
    public bool canAct = true;

    void Start()
    {
        maxSelectionInt = 3;
        minSelectionInt = 0;
        // Fix: same off-screen pivot issue as ActingText
        if (useText != null)
        {
            useText.rectTransform.pivot = new Vector2(0f, 0.5f);
            useText.rectTransform.sizeDelta = new Vector2(8.1f, 1.7f);
            useText.fontSize = 1.55f;
            useText.enableAutoSizing = false;
            useText.enableWordWrapping = true;
        }

        // ── Sincronizar ítems desde el inventario persistente ──
        SyncFromInventory();
    }

    /// <summary>
    /// Lee los ítems del InventoryManager (LinkedList persistente) y los
    /// escribe en los ItemButtons de la escena de batalla.
    /// Máximo 4 slots (la cantidad de botones en la UI).
    /// </summary>
    public void SyncFromInventory()
    {
        if (InventoryManager.instance == null)
        {
            Debug.LogWarning("[ItemManager] InventoryManager no encontrado. " +
                             "Los ítems de batalla quedarán con valores del Inspector.");
            return;
        }

        InventoryLinkedList inv = InventoryManager.instance.inventory;

        for (int i = 0; i < buttons.Count; i++)
        {
            InventoryItem item = inv.GetItem(i);

            if (item != null)
            {
                buttons[i].itemName = item.itemName;
                buttons[i].itemHeal = item.healAmount;
                buttons[i].isEmpty = false;
            }
            else
            {
                // Slot vacío — marcarlo como deshabilitado
                buttons[i].itemName = "";
                buttons[i].itemHeal = 0;
                buttons[i].isEmpty = true;
            }
        }

        RefreshButtonLabels();
        Debug.Log($"[ItemManager] Inventario sincronizado: {Mathf.Min(inv.Count, buttons.Count)} ítems cargados de {inv.Count} disponibles.");
    }

    /// <summary>
    /// Actualiza los TextMeshPro de cada botón para reflejar el nombre del ítem
    /// o "---" si el slot está vacío.
    /// </summary>
    private void RefreshButtonLabels()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            TextMeshPro label = buttons[i].GetComponentInChildren<TextMeshPro>();
            if (label == null) continue;

            if (buttons[i].isEmpty)
            {
                label.text = "---";
                label.color = new Color(0.4f, 0.4f, 0.4f, 1f); // Gris apagado
            }
            else
            {
                label.text = buttons[i].itemName;
                label.color = Color.white;
            }
        }
    }

    private bool axisInUse = false;

    void Update()
    {
        if (!BattleManager.battleInstance.isFighting && isMenu)
        {
            if (selectionInt > maxSelectionInt)
            {
                selectionInt = 0;
            }
            if (selectionInt < minSelectionInt)
            {
                selectionInt = 3;
            }

            // Joystick/Dpad Axis Navigation
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            float verticalInput = Input.GetAxisRaw("Vertical");

            bool leftPressed = Input.GetKeyDown(KeyCode.LeftArrow);
            bool rightPressed = Input.GetKeyDown(KeyCode.RightArrow);
            bool upPressed = Input.GetKeyDown(KeyCode.UpArrow);
            bool downPressed = Input.GetKeyDown(KeyCode.DownArrow);

            if (horizontalInput == 0f && verticalInput == 0f)
            {
                axisInUse = false;
            }
            else if (!axisInUse)
            {
                if (horizontalInput < -0.5f)
                {
                    leftPressed = true;
                    axisInUse = true;
                }
                else if (horizontalInput > 0.5f)
                {
                    rightPressed = true;
                    axisInUse = true;
                }
                else if (verticalInput < -0.5f)
                {
                    downPressed = true;
                    axisInUse = true;
                }
                else if (verticalInput > 0.5f)
                {
                    upPressed = true;
                    axisInUse = true;
                }
            }

            if (leftPressed)
            {
                selectionInt--;
            }
            if (rightPressed)
            {
                selectionInt++;
            }
            if (upPressed)
            {
                selectionInt -= 2;
            }
            if (downPressed)
            {
                selectionInt += 2;
            }
            Selection();
            time += Time.deltaTime;
            if (time > 0.25f)
            {
                if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.JoystickButton1))
                {
                    if (canAct)
                    {
                        BattleManager.battleInstance.CloseItemMenu();
                        return;
                    }
                }
                if (canAct && DialogueManager.instance.done)
                {
                    if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.JoystickButton0))
                    {
                        canAct = false;
                        Selected();
                    }
                }
            }
        }
    }

    void Selecting(int selectedInt)
    {
        if (buttons[selectedInt].selected)
        {
            if (soul != null) soul.enabled = false;
            SetButtonVisual(selectedInt, true);
        }
    }
    void Deselecting(int deselectionInt)
    {
        buttons[deselectionInt].selected = false;
        SetButtonVisual(deselectionInt, false);
    }

    private void SetButtonVisual(int index, bool selected)
    {
        if (index < 0 || index >= buttons.Count || buttons[index] == null) return;

        Transform option = buttons[index].transform;
        // Resaltado por contorno/halo: NO se altera el color base del botón.
        option.localScale = selected ? Vector3.one * 1.1f : Vector3.one;

        BattleSelectionHalo.Apply(option, selected);

        TextMeshPro label = option.GetComponentInChildren<TextMeshPro>();
        if (label != null)
        {
            if (buttons[index].isEmpty)
            {
                label.color = new Color(0.4f, 0.4f, 0.4f, 1f);
                label.fontStyle = FontStyles.Normal;
            }
            else
            {
                label.color = Color.white;
                label.fontStyle = selected ? FontStyles.Bold : FontStyles.Normal;
            }
        }
    }
    void Selection()
    {

        if (selectionInt == 0)
        {
            buttons[selectionInt].selected = true;
            Selecting(0);
        }
        else
        {
            Deselecting(0);
        }
        if (selectionInt == 1)
        {
            buttons[selectionInt].selected = true;
            Selecting(1);
        }
        else
        {
            Deselecting(1);
        }
        if (selectionInt == 2)
        {
            buttons[selectionInt].selected = true;
            Selecting(2);
        }
        else
        {
            Deselecting(2);
        }
        if (selectionInt == 3)
        {
            buttons[selectionInt].selected = true;
            Selecting(3);
        }
        else
        {
            Deselecting(3);
        }
    }

    void Selected()
    {
        // ── Bloquear si el slot está vacío ──
        if (selectionInt >= 0 && selectionInt < buttons.Count && buttons[selectionInt].isEmpty)
        {
            // Mostrar mensaje de slot vacío y volver a permitir selección
            if (useText != null)
            {
                useText.gameObject.SetActive(true);
                useText.text = "* No tienes nada en ese espacio.";
            }
            canAct = true;
            return;
        }

        // ── Bloquear si la vida está al máximo ──
        if (PlayerVars.instance != null && PlayerVars.instance.health >= PlayerVars.instance.maxHealth)
        {
            if (useText != null)
            {
                useText.gameObject.SetActive(true);
                useText.text = "* Tu HP ya está al máximo.";
            }
            canAct = true;
            return;
        }

        UseSlot(selectionInt);
    }

    /// <summary>
    /// Usa el ítem del slot indicado: cura, muestra diálogo, remueve del inventario,
    /// y lanza la secuencia de turno enemigo.
    /// </summary>
    private void UseSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= buttons.Count) return;

        ItemButtons btn = buttons[slotIndex];

        // Curar al jugador
        if (PlayerVars.instance != null && PlayerVars.instance.health < PlayerVars.instance.maxHealth)
        {
            PlayerVars.instance.Heal(btn.itemHeal);
        }

        soul.enabled = false;

        // Diálogo
        DialogueManager.instance.dialogueTxt = $"* Usaste {btn.itemName}. Recuperaste {btn.itemHeal} HP.";
        DialogueManager.instance.text.gameObject.SetActive(true);
        DialogueManager.instance.enemyTxt = BattleManager.battleInstance.enemyDialogue[
            UnityEngine.Random.Range(0, BattleManager.battleInstance.enemyDialogue.Count)];
        DialogueManager.instance.shouldTalk = true;

        Action dialogue = () =>
        {
            DialogueManager.instance.shouldTalk = false;
            StartCoroutine(BattleManager.battleInstance.ItemSequence());
        };
        DialogueManager.instance.Talking(dialogue);
        itemObjects.SetActive(false);

        // ── Remover del inventario persistente ──
        RemoveFromInventory(btn.itemName);

        // Marcar este slot como vacío
        btn.isEmpty = true;
        btn.itemName = "";
        btn.itemHeal = 0;

        // Actualizar etiquetas visuales
        RefreshButtonLabels();
    }

    /// <summary>
    /// Remueve un ítem del InventoryManager persistente y guarda automáticamente.
    /// </summary>
    private void RemoveFromInventory(string itemName)
    {
        if (InventoryManager.instance == null) return;

        InventoryManager.instance.inventory.RemoveItem(itemName);
        Debug.Log($"[ItemManager] '{itemName}' removido del inventario persistente. Quedan {InventoryManager.instance.inventory.Count} ítems.");

        // Auto-guardar la partida
        if (AntiBullyingGame.Managers.SaveManager.Instance != null)
        {
            AntiBullyingGame.Managers.SaveManager.Instance.SaveCurrentGameState();
        }
    }
}
