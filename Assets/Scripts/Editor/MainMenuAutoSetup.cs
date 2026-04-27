using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using AntiBullyingGame.UI;

public class MainMenuAutoSetup
{
    // [MenuItem("POO Game/Setup Main Menu UI")] // Removido: usar solo Build_Heartquest
    // Colores cyberpunk
    static Color bgColor = new Color(0.039f, 0.059f, 0.110f, 1f);       // #0A0F1C
    static Color panelColor = new Color(0.071f, 0.102f, 0.169f, 0.85f); // #121A2B
    static Color accentPurple = new Color(0.616f, 0.302f, 1f, 1f);      // #9D4DFF
    static Color accentCyan = new Color(0f, 0.898f, 1f, 1f);            // #00E5FF
    static Color textWhite = new Color(0.9f, 0.92f, 0.95f, 1f);
    static Color textDim = new Color(0.5f, 0.55f, 0.65f, 1f);

    [MenuItem("POO Game/Setup Cyberpunk Main Menu")]
    public static void SetupMainMenu()
    {
        // 0. Abrir la escena MainMenu específicamente
        string scenePath = "Assets/Scenes/MainMenu.unity";
        
        // Guardar escena actual si tiene cambios antes de abrir la otra
        UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

        // Abrir la escena de MainMenu
        var currentScene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);
        foreach (var obj in currentScene.GetRootGameObjects())
        {
            if (obj.name != "Main Camera" && obj.name != "Directional Light")
                GameObject.DestroyImmediate(obj);
        }

