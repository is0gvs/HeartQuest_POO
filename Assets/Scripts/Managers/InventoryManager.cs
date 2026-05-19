using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

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
        foreach(var audio in allAudio) 
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
        slotStyle.alignment = TextAnchor.MiddleCenter;

        emptySlotStyle = new GUIStyle(GUI.skin.box);

        stylesInitialized = true;
    }

    private void OnGUI()
    {
        // Si el inventario está abierto, dibujamos la interfaz directamente en pantalla
        if (isInventoryOpen)
        {
            InitStyles();

            // Dimensiones de la ventana del inventario
            float width = 360;
            float height = 300;
            float x = (Screen.width - width) / 2;
            float y = (Screen.height - height) / 2;

            // Cambiar color del fondo para que use el tono tierra oscuro del MainMenu
            Color oldBgColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.2f, 0.1f, 0.05f, 0.95f);

            // Caja de fondo
            GUI.Box(new Rect(x, y, width, height), "\nINVENTARIO", titleStyle);

            // Restaurar el color de fondo
            GUI.backgroundColor = oldBgColor;

            // Configuración de la cuadrícula (Grid) estilo Minecraft
            int columnas = 4;
            int filas = 3;
            int totalSlots = columnas * filas; // 12 espacios en total
            float slotSize = 65; // Tamaño de cada cuadrito
            float padding = 12;  // Espacio entre cuadritos

            // Calcular dónde empieza a dibujarse la cuadrícula para que quede centrada
            float gridWidth = (columnas * slotSize) + ((columnas - 1) * padding);
            float startX = x + (width - gridWidth) / 2;
            float startY = y + 70;

            // Recorrer todos los espacios de la cuadrícula
            for (int i = 0; i < totalSlots; i++)
            {
                int filaActual = i / columnas;
                int colActual = i % columnas;
                
                float slotX = startX + colActual * (slotSize + padding);
                float slotY = startY + filaActual * (slotSize + padding);
                Rect slotRect = new Rect(slotX, slotY, slotSize, slotSize);

                InventoryItem item = inventory.GetItem(i);
                
                if (item != null)
                {
                    // Si hay un objeto, dibujamos un botón interactivo
                    if (GUI.Button(slotRect, $"{item.itemName}\n+{item.healAmount}", slotStyle))
                    {
                        UseItem(i); // Al hacer clic se usa y desaparece de la lista
                    }
                }
                else
                {
                    // Si está vacío, dibujamos una ranura (slot) oscurecida
                    GUI.backgroundColor = new Color(0.1f, 0.05f, 0.02f, 0.6f);
                    GUI.Box(slotRect, "", emptySlotStyle);
                    GUI.backgroundColor = oldBgColor; // Restaurar color para el siguiente
                }
            }

            // Subtítulo pequeño al final
            GUIStyle subStyle = new GUIStyle(GUI.skin.label);
            subStyle.fontSize = 12;
            subStyle.alignment = TextAnchor.UpperCenter;
            subStyle.normal.textColor = new Color(0.8f, 0.7f, 0.6f, 1f);
            GUI.Label(new Rect(x, y + height - 25, width, 20), "[ El panel se cierra usando la letra 'Y' ]", subStyle);
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
    }
}