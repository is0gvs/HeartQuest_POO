using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct ItemIconMapping
{
    public string itemName;
    public Texture2D icon;
}

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    [Header("Íconos de los Items")]
    public List<ItemIconMapping> itemIcons = new List<ItemIconMapping>();

    public InventoryLinkedList inventory = new InventoryLinkedList();

    private bool isInventoryOpen = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            // Solo destruimos el COMPONENTE InventoryManager, NO el GameObject entero.
            // Si el usuario puso este script en un GameObject importante (como el GameManager o uno con Audio),
            // usar Destroy(gameObject) mataría la música y la lógica del juego.
            Destroy(this);
        }
    }

    private bool wasLoaded = false;

    private void Start()
    {
        // --- DIAGNÓSTICO DE MÚSICA ---
        AudioSource[] allAudio = FindObjectsOfType<AudioSource>();
        Debug.Log($"[DIAGNÓSTICO DE AUDIO] Encontré {allAudio.Length} AudioSources en la escena.");
        foreach (var audio in allAudio)
        {
            Debug.Log($"   -> AudioSource en: {audio.gameObject.name} | Sonando: {audio.isPlaying} | Volumen: {audio.volume} | Clip: {(audio.clip != null ? audio.clip.name : "NULO")}");
            // Si hay un AudioSource que debería sonar y no está sonando, intentamos forzarlo:
            if (audio.playOnAwake && !audio.isPlaying && audio.clip != null)
            {
                audio.Play();
                Debug.Log($"      (FORZANDO REPRODUCCIÓN EN {audio.gameObject.name})");
            }
        }
        Debug.Log($"[DIAGNÓSTICO DE AUDIO] Volumen Global (AudioListener): {AudioListener.volume}");

        // Si el volumen global está en 0 por algún bug del menú, lo forzamos a 1
        if (AudioListener.volume <= 0.01f)
        {
            AudioListener.volume = 1f;
            Debug.Log("[DIAGNÓSTICO DE AUDIO] ¡El volumen global estaba en 0! Lo he subido a 1.");
        }
        // ------------------------------

        // Si wasLoaded es false, significa que el SaveManager NO nos cargó una partida
        // antes de que iniciara el Start(). Por lo tanto, es un Nuevo Juego.
        if (!wasLoaded && inventory.Count == 0)
        {
            inventory.AddItem(new InventoryItem("Lapiz", 5));
            inventory.AddItem(new InventoryItem("Carta", 10));
            inventory.AddItem(new InventoryItem("Cuaderno", 15));
            inventory.AddItem(new InventoryItem("Jugo", 3));
            inventory.AddItem(new InventoryItem("Sandwich", 8));
            inventory.AddItem(new InventoryItem("Pulsera", 12));
            inventory.AddItem(new InventoryItem("Nota", 7));
            inventory.AddItem(new InventoryItem("Dibujo", 10));
            inventory.AddItem(new InventoryItem("Calcomania", 4));

            // Guardamos inmediatamente para crear el archivo de esta nueva partida
            if (AntiBullyingGame.Managers.SaveManager.Instance != null)
            {
                AntiBullyingGame.Managers.SaveManager.Instance.SaveCurrentGameState();
            }
        }

        inventory.PrintInventory();
    }

    private void Update()
    {
        // Al presionar la letra 'Y', abrimos o cerramos el inventario
        if (Input.GetKeyDown(KeyCode.Y))
        {
            isInventoryOpen = !isInventoryOpen;
        }
    }

    private GUIStyle titleStyle;
    private GUIStyle slotStyle;
    private GUIStyle emptySlotStyle;
    private bool stylesInitialized = false;

    private void InitStyles()
    {
        if (stylesInitialized) return;

        titleStyle = new GUIStyle(GUI.skin.box);
        titleStyle.fontSize = 24;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = new Color(0.95f, 0.82f, 0.55f, 1f); // Dorado arena suave
        titleStyle.alignment = TextAnchor.UpperCenter;

        slotStyle = new GUIStyle(GUI.skin.button);
        slotStyle.fontSize = 13;
        slotStyle.fontStyle = FontStyle.Bold;
        slotStyle.normal.textColor = new Color(0.98f, 0.92f, 0.85f, 1f); // Crema pálido
        slotStyle.hover.textColor = new Color(0.95f, 0.82f, 0.55f, 1f);  // Dorado al pasar el mouse
        slotStyle.wordWrap = true;
        slotStyle.alignment = TextAnchor.LowerCenter;
        slotStyle.imagePosition = ImagePosition.ImageOnly;
        slotStyle.padding = new RectOffset(0, 0, 0, 0);

        emptySlotStyle = new GUIStyle(GUI.skin.box);

        stylesInitialized = true;
    }

    private Texture2D GetItemIcon(string itemName)
    {
        foreach (var mapping in itemIcons)
        {
            if (mapping.itemName == itemName)
                return mapping.icon;
        }
        return null;
    }

    private void OnGUI()
    {
        // Si el inventario está abierto, dibujamos la interfaz directamente en pantalla
        if (isInventoryOpen)
        {
            InitStyles();

            // Dimensiones de la ventana del inventario
            float width = 360;
            float height = 385;  // Más espacio entre filas para ver los números
            float x = (Screen.width - width) / 2;
            float y = (Screen.height - height) / 2;

            Color oldBgColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.2f, 0.1f, 0.05f, 0.95f);
            GUI.Box(new Rect(x, y, width, height), "\nINVENTARIO", titleStyle);
            GUI.backgroundColor = oldBgColor;

            // Configuración de la cuadrícula
            int columnas = 4;
            int totalSlots = 12; // 4x3
            float slotSize = 55;    // Íconos más pequeños para que quepa todo
            float colPadding = 10;
            float labelHeight = 28;
            float rowGap = 10;       // Más espacio entre filas para ver el texto
            float rowHeight = slotSize + labelHeight + rowGap;

            float gridWidth = (columnas * slotSize) + ((columnas - 1) * colPadding);
            float startX = x + (width - gridWidth) / 2;
            float startY = y + 58;

            // Estilo de etiqueta (creado una sola vez fuera del loop)
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 11;
            labelStyle.alignment = TextAnchor.UpperCenter;
            labelStyle.normal.textColor = new Color(0.98f, 0.92f, 0.85f, 1f);
            labelStyle.wordWrap = true;

            for (int i = 0; i < totalSlots; i++)
            {
                int filaActual = i / columnas;
                int colActual  = i % columnas;

                float slotX = startX + colActual * (slotSize + colPadding);
                float slotY = startY + filaActual * rowHeight;
                Rect slotRect = new Rect(slotX, slotY, slotSize, slotSize);

                InventoryItem item = inventory.GetItem(i);

                if (item != null)
                {
                    Texture2D icon = GetItemIcon(item.itemName);

                    // Botón invisible para capturar el clic
                    if (GUI.Button(slotRect, "", slotStyle))
                    {
                        UseItem(i);
                    }

                    // Ícono centrado dentro del slot con padding uniforme
                    if (icon != null)
                    {
                        float pad = 6f;
                        Rect iconRect = new Rect(
                            slotRect.x + pad,
                            slotRect.y + pad,
                            slotRect.width  - pad * 2,
                            slotRect.height - pad * 2
                        );
                        GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit, true);
                    }

                    // Nombre y HP debajo del slot
                    Rect labelRect = new Rect(slotX - 5, slotY + slotSize + 1, slotSize + 10, labelHeight);
                    GUI.Label(labelRect, $"{item.itemName}\n+{item.healAmount}", labelStyle);
                }
                else
                {
                    GUI.backgroundColor = new Color(0.1f, 0.05f, 0.02f, 0.6f);
                    GUI.Box(slotRect, "", emptySlotStyle);
                    GUI.backgroundColor = oldBgColor;
                }
            }

            // Texto de ayuda al fondo
            GUIStyle subStyle = new GUIStyle(GUI.skin.label);
            subStyle.fontSize = 11;
            subStyle.alignment = TextAnchor.UpperCenter;
            subStyle.normal.textColor = new Color(0.8f, 0.7f, 0.6f, 1f);
            GUI.Label(new Rect(x, y + height - 22, width, 20), "[ Presiona 'Y' para cerrar ]", subStyle);
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

            // Guardar automáticamente la partida en el archivo de persistencia
            if (AntiBullyingGame.Managers.SaveManager.Instance != null)
            {
                AntiBullyingGame.Managers.SaveManager.Instance.SaveCurrentGameState();
                Debug.Log("Inventario guardado en persistencia tras usar objeto.");
            }
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
        wasLoaded = true; // Marcamos que el inventario ha sido cargado desde una partida

        if (items == null)
        {
            inventory = new InventoryLinkedList();
            return;
        }

        inventory = new InventoryLinkedList();
        inventory.LoadFromSaveData(items);
    }

    // Reiniciar inventario para nueva partida
    public void ResetInventory()
    {
        wasLoaded = false;
        inventory = new InventoryLinkedList();
        inventory.AddItem(new InventoryItem("Lapiz", 5));
        inventory.AddItem(new InventoryItem("Carta", 10));
        inventory.AddItem(new InventoryItem("Cuaderno", 15));
        inventory.AddItem(new InventoryItem("Jugo", 3));
        inventory.AddItem(new InventoryItem("Sandwich", 8));
        inventory.AddItem(new InventoryItem("Pulsera", 12));
        inventory.AddItem(new InventoryItem("Nota", 7));
        inventory.AddItem(new InventoryItem("Dibujo", 10));
        inventory.AddItem(new InventoryItem("Calcomania", 4));
    }
}