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