        // 1.5 Configurar cámara para el menú
        var cam = Camera.main;
        if (cam != null)
        {
            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.08f, 0.12f); // Fondo oscuro elegante
        }

        // 2. Crear EventSystem (Necesario para que los botones detecten clicks)
        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
        }

        // 3. Crear Canvas Principal
        GameObject canvasObj = new GameObject("MainMenuCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // 4. Añadir el Controller que creamos antes
        var controller = canvasObj.AddComponent<MainMenuController>();

        // Cargar fuente bonita (Roboto-Bold)
        Font customFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/TextMesh Pro/Examples & Extras/Fonts/Roboto-Bold.ttf");

        // 5. Crear Panel Principal (Main Menu)
        GameObject mainPanel = CreatePanel("MainPanel", canvasObj.transform);
        
        // Remover el componente Image por defecto y usar RawImage para garantizar que cargue cualquier textura
        GameObject.DestroyImmediate(mainPanel.GetComponent<Image>());
        RawImage mainPanelRawImage = mainPanel.AddComponent<RawImage>();
        
        // Cargar la textura de fondo
        string bgPath = "Assets/Sprites/Backgrounds/BMain.jpg";
        Texture2D bgTex = AssetDatabase.LoadAssetAtPath<Texture2D>(bgPath);

        if (bgTex != null)
        {
            mainPanelRawImage.color = Color.white; 
            mainPanelRawImage.texture = bgTex;
        }
        else
        {
            mainPanelRawImage.color = new Color(0.15f, 0.15f, 0.2f, 1f); // Fallback
            Debug.LogError("No se pudo cargar la textura de fondo en: " + bgPath);
        }

        // 5.5 Crear un panel oscuro superpuesto para mejorar la legibilidad de la UI
        GameObject overlayObj = CreatePanel("Overlay", mainPanel.transform);
        Image overlayImage = overlayObj.GetComponent<Image>();
        overlayImage.color = new Color(0.2f, 0.1f, 0.05f, 0.6f); // Tonos tierra cálidos semi-transparentes

        // Añadir Título del Juego
        GameObject titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(Text));
        titleObj.transform.SetParent(mainPanel.transform, false);
        Text titleText = titleObj.GetComponent<Text>();
        if (customFont != null) titleText.font = customFont;
        titleText.text = "HEARTQUEST";
        titleText.fontSize = 130;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = new Color(0.95f, 0.82f, 0.55f, 1f); // Dorado arena suave
        
        Outline titleOutline = titleObj.AddComponent<Outline>();
        titleOutline.effectColor = new Color(0.3f, 0.15f, 0.05f, 1f); // Marrón oscuro de sombra
        titleOutline.effectDistance = new Vector2(4, -4);

        RectTransform titleRT = titleObj.GetComponent<RectTransform>();
        titleRT.sizeDelta = new Vector2(1400, 250);
        titleRT.anchoredPosition = new Vector2(0, 350);

        // 6. Crear Panel de Opciones
        GameObject optionsPanel = CreatePanel("OptionsPanel", canvasObj.transform);
        optionsPanel.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f, 1f); // Un poco más oscuro
        optionsPanel.SetActive(false); // Oculto por defecto

        // Añadir Título de Opciones
        GameObject optTitle = new GameObject("OptionsTitle", typeof(RectTransform), typeof(Text));
        optTitle.transform.SetParent(optionsPanel.transform, false);
        Text optText = optTitle.GetComponent<Text>();
        if (customFont != null) optText.font = customFont;
        optText.text = "OPCIONES";
        optText.fontSize = 90;
        optText.fontStyle = FontStyle.Bold;
        optText.alignment = TextAnchor.MiddleCenter;
        optText.color = new Color(0.95f, 0.82f, 0.55f, 1f);
        RectTransform optTitleRT = optTitle.GetComponent<RectTransform>();
        optTitleRT.sizeDelta = new Vector2(800, 150);
        optTitleRT.anchoredPosition = new Vector2(0, 250);

        // 6.5 Crear Panel de Carga (LoadPanel)
        GameObject loadPanel = CreatePanel("LoadPanel", canvasObj.transform);
        loadPanel.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f, 1f); 
        loadPanel.SetActive(false); // Oculto por defecto

        // Añadir Título de Carga
        GameObject loadTitle = new GameObject("LoadTitle", typeof(RectTransform), typeof(Text));
        loadTitle.transform.SetParent(loadPanel.transform, false);
        Text loadText = loadTitle.GetComponent<Text>();
        if (customFont != null) loadText.font = customFont;
        loadText.text = "CARGAR PARTIDA";
        loadText.fontSize = 90;
        loadText.fontStyle = FontStyle.Bold;
        loadText.alignment = TextAnchor.MiddleCenter;
        loadText.color = new Color(0.95f, 0.82f, 0.55f, 1f);
        RectTransform loadTitleRT = loadTitle.GetComponent<RectTransform>();
        loadTitleRT.sizeDelta = new Vector2(1000, 150);
        loadTitleRT.anchoredPosition = new Vector2(0, 350);

        // Crear ScrollRect simplificado (solo Content con VerticalLayout)
        GameObject scrollObj = new GameObject("ScrollArea", typeof(RectTransform));
        scrollObj.transform.SetParent(loadPanel.transform, false);
        RectTransform scrollRT = scrollObj.GetComponent<RectTransform>();
        scrollRT.sizeDelta = new Vector2(600, 600);
        scrollRT.anchoredPosition = new Vector2(0, 0);

        GameObject contentObj = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        contentObj.transform.SetParent(scrollObj.transform, false);
        RectTransform contentRT = contentObj.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1);
        contentRT.sizeDelta = new Vector2(0, 0);
        contentRT.anchoredPosition = Vector2.zero;

        VerticalLayoutGroup vlg = contentObj.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 20;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlHeight = false;
        vlg.childControlWidth = false;

        ContentSizeFitter csf = contentObj.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // 7. Crear Botones en Main Panel
        Button newGameBtn = CreateButton("NewGameButton", mainPanel.transform, "NUEVO JUEGO", new Vector2(0, 100), customFont);
        Button continueBtn = CreateButton("ContinueButton", mainPanel.transform, "CONTINUAR JUEGO", new Vector2(0, 0), customFont);
        Button loadBtn = CreateButton("LoadButton", mainPanel.transform, "CARGAR JUEGO", new Vector2(0, -100), customFont);
        Button optBtn = CreateButton("OptionsButton", mainPanel.transform, "OPCIONES", new Vector2(0, -200), customFont);
        Button quitBtn = CreateButton("QuitButton", mainPanel.transform, "SALIR", new Vector2(0, -300), customFont);

        // 8. Crear Botones en Options Panel y Load Panel
        Button backBtn = CreateButton("BackButton", optionsPanel.transform, "VOLVER", new Vector2(0, -300), customFont);
        Button loadBackBtn = CreateButton("BackButton", loadPanel.transform, "VOLVER", new Vector2(0, -400), customFont);

        // 9. Asignar referencias en el Controller
        var so = new SerializedObject(controller);
        so.FindProperty("mainPanel").objectReferenceValue = mainPanel;
        so.FindProperty("optionsPanel").objectReferenceValue = optionsPanel;
        so.FindProperty("loadPanel").objectReferenceValue = loadPanel;
        so.FindProperty("loadContentTransform").objectReferenceValue = contentObj.transform;
        so.FindProperty("sceneToLoad").stringValue = "ClassroomScene"; 
        so.ApplyModifiedProperties();

        // 10. Conectar Eventos (OnClicks) de Unity automáticamente usando UnityAction
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(newGameBtn.onClick, new UnityEngine.Events.UnityAction(controller.NewGame));
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(continueBtn.onClick, new UnityEngine.Events.UnityAction(controller.ContinueGame));
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(loadBtn.onClick, new UnityEngine.Events.UnityAction(controller.ShowLoadPanel));
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(optBtn.onClick, new UnityEngine.Events.UnityAction(controller.ShowOptions));
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(quitBtn.onClick, new UnityEngine.Events.UnityAction(controller.QuitGame));
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(backBtn.onClick, new UnityEngine.Events.UnityAction(controller.ShowMainPanel));
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(loadBackBtn.onClick, new UnityEngine.Events.UnityAction(controller.ShowMainPanel));

        // 11. Guardar cambios en la escena automáticamente
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(currentScene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(currentScene);
        Debug.Log("¡El Menú Principal ha sido inyectado y guardado automáticamente con éxito!");
    }

    private static GameObject CreatePanel(string name, Transform parent)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return panel;
    }

    private static Button CreateButton(string name, Transform parent, string textStr, Vector2 pos, Font customFont = null)
    {
        GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(parent, false);
        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(500, 80);
        rt.anchoredPosition = pos;

        Image img = btnObj.GetComponent<Image>();
        img.color = new Color(0.45f, 0.28f, 0.18f, 0.95f); // Marrón cálido (Tierra)

        Outline outlineBtn = btnObj.AddComponent<Outline>();
        outlineBtn.effectColor = new Color(0.85f, 0.65f, 0.4f, 0.8f); // Borde dorado suave
        outlineBtn.effectDistance = new Vector2(2, -2);

        GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
        textObj.transform.SetParent(btnObj.transform, false);
        Text txt = textObj.GetComponent<Text>();
        if (customFont != null) txt.font = customFont;
        txt.text = textStr;
        txt.fontSize = 38;
        txt.fontStyle = FontStyle.Bold;
        txt.color = new Color(0.98f, 0.92f, 0.85f, 1f); // Texto crema pálido
        txt.alignment = TextAnchor.MiddleCenter;
        
        Shadow textShadow = textObj.AddComponent<Shadow>();
        textShadow.effectColor = Color.black;
        textShadow.effectDistance = new Vector2(2, -2);
        
        RectTransform textRT = textObj.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        return btnObj.GetComponent<Button>();
    }
}
