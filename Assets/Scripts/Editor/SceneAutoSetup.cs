using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using AntiBullyingGame.RPG;
using AntiBullyingGame.Core;
using System.Collections.Generic;
using UnityEditor.Animations;
using System.Linq;
using UnityEngine.UI;

public class SceneAutoSetup : EditorWindow
{
    [MenuItem("POO Game/1. Configurar Escenas en Build")]
    public static void SetupBuildScenes()
    {
        // Buscar la escena de la escuela en todas las ubicaciones posibles
        string escuelaPath = null;
        string[] possiblePaths = { "Assets/Escuela.unity", "Assets/Scenes/Escuela.unity" };
        foreach (var p in possiblePaths)
        {
            if (System.IO.File.Exists(p)) { escuelaPath = p; break; }
        }
        
        // Si no la encontramos, guardar la escena actual
        if (escuelaPath == null)
        {
            var currentScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
            escuelaPath = string.IsNullOrEmpty(currentScene.path) ? "Assets/Escuela.unity" : currentScene.path;
            if (string.IsNullOrEmpty(currentScene.path) || currentScene.name == "Untitled")
            {
                UnityEditor.SceneManagement.EditorSceneManager.SaveScene(currentScene, "Assets/Escuela.unity");
                escuelaPath = "Assets/Escuela.unity";
            }
        }

        string battlePath = "Assets/Examples/Scenes/main scene.unity";

        List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>();
        
        if (System.IO.File.Exists(escuelaPath))
            buildScenes.Add(new EditorBuildSettingsScene(escuelaPath, true));
        else
            Debug.LogWarning($"No se encontró la escena de la Escuela");
            
        if (System.IO.File.Exists(battlePath))
            buildScenes.Add(new EditorBuildSettingsScene(battlePath, true));
        else
            Debug.LogWarning($"No se encontró la escena de batalla en: {battlePath}");
        
        EditorBuildSettings.scenes = buildScenes.ToArray();
        Debug.Log($"¡Escenas configuradas! Total: {buildScenes.Count} escenas en Build Settings.");
    }

