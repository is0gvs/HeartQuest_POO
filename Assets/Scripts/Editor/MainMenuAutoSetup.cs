using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using AntiBullyingGame.UI;

public class MainMenuAutoSetup
{
<<<<<<< Updated upstream
    // [MenuItem("POO Game/Setup Main Menu UI")] // Removido: usar solo Build_Heartquest
=======
    // Colores cyberpunk
    static Color bgColor = new Color(0.039f, 0.059f, 0.110f, 1f);       // #0A0F1C
    static Color panelColor = new Color(0.071f, 0.102f, 0.169f, 0.85f); // #121A2B
    static Color accentPurple = new Color(0.616f, 0.302f, 1f, 1f);      // #9D4DFF
    static Color accentCyan = new Color(0f, 0.898f, 1f, 1f);            // #00E5FF
    static Color textWhite = new Color(0.9f, 0.92f, 0.95f, 1f);
    static Color textDim = new Color(0.5f, 0.55f, 0.65f, 1f);

    [MenuItem("POO Game/Setup Cyberpunk Main Menu")]
>>>>>>> Stashed changes
    public static void SetupMainMenu()
    {
        // 1. Limpiamos la escena actual
        var currentScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
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

        // 5. Crear Panel Principal (Main Menu)
        GameObject mainPanel = CreatePanel("MainPanel", canvasObj.transform);
        mainPanel.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f, 1f); // Fondo azul marino oscuro

        // Añadir Título del Juego
        GameObject titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(Text));
        titleObj.transform.SetParent(mainPanel.transform, false);
        Text titleText = titleObj.GetComponent<Text>();
        titleText.text = "HEARTQUEST POO";
        titleText.fontSize = 120;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = Color.white;
        RectTransform titleRT = titleObj.GetComponent<RectTransform>();
        titleRT.sizeDelta = new Vector2(1200, 200);
        titleRT.anchoredPosition = new Vector2(0, 250);

        // 6. Crear Panel de Opciones
        GameObject optionsPanel = CreatePanel("OptionsPanel", canvasObj.transform);
        optionsPanel.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f, 1f); // Un poco más oscuro
        optionsPanel.SetActive(false); // Oculto por defecto

        // Añadir Título de Opciones
        GameObject optTitle = new GameObject("OptionsTitle", typeof(RectTransform), typeof(Text));
        optTitle.transform.SetParent(optionsPanel.transform, false);
        Text optText = optTitle.GetComponent<Text>();
        optText.text = "OPCIONES";
        optText.fontSize = 90;
        optText.fontStyle = FontStyle.Bold;
        optText.alignment = TextAnchor.MiddleCenter;
        optText.color = Color.white;
        RectTransform optTitleRT = optTitle.GetComponent<RectTransform>();
        optTitleRT.sizeDelta = new Vector2(800, 150);
        optTitleRT.anchoredPosition = new Vector2(0, 250);

        // 7. Crear Botones en Main Panel
        Button playBtn = CreateButton("PlayButton", mainPanel.transform, "JUGAR", new Vector2(0, 0));
        Button optBtn = CreateButton("OptionsButton", mainPanel.transform, "OPCIONES", new Vector2(0, -150));
        Button quitBtn = CreateButton("QuitButton", mainPanel.transform, "SALIR", new Vector2(0, -300));

        // 8. Crear Botones en Options Panel
        Button backBtn = CreateButton("BackButton", optionsPanel.transform, "VOLVER", new Vector2(0, -300));

        // 9. Asignar referencias en el Controller
        var so = new SerializedObject(controller);
        so.FindProperty("mainPanel").objectReferenceValue = mainPanel;
        so.FindProperty("optionsPanel").objectReferenceValue = optionsPanel;
        so.FindProperty("sceneToLoad").stringValue = "ClassroomScene"; 
        so.ApplyModifiedProperties();

        // 10. Conectar Eventos (OnClicks) de Unity automáticamente usando UnityAction
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(playBtn.onClick, new UnityEngine.Events.UnityAction(controller.PlayGame));
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(optBtn.onClick, new UnityEngine.Events.UnityAction(controller.ShowOptions));
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(quitBtn.onClick, new UnityEngine.Events.UnityAction(controller.QuitGame));
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(backBtn.onClick, new UnityEngine.Events.UnityAction(controller.ShowMainPanel));

        // 11. Guardar cambios en la escena
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(currentScene);
        Debug.Log("¡El Menú Principal ha sido inyectado automáticamente con éxito!");
    }

