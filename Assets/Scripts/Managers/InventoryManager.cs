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

    [Header("Puntos de Spawn de Objetos en el Mundo")]
    [Tooltip("Arrastra aquí los GameObjects vacíos que marcan dónde pueden aparecer los ítems en el mapa.")]
    public Transform[] spawnPoints;

    public InventoryLinkedList inventory = new InventoryLinkedList();

    private bool isInventoryOpen = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            // Persistir entre escenas para que el inventario esté disponible en BattleScene.
            // Solo hacemos DontDestroyOnLoad si este componente es raíz o tiene su propio GameObject.
            // Si comparte GameObject con otro manager, ya será persistente por ese otro script.
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
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
            inventory.AddItem(new InventoryItem("Cuaderno", 15));

            // Guardamos inmediatamente para crear el archivo de esta nueva partida
            if (AntiBullyingGame.Managers.SaveManager.Instance != null)
            {
                AntiBullyingGame.Managers.SaveManager.Instance.SaveCurrentGameState();
            }
        }
        SpawnRandomItems(5); // <-- Activar cuando el mapa esté listo y los SpawnPoints estén asignados en el Inspector

        inventory.PrintInventory();
    }

    private void SpawnRandomItems(int count)
    {
        string[] possibleItems = new string[] { "Carta", "Jugo", "Sandwich", "Pulsera", "Nota", "Dibujo", "Calcomania" };
        int[] possibleHeals = new int[] { 10, 3, 8, 12, 7, 10, 4 };

        // Verificamos si hay puntos de spawn asignados en el Inspector
        bool useSpawnPoints = spawnPoints != null && spawnPoints.Length > 0;

        if (!useSpawnPoints)
        {
            Debug.LogWarning("[InventoryManager] No hay SpawnPoints asignados. " +
                             "Los ítems NO se generarán para evitar que aparezcan fuera del mapa. " +
                             "Asigna puntos de spawn en el Inspector.");
            return;
        }

        // Mezclamos los puntos de spawn para que los ítems no aparezcan siempre en el mismo lugar
        // (Fisher-Yates shuffle sobre los índices)
        List<int> pointIndices = new List<int>();
        for (int p = 0; p < spawnPoints.Length; p++) pointIndices.Add(p);
        for (int p = pointIndices.Count - 1; p > 0; p--)
        {
            int swapIdx = UnityEngine.Random.Range(0, p + 1);
            int tmp = pointIndices[p];
            pointIndices[p] = pointIndices[swapIdx];
            pointIndices[swapIdx] = tmp;
        }

        // Mezclamos los índices de los ítems posibles para no generar repetidos
        List<int> itemIndices = new List<int>();
        for (int i = 0; i < possibleItems.Length; i++) itemIndices.Add(i);
        for (int i = itemIndices.Count - 1; i > 0; i--)
        {
            int swapIdx = UnityEngine.Random.Range(0, i + 1);
            int tmp = itemIndices[i];
            itemIndices[i] = itemIndices[swapIdx];
            itemIndices[swapIdx] = tmp;
        }

        // Generamos como máximo 'count' ítems, o menos si no hay suficientes puntos
        int spawnCount = Mathf.Min(count, spawnPoints.Length);

        for (int i = 0; i < spawnCount; i++)
        {
            // Usamos el índice de ítem mezclado (usamos módulo por si count es mayor a possibleItems.Length)
            int rndIndex = itemIndices[i % itemIndices.Count];
            string iName = possibleItems[rndIndex];
            int iHeal = possibleHeals[rndIndex];

            // Usamos el punto de spawn mezclado
            Vector3 spawnPos = spawnPoints[pointIndices[i]].position;

            GameObject itemObj = new GameObject("Pickup_" + iName);
            itemObj.transform.position = spawnPos;

            SpriteRenderer sr = itemObj.AddComponent<SpriteRenderer>();
            Texture2D iconTex = GetItemIcon(iName);
            if (iconTex != null)
            {
                sr.sprite = Sprite.Create(iconTex, new Rect(0, 0, iconTex.width, iconTex.height), new Vector2(0.5f, 0.5f));
            }
            sr.sortingOrder = 5;

            itemObj.transform.localScale = new Vector3(0.3f, 0.3f, 1f);

            BoxCollider2D col = itemObj.AddComponent<BoxCollider2D>();
            col.isTrigger = true;

            ItemPickup pickup = itemObj.AddComponent<ItemPickup>();
            pickup.itemName = iName;
            pickup.healAmount = iHeal;
        }
    }

    /// <summary>
    /// Devuelve true si la escena actual es la de batalla.
    /// En ese caso, el inventario visual (OnGUI) se oculta porque
    /// el ItemManager de batalla maneja la mochila.
    /// </summary>
    private bool IsInBattleScene()
    {
        return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "BattleScene";
    }

    private void Update()
    {
        // En BattleScene, cerrar el inventario visual y bloquear el toggle
        if (IsInBattleScene())
        {
            isInventoryOpen = false;
            return;
        }

        // Al presionar la letra 'Y' o el botón Y del control (JoystickButton3), abrimos o cerramos el inventario
        if (Input.GetKeyDown(KeyCode.Y) || Input.GetKeyDown(KeyCode.JoystickButton3))
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
                int colActual = i % columnas;

                float slotX = startX + colActual * (slotSize + colPadding);
                float slotY = startY + filaActual * rowHeight;
                Rect slotRect = new Rect(slotX, slotY, slotSize, slotSize);

                InventoryItem item = inventory.GetItem(i);

                if (item != null)
                {
                    Texture2D icon = GetItemIcon(item.itemName);

                    // Botón invisible para capturar el clic sin fondo
                    Color oldColor = GUI.backgroundColor;
                    GUI.backgroundColor = Color.clear;
                    if (GUI.Button(slotRect, "", slotStyle))
                    {
                        UseItem(i);
                    }
                    GUI.backgroundColor = oldColor;

                    // Ícono centrado dentro del slot con padding uniforme
                    if (icon != null)
                    {
                        float pad = 6f;
                        Rect iconRect = new Rect(
                            slotRect.x + pad,
                            slotRect.y + pad,
                            slotRect.width - pad * 2,
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

            PlayerVars player = FindObjectOfType<PlayerVars>();

            if (player != null)
            {
                Debug.Log("PLAYER ENCONTRADO: " + player.gameObject.name);
                player.Heal(item.healAmount);
            }
            else
            {
                Debug.LogError("NO SE ENCONTRÓ PLAYERVARS EN LA ESCENA");
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
        inventory.AddItem(new InventoryItem("Cuaderno", 15));
    }
}