    [MenuItem("POO Game/2. Master Final Delivery Setup")]
    public static void Setup()
    {
        if (Application.isPlaying)
        {
            Debug.LogError("¡ERROR! Estás intentando regenerar la escena mientras el juego está en PLAY. Por favor, detén el juego (presiona el botón de Play para salir del modo juego) antes de usar este menú.");
            return;
        }

        // 0. Configurar automáticamente las escenas en Build Settings
        SetupBuildScenes();

        // 0.5 Limpiar cache de animaciones para forzar regeneración limpia
        AnimationBuilder.ClearCache();
        // Borrar controllers corruptos (menos de 1KB = corrupto)
        if (System.IO.File.Exists("Assets/Controllers/blonde_man_Controller.controller"))
        {
            var fi = new System.IO.FileInfo("Assets/Controllers/blonde_man_Controller.controller");
            if (fi.Length < 1000)
            {
                AssetDatabase.DeleteAsset("Assets/Controllers/blonde_man_Controller.controller");
                Debug.Log("Controller corrupto de blonde_man borrado. Se regenerará.");
            }
        }

        // 1. Force Maximize on Play
        EditorPrefs.SetBool("GameView.MaximizeOnPlay", true);

        // 2. Configure Slicing & Import Settings
        string tilesetPath = "Assets/Sprites/school_tileset.png";
        ConfigureSlicing(tilesetPath, 8, 8, true);
        string p1Path = "Assets/Sprites/Characters/blonde_man.png";
        ConfigureSlicing(p1Path, 32, 32, false);
        string p2Path = "Assets/Sprites/Characters/blue_haired_woman.png";
        ConfigureSlicing(p2Path, 32, 32, false);

        string titleScreenPath = "Assets/Sprites/Backgrounds/TitleScreen_BG.png";
        string classroomBgPath = "Assets/Sprites/Backgrounds/Classroom_BG.png";
        ConfigureSingleSprite(titleScreenPath, 16);
        ConfigureSingleSprite(classroomBgPath, 32);

        // 3. Cleanup Scene safely
        var currentScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
        foreach (var obj in currentScene.GetRootGameObjects())
        {
            if (obj.name != "Main Camera" && obj.name != "Directional Light")
                DestroyImmediate(obj);
        }

        // 4. Background Preparation
        // Instead of generating tiles, we instantiate the Classroom image
        GameObject backgroundObj = new GameObject("Classroom_Background");
        backgroundObj.transform.position = Vector3.zero;
        var bgRenderer = backgroundObj.AddComponent<SpriteRenderer>();
        bgRenderer.sortingOrder = -20; // Ensure it's behind everything
        Sprite classroomSprite = AssetDatabase.LoadAssetAtPath<Sprite>(classroomBgPath);
        if (classroomSprite != null)
        {
            bgRenderer.sprite = classroomSprite;
            backgroundObj.transform.position = new Vector3(0, 0, 0); 
            // Restaurar la escala de la escena pero más grande
            backgroundObj.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
        }

        // --- SISTEMA DE FÍSICAS (BARRERAS INVISIBLES) ---
        var rb = backgroundObj.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;

        // Limite Superior (Pizarra / Muro)
        var topWall = backgroundObj.AddComponent<BoxCollider2D>();
        topWall.size = new Vector2(15f, 1f);
        topWall.offset = new Vector2(0f, 3.3f);

        // Limite Inferior
        var bottomWall = backgroundObj.AddComponent<BoxCollider2D>();
        bottomWall.size = new Vector2(15f, 1f);
        bottomWall.offset = new Vector2(0f, -3.6f);

        // Limite Izquierdo
        var leftWall = backgroundObj.AddComponent<BoxCollider2D>();
        leftWall.size = new Vector2(1f, 10f);
        leftWall.offset = new Vector2(-6.9f, 0f);

        // Limite Derecho
        var rightWall = backgroundObj.AddComponent<BoxCollider2D>();
        rightWall.size = new Vector2(1f, 10f);
        rightWall.offset = new Vector2(6.9f, 0f);

        // 5. Build Environment Collision/Grid (Optional for now, but keeping an empty grid)
        GameObject gridObj = new GameObject("Environment_Grid", typeof(Grid));
        gridObj.GetComponent<Grid>().cellSize = new Vector3(1, 1, 0);

        // 6. Player
        GameObject playerObj = new GameObject("PlayerAccountant");
        playerObj.transform.position = new Vector3(0, -1, 0);
        var sr = playerObj.AddComponent<SpriteRenderer>();
        var anim = playerObj.AddComponent<Animator>();
        var pClass = playerObj.AddComponent<Player>();
        
        // Físicas para el Jugador
        var pRb = playerObj.AddComponent<Rigidbody2D>();
        pRb.bodyType = RigidbodyType2D.Dynamic;
        pRb.gravityScale = 0;
        pRb.freezeRotation = true;
        var pCol = playerObj.AddComponent<BoxCollider2D>();
        pCol.size = new Vector2(0.8f, 0.8f);
        pCol.offset = new Vector2(0, 0.4f);

        sr.sortingOrder = 10;
        playerObj.tag = "Player";
        anim.runtimeAnimatorController = AnimationBuilder.GeneratePlayerAnimator(p1Path);
        sr.sprite = AssetDatabase.LoadAllAssetsAtPath(p1Path).OfType<Sprite>().FirstOrDefault();

        // 7. NPC (Victima)
        GameObject npcObj = new GameObject("Classmate_Sofia");
        npcObj.transform.position = new Vector3(2, -0.5f, 0); // Spaced appropriately
        npcObj.AddComponent<SpriteRenderer>().sortingOrder = 9;
        npcObj.AddComponent<Animator>().runtimeAnimatorController = AnimationBuilder.GeneratePlayerAnimator(p2Path);
        var npcClass = npcObj.AddComponent<Victim>();
        var soNpc = new SerializedObject(npcClass);
        soNpc.FindProperty("entityName").stringValue = "Sofía";
        
        // --- Generar Historia de Prueba para Sofía (2 Partes) ---
        if (!AssetDatabase.IsValidFolder("Assets/Stories")) AssetDatabase.CreateFolder("Assets", "Stories");
        
        string story1Path = "Assets/Stories/Sofia_Part1.asset";
        string story2Path = "Assets/Stories/Sofia_Part2.asset";
        
        var story2 = AssetDatabase.LoadAssetAtPath<HeartQuest.Core.DialogueData>(story2Path);
        if (story2 == null)
        {
            story2 = ScriptableObject.CreateInstance<HeartQuest.Core.DialogueData>();
            story2.moraleChangeOnComplete = 0;
            story2.lines = new HeartQuest.Core.DialogueLine[]
            {
                new HeartQuest.Core.DialogueLine { speakerName = "Sofía", text = "Mucho gusto, {PLAYER_NAME}. Qué bueno que estás aquí." },
                new HeartQuest.Core.DialogueLine { speakerName = "Sofía", text = "No quería decírselo a los profesores, pero esos chicos de allá... me han estado molestando toda la semana." },
                new HeartQuest.Core.DialogueLine { speakerName = "Sofía", text = "Hoy me quitaron mis apuntes de matemáticas y los rompieron frente a todos. Fue horrible." },
                new HeartQuest.Core.DialogueLine { speakerName = "Sofía", text = "Siento que a nadie le importa. Nadie interviene porque tienen miedo de que se la agarren con ellos..." },
                new HeartQuest.Core.DialogueLine { speakerName = "Sofía", text = "Tengo mucho miedo de volver mañana a clases. ¿Crees... crees que estoy exagerando?" }
            };
            story2.choices = new HeartQuest.Core.DialogueChoice[]
            {
                new HeartQuest.Core.DialogueChoice { choiceText = "Claro que no, te ayudaré a reportarlos.", moraleChange = 25 },
                new HeartQuest.Core.DialogueChoice { choiceText = "Tal vez sí exageras. Déjalos en paz.", moraleChange = -15 },
                new HeartQuest.Core.DialogueChoice { choiceText = "Lo siento, pero no me quiero meter en problemas.", moraleChange = -5 }
            };
            AssetDatabase.CreateAsset(story2, story2Path);
        }

        var story1 = AssetDatabase.LoadAssetAtPath<HeartQuest.Core.DialogueData>(story1Path);
        if (story1 == null)
        {
            story1 = ScriptableObject.CreateInstance<HeartQuest.Core.DialogueData>();
            story1.moraleChangeOnComplete = 0;
            story1.requiresNameInput = true;
            story1.nextDialogueAfterInput = story2;
            story1.lines = new HeartQuest.Core.DialogueLine[]
            {
                new HeartQuest.Core.DialogueLine { speakerName = "???", text = "Hola... no te había visto por aquí." },
                new HeartQuest.Core.DialogueLine { speakerName = "???", text = "Disculpa si me ves llorando... no he tenido un buen día." }
            };
            AssetDatabase.CreateAsset(story1, story1Path);
        }
        AssetDatabase.SaveAssets();
        
        soNpc.FindProperty("story").objectReferenceValue = story1;
        soNpc.ApplyModifiedProperties();

        npcObj.GetComponent<SpriteRenderer>().sprite = AssetDatabase.LoadAllAssetsAtPath(p2Path).OfType<Sprite>().FirstOrDefault();
        
        // Físicas para NPC
        var nRb = npcObj.AddComponent<Rigidbody2D>();
        nRb.bodyType = RigidbodyType2D.Static; // No la empujamos
        var nCol = npcObj.AddComponent<BoxCollider2D>();
        nCol.size = new Vector2(0.8f, 0.8f);
        nCol.offset = new Vector2(0, 0.4f);

        // 7.5 BULLIES (3 Personajes en un rincón)
        string bullyStoryPath = "Assets/Stories/BullyEncounter.asset";
        var bullyStory = AssetDatabase.LoadAssetAtPath<HeartQuest.Core.DialogueData>(bullyStoryPath);
        if (bullyStory == null)
        {
            bullyStory = ScriptableObject.CreateInstance<HeartQuest.Core.DialogueData>();
            bullyStory.moraleChangeOnComplete = -10;
            bullyStory.triggersBattle = true; // ESTO INICIA LA BATALLA UNDERTALE
            bullyStory.lines = new HeartQuest.Core.DialogueLine[]
            {
                new HeartQuest.Core.DialogueLine { speakerName = "Banda de Bullys", text = "¿Qué estás mirando, perdedor?" },
                new HeartQuest.Core.DialogueLine { speakerName = "Banda de Bullys", text = "Te equivocaste de pasillo. ¡Prepárate!" }
            };
            AssetDatabase.CreateAsset(bullyStory, bullyStoryPath);
        }

        string[] bullyNames = { "Max", "Leo", "Tyson" };
        Vector3[] bullyPositions = { new Vector3(-4f, 1f, 0), new Vector3(-5f, 0.5f, 0), new Vector3(-4f, 0f, 0) };

        for (int i = 0; i < 3; i++)
        {
            GameObject bObj = new GameObject("Bully_" + bullyNames[i]);
            bObj.transform.position = bullyPositions[i];
            bObj.AddComponent<SpriteRenderer>().sortingOrder = 9;
            // Usamos a blonde_man temporalmente como sprite del Bully
            bObj.AddComponent<Animator>().runtimeAnimatorController = AnimationBuilder.GeneratePlayerAnimator(p1Path);
            bObj.GetComponent<SpriteRenderer>().sprite = AssetDatabase.LoadAllAssetsAtPath(p1Path).OfType<Sprite>().FirstOrDefault();
            
            var bClass = bObj.AddComponent<AntiBullyingGame.RPG.Bully>();
            var soBully = new SerializedObject(bClass);
            soBully.FindProperty("entityName").stringValue = bullyNames[i];
            soBully.FindProperty("story").objectReferenceValue = bullyStory;
            soBully.ApplyModifiedProperties();

            var bRb = bObj.AddComponent<Rigidbody2D>();
            bRb.bodyType = RigidbodyType2D.Static;
            var bCol = bObj.AddComponent<BoxCollider2D>();
            bCol.size = new Vector2(0.8f, 0.8f);
            bCol.offset = new Vector2(0, 0.4f);
        }

        // 7.8 GameManager
        GameObject gmObj = new GameObject("GameManager");
        gmObj.AddComponent<GameManager>();

        // 8. Quality Lighting (Removed or kept minimal for 2D Unlit)
        // Ya que usamos Sprites, si no hay material iluminado no se verá el efecto normal,
        // pero podemos eliminar la luz o dejarla si usan URP.

        // 9. UI Canvas (Start Menu & UI System)
        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
        }