<<<<<<< Updated upstream
    private static GameObject CreatePanel(string name, Transform parent)
=======
    // ════════════════════════════════════════════════
    // BACKGROUND
    // ════════════════════════════════════════════════
    static void CreateBackground(Transform parent)
    {
        GameObject bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bg.transform.SetParent(parent, false);
        StretchFull(bg);
        bg.GetComponent<Image>().color = bgColor;

        // Grid lines decorativas
        GameObject grid = new GameObject("GridOverlay", typeof(RectTransform), typeof(Image));
        grid.transform.SetParent(bg.transform, false);
        StretchFull(grid);
        Image gridImg = grid.GetComponent<Image>();
        gridImg.color = new Color(accentPurple.r, accentPurple.g, accentPurple.b, 0.03f);
    }

    // ════════════════════════════════════════════════
    // TOP BAR
    // ════════════════════════════════════════════════
    static void CreateTopBar(Transform parent)
    {
        GameObject topBar = CreatePanel("TopBar", parent, panelColor);
        RectTransform rt = topBar.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.sizeDelta = new Vector2(0, 60);
        rt.anchoredPosition = Vector2.zero;
        AddOutline(topBar, accentPurple, 1f);

        // Player Info
        GameObject playerInfo = new GameObject("PlayerInfo", typeof(RectTransform));
        playerInfo.transform.SetParent(topBar.transform, false);
        RectTransform piRT = playerInfo.GetComponent<RectTransform>();
        piRT.anchorMin = new Vector2(0, 0); piRT.anchorMax = new Vector2(0.3f, 1);
        piRT.offsetMin = new Vector2(20, 5); piRT.offsetMax = new Vector2(0, -5);

        CreateTMPText("LV_Label", playerInfo.transform, "LV 1", 18, accentCyan, TextAlignmentOptions.MidlineLeft,
            new Vector2(0, 0), new Vector2(0.2f, 1), Vector2.zero, Vector2.zero);

        // XP Bar en TopBar
        CreateXPBar(playerInfo.transform, new Vector2(0.22f, 0), new Vector2(0.9f, 1));

        // Currency
        GameObject currency = new GameObject("Currency", typeof(RectTransform));
        currency.transform.SetParent(topBar.transform, false);
        RectTransform cRT = currency.GetComponent<RectTransform>();
        cRT.anchorMin = new Vector2(0.7f, 0); cRT.anchorMax = new Vector2(1, 1);
        cRT.offsetMin = new Vector2(0, 5); cRT.offsetMax = new Vector2(-20, -5);

        CreateTMPText("GoldText", currency.transform, "♦ 9999", 20, accentCyan, TextAlignmentOptions.MidlineRight,
            new Vector2(0.5f, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
    }

    // ════════════════════════════════════════════════
    // LEFT MENU
    // ════════════════════════════════════════════════
    [MenuItem("POO Game/Fix Menu Position")]
    public static void FixMenuPosition()
    {
        var panel = GameObject.Find("MainMenuPanel");
        if (panel != null)
        {
            RectTransform rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(400, 500);
            rt.anchoredPosition = Vector2.zero;
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            Debug.Log("<color=#00E5FF>♥ Menú centrado correctamente ♥</color>");
        }
        else
        {
            Debug.LogWarning("No se encontró MainMenuPanel en la escena. Asegúrate de estar en la escena correcta.");
        }
    }

    static GameObject CreateLeftMenu(Transform parent)
    {
        GameObject leftMenu = CreatePanel("MainMenuPanel", parent, new Color(panelColor.r, panelColor.g, panelColor.b, 0.8f));
        RectTransform rt = leftMenu.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(400, 500);
        rt.anchoredPosition = Vector2.zero;
        AddOutline(leftMenu, accentPurple, 2f);
        leftMenu.AddComponent<NeonPanelFlicker>();

        // Vertical Layout
        VerticalLayoutGroup vlg = leftMenu.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(20, 20, 30, 30);
        vlg.spacing = 12;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlHeight = false;
        vlg.childControlWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = true;

        // Título del menú
        GameObject title = CreateTMPText("MenuTitle", leftMenu.transform, "♥ HEARTQUEST", 28, accentPurple,
            TextAlignmentOptions.Center, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        LayoutElement titleLE = title.AddComponent<LayoutElement>();
        titleLE.preferredHeight = 50;
        titleLE.flexibleWidth = 1;

        // Separador
        GameObject sep = new GameObject("Separator", typeof(RectTransform), typeof(Image));
        sep.transform.SetParent(leftMenu.transform, false);
        sep.GetComponent<Image>().color = new Color(accentPurple.r, accentPurple.g, accentPurple.b, 0.4f);
        LayoutElement sepLE = sep.AddComponent<LayoutElement>();
        sepLE.preferredHeight = 2;
        sepLE.flexibleWidth = 1;

        // Botones
        string[] btnNames = { "Btn_Start", "Btn_Continue", "Btn_NewGame", "Btn_Options", "Btn_Exit" };
        string[] btnLabels = { "START", "CONTINUAR JUEGO", "NUEVO JUEGO", "OPCIONES", "SALIR" };

        for (int i = 0; i < btnNames.Length; i++)
        {
            CreateNeonButton(btnNames[i], leftMenu.transform, btnLabels[i]);
        }

        return leftMenu;
    }

    // ════════════════════════════════════════════════
    // CENTER VISUAL
    // ════════════════════════════════════════════════
    static void CreateCenterVisual(Transform parent)
    {
        GameObject center = new GameObject("CenterVisual", typeof(RectTransform));
        center.transform.SetParent(parent, false);
        RectTransform rt = center.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.28f, 0.15f);
        rt.anchorMax = new Vector2(0.68f, 0.9f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // Character placeholder
        GameObject character = new GameObject("Character", typeof(RectTransform), typeof(Image));
        character.transform.SetParent(center.transform, false);
        RectTransform charRT = character.GetComponent<RectTransform>();
        charRT.anchorMin = new Vector2(0.15f, 0.05f);
        charRT.anchorMax = new Vector2(0.85f, 0.75f);
        charRT.offsetMin = Vector2.zero;
        charRT.offsetMax = Vector2.zero;
        Image charImg = character.GetComponent<Image>();
        charImg.color = new Color(textDim.r, textDim.g, textDim.b, 0.15f);

        CreateTMPText("CharLabel", character.transform, "[ CHARACTER ]", 22, textDim, TextAlignmentOptions.Center,
            new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);

        // Floating Heart
        GameObject heart = new GameObject("FloatingHeart", typeof(RectTransform), typeof(Image));
        heart.transform.SetParent(center.transform, false);
        RectTransform hrt = heart.GetComponent<RectTransform>();
        hrt.anchorMin = new Vector2(0.5f, 0.5f);
        hrt.anchorMax = new Vector2(0.5f, 0.5f);
        hrt.sizeDelta = new Vector2(80, 80);
        hrt.anchoredPosition = new Vector2(0, 200);
        Image heartImg = heart.GetComponent<Image>();
        heartImg.color = accentPurple;
        heart.AddComponent<HeartPulse>();

        // Heart Glow
        GameObject glow = new GameObject("Glow", typeof(RectTransform), typeof(Image));
        glow.transform.SetParent(heart.transform, false);
        RectTransform glowRT = glow.GetComponent<RectTransform>();
        glowRT.anchorMin = new Vector2(0.5f, 0.5f);
        glowRT.anchorMax = new Vector2(0.5f, 0.5f);
        glowRT.sizeDelta = new Vector2(160, 160);
        glowRT.anchoredPosition = Vector2.zero;
        Image glowImg = glow.GetComponent<Image>();
        glowImg.color = new Color(accentPurple.r, accentPurple.g, accentPurple.b, 0.2f);
    }

    // ════════════════════════════════════════════════
    // RIGHT PANEL
    // ════════════════════════════════════════════════
    static void CreateRightPanel(Transform parent)
    {
        GameObject rightPanel = CreatePanel("RightPanel", parent, new Color(panelColor.r, panelColor.g, panelColor.b, 0.7f));
        RectTransform rt = rightPanel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.72f, 0.1f);
        rt.anchorMax = new Vector2(1, 0.85f);
        rt.offsetMin = new Vector2(0, 0);
        rt.offsetMax = new Vector2(-30, 0);
        AddOutline(rightPanel, accentCyan, 2f);

        VerticalLayoutGroup vlg = rightPanel.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(15, 15, 15, 15);
        vlg.spacing = 10;
        vlg.childControlHeight = false;
        vlg.childControlWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = true;

        // ─── Profile Card ───
        GameObject profileCard = CreatePanel("ProfileCard", rightPanel.transform, new Color(0.05f, 0.08f, 0.14f, 0.9f));
        LayoutElement pcLE = profileCard.AddComponent<LayoutElement>();
        pcLE.preferredHeight = 200;
        AddOutline(profileCard, accentPurple, 1f);

        VerticalLayoutGroup pcVLG = profileCard.AddComponent<VerticalLayoutGroup>();
        pcVLG.padding = new RectOffset(12, 12, 12, 12);
        pcVLG.spacing = 6;
        pcVLG.childControlHeight = false;
        pcVLG.childControlWidth = true;
        pcVLG.childForceExpandHeight = false;

        // Avatar
        GameObject avatar = new GameObject("Avatar", typeof(RectTransform), typeof(Image));
        avatar.transform.SetParent(profileCard.transform, false);
        avatar.GetComponent<Image>().color = new Color(accentCyan.r, accentCyan.g, accentCyan.b, 0.2f);
        LayoutElement avLE = avatar.AddComponent<LayoutElement>();
        avLE.preferredHeight = 70;

        CreateTMPText("AvatarLabel", avatar.transform, "♥", 32, accentPurple, TextAlignmentOptions.Center,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        // Name
        GameObject nameText = CreateTMPText("PlayerName", profileCard.transform, "JUGADOR", 22, textWhite,
            TextAlignmentOptions.Center, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        nameText.AddComponent<LayoutElement>().preferredHeight = 30;

        // LV + XP
        GameObject lvRow = new GameObject("LV_Row", typeof(RectTransform));
        lvRow.transform.SetParent(profileCard.transform, false);
        lvRow.AddComponent<LayoutElement>().preferredHeight = 25;

        CreateTMPText("LV", lvRow.transform, "LV 1", 16, accentCyan, TextAlignmentOptions.MidlineLeft,
            new Vector2(0, 0), new Vector2(0.25f, 1), Vector2.zero, Vector2.zero);

        CreateXPBar(lvRow.transform, new Vector2(0.28f, 0.15f), new Vector2(0.98f, 0.85f));

        // ─── Stats Panel ───
        GameObject statsPanel = CreatePanel("StatsPanel", rightPanel.transform, new Color(0.05f, 0.08f, 0.14f, 0.9f));
        LayoutElement spLE = statsPanel.AddComponent<LayoutElement>();
        spLE.preferredHeight = 220;
        AddOutline(statsPanel, accentCyan, 1f);

        VerticalLayoutGroup spVLG = statsPanel.AddComponent<VerticalLayoutGroup>();
        spVLG.padding = new RectOffset(15, 15, 12, 12);
        spVLG.spacing = 8;
        spVLG.childControlHeight = false;
        spVLG.childControlWidth = true;
        spVLG.childForceExpandHeight = false;

        CreateTMPText("StatsTitle", statsPanel.transform, "─ ESTADÍSTICAS ─", 16, accentCyan,
            TextAlignmentOptions.Center, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero)
            .AddComponent<LayoutElement>().preferredHeight = 25;

        string[][] stats = {
            new[] { "TimePlayed", "Tiempo Jugado:", "00:00:00" },
            new[] { "EnemiesDefeated", "Enemigos:", "0" },
            new[] { "RoutesCompleted", "Rutas:", "0 / 3" },
            new[] { "EndingsUnlocked", "Finales:", "0 / 5" }
        };

        foreach (var s in stats)
        {
            GameObject row = new GameObject(s[0], typeof(RectTransform));
            row.transform.SetParent(statsPanel.transform, false);
            row.AddComponent<LayoutElement>().preferredHeight = 22;

            CreateTMPText("Label", row.transform, s[1], 14, textDim, TextAlignmentOptions.MidlineLeft,
                new Vector2(0, 0), new Vector2(0.6f, 1), Vector2.zero, Vector2.zero);
            CreateTMPText("Value", row.transform, s[2], 14, accentCyan, TextAlignmentOptions.MidlineRight,
                new Vector2(0.6f, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
        }
    }

    // ════════════════════════════════════════════════
    // BOTTOM DIALOGUE
    // ════════════════════════════════════════════════
    static void CreateBottomDialogue(Transform parent)
    {
        GameObject dialogue = CreatePanel("BottomDialogue", parent, new Color(0.02f, 0.03f, 0.06f, 0.95f));
        RectTransform rt = dialogue.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.05f, 0);
        rt.anchorMax = new Vector2(0.95f, 0);
        rt.pivot = new Vector2(0.5f, 0);
        rt.sizeDelta = new Vector2(0, 100);
        rt.anchoredPosition = new Vector2(0, 15);
        AddOutline(dialogue, accentCyan, 2f);

        // Portrait
        GameObject portrait = new GameObject("Portrait", typeof(RectTransform), typeof(Image));
        portrait.transform.SetParent(dialogue.transform, false);
        RectTransform prt = portrait.GetComponent<RectTransform>();
        prt.anchorMin = new Vector2(0, 0); prt.anchorMax = new Vector2(0, 1);
        prt.offsetMin = new Vector2(10, 10); prt.offsetMax = new Vector2(90, -10);
        portrait.GetComponent<Image>().color = new Color(accentPurple.r, accentPurple.g, accentPurple.b, 0.3f);

        // Dialogue Text
        CreateTMPText("DialogueText", dialogue.transform,
            "* Bienvenido a HeartQuest... Tu aventura comienza aquí.", 20, textWhite,
            TextAlignmentOptions.MidlineLeft,
            new Vector2(0, 0), new Vector2(1, 1),
            new Vector2(100, 10), new Vector2(-15, -10));

        // Añadir DialogueSystem
        var ds = dialogue.AddComponent<DialogueSystem>();
    }

    // ════════════════════════════════════════════════
    // FADE OVERLAY
    // ════════════════════════════════════════════════
    static GameObject CreateFadeOverlay(Transform parent)
    {
        GameObject fade = new GameObject("FadeOverlay", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        fade.transform.SetParent(parent, false);
        StretchFull(fade);
        fade.GetComponent<Image>().color = Color.black;
        fade.transform.SetAsLastSibling();
        return fade;
    }

    // ════════════════════════════════════════════════
    // CONECTAR BOTONES
    // ════════════════════════════════════════════════
    static void ConnectButtons(GameObject leftMenu, MenuController controller)
    {
        var buttons = leftMenu.GetComponentsInChildren<Button>();
        var so = new SerializedObject(controller);
        var menuButtonsProp = so.FindProperty("menuButtons");
        menuButtonsProp.ClearArray();
        int i = 0;

        foreach (var btn in buttons)
        {
            string name = btn.gameObject.name;
            if (name.Contains("Start") || name.Contains("Btn_Start"))
                UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(btn.onClick, new UnityEngine.Events.UnityAction(controller.OnStartGame));
            else if (name.Contains("Continue"))
                UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(btn.onClick, new UnityEngine.Events.UnityAction(controller.OnContinueGame));
            else if (name.Contains("NewGame"))
                UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(btn.onClick, new UnityEngine.Events.UnityAction(controller.OnNewGame));
            else if (name.Contains("LoadGame"))
                UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(btn.onClick, new UnityEngine.Events.UnityAction(controller.OnLoadGame));
            else if (name.Contains("Options"))
                UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(btn.onClick, new UnityEngine.Events.UnityAction(controller.OnOptions));
            else if (name.Contains("Extras"))
                UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(btn.onClick, new UnityEngine.Events.UnityAction(controller.OnExtras));
            else if (name.Contains("Exit"))
                UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(btn.onClick, new UnityEngine.Events.UnityAction(controller.OnExitGame));

            menuButtonsProp.InsertArrayElementAtIndex(i);
            menuButtonsProp.GetArrayElementAtIndex(i).objectReferenceValue = btn;
            i++;
        }
        so.ApplyModifiedProperties();
    }

    // ════════════════════════════════════════════════
    // UTILIDADES
    // ════════════════════════════════════════════════

    static GameObject CreatePanel(string name, Transform parent, Color color)
>>>>>>> Stashed changes
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

    private static Button CreateButton(string name, Transform parent, string textStr, Vector2 pos)
    {
        GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(parent, false);
        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(400, 100);
        rt.anchoredPosition = pos;

        Image img = btnObj.GetComponent<Image>();
        img.color = new Color(0.2f, 0.5f, 0.8f, 1f); // Color azul brillante para botones

        GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
        textObj.transform.SetParent(btnObj.transform, false);
        Text txt = textObj.GetComponent<Text>();
        txt.text = textStr;
        txt.fontSize = 45;
        txt.fontStyle = FontStyle.Bold;
        txt.color = Color.white;
        txt.alignment = TextAnchor.MiddleCenter;
        
        RectTransform textRT = textObj.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        return btnObj.GetComponent<Button>();
    }
}