        GameObject canvasObj = new GameObject("UI_Presentation", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.layer = 5; // Layer UI
        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        // Panel de Inicio (Imagen de título)
        GameObject panelObj = new GameObject("StartScreenPanel", typeof(RectTransform), typeof(Image));
        panelObj.transform.SetParent(canvasObj.transform, false);
        panelObj.layer = 5; // Layer UI
        Image panelImg = panelObj.GetComponent<Image>();
        Sprite titleSprite = AssetDatabase.LoadAssetAtPath<Sprite>(titleScreenPath);
        if (titleSprite != null) {
            panelImg.sprite = titleSprite;
            panelImg.color = Color.white;
        } else {
            panelImg.color = new Color(0.1f, 0.1f, 0.1f, 1f);
        }
        
        RectTransform panelRT = panelObj.GetComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero; panelRT.anchorMax = Vector2.one; // Fullscreen
        panelRT.offsetMin = Vector2.zero; panelRT.offsetMax = Vector2.zero;

        // Boton START (Visible)
        GameObject btnObj = new GameObject("StartButton", typeof(RectTransform), typeof(Image), typeof(UnityEngine.UI.Button));
        btnObj.transform.SetParent(panelObj.transform, false);
        btnObj.layer = 5; // Layer UI
        Image btnImg = btnObj.GetComponent<Image>();
        btnImg.color = new Color(0f, 0.8f, 0.2f, 1f); // Verde visible
        RectTransform btnRTC = btnObj.GetComponent<RectTransform>();
        btnRTC.anchorMin = new Vector2(0.5f, 0.5f);
        btnRTC.anchorMax = new Vector2(0.5f, 0.5f);
        btnRTC.sizeDelta = new Vector2(300, 100);
        btnRTC.anchoredPosition = new Vector2(0, -150);
        
        // Texto del botón
        GameObject txtObj = new GameObject("Text", typeof(RectTransform));
        txtObj.transform.SetParent(btnObj.transform, false);
        var txt = txtObj.AddComponent<TMPro.TextMeshProUGUI>();
        txt.text = "INICIAR JUEGO";
        txt.alignment = TMPro.TextAlignmentOptions.Center;
        txt.color = Color.white;
        txt.fontSize = 36;
        RectTransform txtRT = txtObj.GetComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero; txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = Vector2.zero; txtRT.offsetMax = Vector2.zero;
        
        // Conectar el sistema con el Script que congela el juego
        var startScript = canvasObj.AddComponent<AntiBullyingGame.RPG.StartMenu>();
        startScript.startButton = btnObj.GetComponent<UnityEngine.UI.Button>();
        startScript.startMenuPanel = panelObj;

        // 9.5 RPG Stats UI (Top Left, Estilo Undertale Moderno)
        GameObject statsPanelObj = new GameObject("PlayerStatsUI", typeof(RectTransform), typeof(Image));
        statsPanelObj.transform.SetParent(canvasObj.transform, false);
        statsPanelObj.layer = 5;
        
        var statsImg = statsPanelObj.GetComponent<Image>();
        statsImg.color = new Color(0.05f, 0.05f, 0.08f, 0.9f); // Fondo oscuro para visibilidad
        
        RectTransform statsRT = statsPanelObj.GetComponent<RectTransform>();
        statsRT.anchorMin = new Vector2(0, 1); statsRT.anchorMax = new Vector2(0, 1);
        statsRT.pivot = new Vector2(0, 1);
        statsRT.sizeDelta = new Vector2(400, 70); // Más pequeño porque es solo 1 barra
        statsRT.anchoredPosition = new Vector2(20, -20); // Margen superior izquierdo

        var statsScript = statsPanelObj.AddComponent<HeartQuest.UI.PlayerStatsUI>();

        // Datos para generar la barra de Moral
        string[] statNames = { "MORAL" };
        Color[] fillColors = { 
            new Color(0.15f, 0.5f, 0.8f, 1f) // Azul apagado (MP / Moral)
        };
        
        for (int i = 0; i < 1; i++)
        {
            float yPos = -(i * 45); // Espaciado vertical

            // Row Container
            GameObject rowObj = new GameObject(statNames[i] + "_Row", typeof(RectTransform));
            rowObj.transform.SetParent(statsPanelObj.transform, false);
            RectTransform rowRT = rowObj.GetComponent<RectTransform>();
            rowRT.anchorMin = new Vector2(0, 1); rowRT.anchorMax = new Vector2(1, 1);
            rowRT.pivot = new Vector2(0, 1);
            rowRT.sizeDelta = new Vector2(400, 30);
            rowRT.anchoredPosition = new Vector2(0, yPos);

            // Label (e.g. "MORAL")
            GameObject labelObj = new GameObject("Label", typeof(RectTransform), typeof(TMPro.TextMeshProUGUI));
            labelObj.transform.SetParent(rowObj.transform, false);
            RectTransform lRT = labelObj.GetComponent<RectTransform>();
            lRT.anchorMin = new Vector2(0, 0.5f); lRT.anchorMax = new Vector2(0, 0.5f);
            lRT.pivot = new Vector2(0, 0.5f);
            lRT.sizeDelta = new Vector2(90, 30); // Más ancho para la palabra MORAL
            lRT.anchoredPosition = new Vector2(15, 0); // Margen interno

            var lTmp = labelObj.GetComponent<TMPro.TextMeshProUGUI>();
            lTmp.text = statNames[i];
            lTmp.fontSize = 20;
            lTmp.fontStyle = TMPro.FontStyles.Bold;
            lTmp.color = new Color(0.8f, 0.75f, 0.7f, 1f); // Gris más claro para visibilidad
            lTmp.alignment = TMPro.TextAlignmentOptions.Left;

            // Slider
            GameObject sliderObj = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
            sliderObj.transform.SetParent(rowObj.transform, false);
            RectTransform sRT = sliderObj.GetComponent<RectTransform>();
            sRT.anchorMin = new Vector2(0, 0.5f); sRT.anchorMax = new Vector2(0, 0.5f);
            sRT.pivot = new Vector2(0, 0.5f);
            sRT.sizeDelta = new Vector2(160, 18);
            sRT.anchoredPosition = new Vector2(100, 0);

            Slider slider = sliderObj.GetComponent<Slider>();
            slider.interactable = false;
            slider.transition = Selectable.Transition.None;

            // Slider Background
            GameObject bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgObj.transform.SetParent(sliderObj.transform, false);
            RectTransform bgRT = bgObj.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
            bgRT.sizeDelta = Vector2.zero;
            var bgImg = bgObj.GetComponent<Image>();
            bgImg.color = new Color(0.15f, 0.15f, 0.18f, 1f);
            
            // Borde redondeado sutil
            var bgOutline = bgObj.AddComponent<UnityEngine.UI.Outline>();
            bgOutline.effectColor = new Color(0.25f, 0.25f, 0.3f, 1f);
            bgOutline.effectDistance = new Vector2(1, -1);

            // Slider Fill Area
            GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderObj.transform, false);
            RectTransform faRT = fillArea.GetComponent<RectTransform>();
            faRT.anchorMin = Vector2.zero; faRT.anchorMax = Vector2.one;
            faRT.sizeDelta = new Vector2(-4, -4); // Padding interno

            GameObject fillObj = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillObj.transform.SetParent(fillArea.transform, false);
            RectTransform fRT = fillObj.GetComponent<RectTransform>();
            fRT.anchorMin = Vector2.zero; fRT.anchorMax = Vector2.one;
            fRT.sizeDelta = Vector2.zero;
            fillObj.GetComponent<Image>().color = fillColors[i];
            slider.fillRect = fRT;

            // Value Text (e.g. "70/200")
            GameObject valObj = new GameObject("Value", typeof(RectTransform), typeof(TMPro.TextMeshProUGUI));
            valObj.transform.SetParent(rowObj.transform, false);
            RectTransform vRT = valObj.GetComponent<RectTransform>();
            vRT.anchorMin = new Vector2(0, 0.5f); vRT.anchorMax = new Vector2(0, 0.5f);
            vRT.pivot = new Vector2(0, 0.5f);
            vRT.sizeDelta = new Vector2(120, 30);
            vRT.anchoredPosition = new Vector2(275, 0);

            var vTmp = valObj.GetComponent<TMPro.TextMeshProUGUI>();
            vTmp.text = "";
            vTmp.fontSize = 20;
            vTmp.fontStyle = TMPro.FontStyles.Bold;
            vTmp.color = new Color(0.8f, 0.75f, 0.7f, 1f); // Gris más claro
            vTmp.alignment = TMPro.TextAlignmentOptions.Left;

            // Asignar al script (i=0 es Moral en esta configuración)
            if (i == 0) { statsScript.mpSlider = slider; statsScript.mpText = vTmp; }
        }

        // 9.6 Dialogue System (Estilo Undertale)
        GameObject dialogueObj = new GameObject("DialogueBox", typeof(RectTransform), typeof(Image));
        dialogueObj.transform.SetParent(canvasObj.transform, false);
        dialogueObj.layer = 5;
        dialogueObj.GetComponent<Image>().color = new Color(0.05f, 0.05f, 0.1f, 0.95f); // Fondo oscuro
        
        RectTransform dRT = dialogueObj.GetComponent<RectTransform>();
        dRT.anchorMin = new Vector2(0.1f, 0.05f); 
        dRT.anchorMax = new Vector2(0.9f, 0.05f);
        dRT.pivot = new Vector2(0.5f, 0);
        dRT.sizeDelta = new Vector2(0, 200); // Altura de 200
        dRT.anchoredPosition = Vector2.zero;

        // Añadir el borde Neón
        var outline = dialogueObj.AddComponent<UnityEngine.UI.Outline>();
        outline.effectColor = new Color(0f, 0.898f, 1f, 1f); // Cian
        outline.effectDistance = new Vector2(3, -3);

        // Retrato (Portrait)
        GameObject portraitObj = new GameObject("Portrait", typeof(RectTransform), typeof(Image));
        portraitObj.transform.SetParent(dialogueObj.transform, false);
        RectTransform pRT = portraitObj.GetComponent<RectTransform>();
        pRT.anchorMin = new Vector2(0, 0.5f);
        pRT.anchorMax = new Vector2(0, 0.5f);
        pRT.pivot = new Vector2(0, 0.5f);
        pRT.sizeDelta = new Vector2(150, 150);
        pRT.anchoredPosition = new Vector2(100, 0); // Ajustado a la derecha del borde
        portraitObj.GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f, 1f); // Placeholder gris

        // Texto del Diálogo (TMPro)
        GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(TMPro.TextMeshProUGUI));
        textObj.transform.SetParent(dialogueObj.transform, false);
        RectTransform tRT = textObj.GetComponent<RectTransform>();
        tRT.anchorMin = Vector2.zero;
        tRT.anchorMax = Vector2.one;
        tRT.offsetMin = new Vector2(210, 20); // Margen para que no pise el retrato
        tRT.offsetMax = new Vector2(-20, -20);
        
        var tmpro = textObj.GetComponent<TMPro.TextMeshProUGUI>();
        tmpro.text = "";
        tmpro.fontSize = 35;
        tmpro.color = Color.white;
        tmpro.alignment = TMPro.TextAlignmentOptions.TopLeft;

        // Configurar el componente DialogueSystem
        var ds = dialogueObj.AddComponent<HeartQuest.UI.DialogueSystem>();
        var soDS = new SerializedObject(ds);
        soDS.FindProperty("dialogueText").objectReferenceValue = tmpro;
        soDS.FindProperty("portraitImage").objectReferenceValue = portraitObj.GetComponent<Image>();
        soDS.FindProperty("dialogueBox").objectReferenceValue = dialogueObj;
        soDS.ApplyModifiedProperties();

        // Empezar con el diálogo DESACTIVADO para que no bloquee el movimiento
        dialogueObj.SetActive(false);

        // 10. Camera Configuration (The Fix for the Skybox)
        var cam = Camera.main;
        if (cam != null)
        {
            cam.orthographic = true;
            // Aumentamos la cámara porque ampliamos el tamaño del salón
            cam.orthographicSize = 4f; 
            cam.clearFlags = CameraClearFlags.SolidColor;  // REMOVES THE 3D HORIZON
            cam.backgroundColor = new Color(0.1f, 0.1f, 0.12f);
            
            // Clean up old follow component if exists
            var oldF = cam.gameObject.GetComponent<CameraFollow>();
            if (oldF != null) DestroyImmediate(oldF);

            var follow = cam.gameObject.AddComponent<CameraFollow>();
            follow.target = playerObj.transform;
            follow.smoothSpeed = 0.1f;
        }

        if (!Application.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(currentScene);
        }
        Debug.Log("MASTER DELIVERY READY! Fixes injected. Press Play to see the classroom.");
    }

    private static void ConfigureSingleSprite(string path, float ppu)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) return;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = ppu; 
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        EditorUtility.SetDirty(importer);
        importer.SaveAndReimport();
    }

    private static void ConfigureSlicing(string path, int spriteWidth, int spriteHeight, bool isTile)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) return;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        
        importer.spritePixelsPerUnit = isTile ? 8 : 16; 
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.isReadable = true;

        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (texture == null) return;
        
        int width = texture.width;
        int height = texture.height;

        var factory = new UnityEditor.U2D.Sprites.SpriteDataProviderFactories();
        factory.Init();
        var dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);
        dataProvider.InitSpriteEditorDataProvider();

        var rects = new List<UnityEditor.SpriteRect>();
        int count = 0;
        for (int y = height - spriteHeight; y >= 0; y -= spriteHeight)
        {
            for (int x = 0; x < width; x += spriteWidth)
            {
                var smd = new UnityEditor.SpriteRect();
                if (isTile) {
                    smd.pivot = new Vector2(0.5f, 0.5f);
                    smd.alignment = SpriteAlignment.Custom; 
                } else {
                    smd.pivot = new Vector2(0.5f, 0f);
                    smd.alignment = SpriteAlignment.BottomCenter;
                }
                smd.name = System.IO.Path.GetFileNameWithoutExtension(path) + "_" + count++;
                smd.rect = new Rect(x, y, spriteWidth, spriteHeight);
                smd.spriteID = GUID.Generate();
                rects.Add(smd);
            }
        }
        
        dataProvider.SetSpriteRects(rects.ToArray());
        dataProvider.Apply();

        EditorUtility.SetDirty(importer);
        importer.SaveAndReimport();
    }
}
