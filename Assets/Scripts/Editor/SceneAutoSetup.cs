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
    [MenuItem("POO Game/Master Final Delivery Setup")]
    public static void Setup()
    {
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
        ConfigureSingleSprite(classroomBgPath, 16);

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
            // Restaurar la escala de la escena a 1.0
            backgroundObj.transform.localScale = new Vector3(1f, 1f, 1f);
        }

        // --- SISTEMA DE FÍSICAS (BARRERAS INVISIBLES) ---
        var rb = backgroundObj.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;

        // Limite Superior (Pizarra / Muro)
        var topWall = backgroundObj.AddComponent<BoxCollider2D>();
        topWall.size = new Vector2(40f, 6f);
        topWall.offset = new Vector2(0f, 13f);

        // Limite Inferior
        var bottomWall = backgroundObj.AddComponent<BoxCollider2D>();
        bottomWall.size = new Vector2(40f, 2f);
        bottomWall.offset = new Vector2(0f, -14f);

        // Limite Izquierdo
        var leftWall = backgroundObj.AddComponent<BoxCollider2D>();
        leftWall.size = new Vector2(2f, 30f);
        leftWall.offset = new Vector2(-19.5f, 0f);

        // Limite Derecho
        var rightWall = backgroundObj.AddComponent<BoxCollider2D>();
        rightWall.size = new Vector2(2f, 30f);
        rightWall.offset = new Vector2(19.5f, 0f);

        // 5. Build Environment Collision/Grid (Optional for now, but keeping an empty grid)
        GameObject gridObj = new GameObject("Environment_Grid", typeof(Grid));
        gridObj.GetComponent<Grid>().cellSize = new Vector3(1, 1, 0);

        // 6. Player
        GameObject playerObj = new GameObject("PlayerAccountant");
        playerObj.transform.position = new Vector3(0, -2, 0);
        var sr = playerObj.AddComponent<SpriteRenderer>();
        var anim = playerObj.AddComponent<Animator>();
        var pClass = playerObj.AddComponent<Player>();
        
        sr.sortingOrder = 10;
        playerObj.tag = "Player";
        anim.runtimeAnimatorController = AnimationBuilder.GeneratePlayerAnimator(p1Path);
        sr.sprite = AssetDatabase.LoadAllAssetsAtPath(p1Path).OfType<Sprite>().FirstOrDefault();

        // 7. NPC
        GameObject npcObj = new GameObject("Classmate_Sofia");
        npcObj.transform.position = new Vector3(4, -1, 0); // Spaced appropriately
        npcObj.AddComponent<SpriteRenderer>().sortingOrder = 9;
        npcObj.AddComponent<Animator>().runtimeAnimatorController = AnimationBuilder.GeneratePlayerAnimator(p2Path);
        npcObj.AddComponent<Victim>();
        npcObj.GetComponent<SpriteRenderer>().sprite = AssetDatabase.LoadAllAssetsAtPath(p2Path).OfType<Sprite>().FirstOrDefault();

        // 8. Quality Lighting (Removed or kept minimal for 2D Unlit)
        // Ya que usamos Sprites, si no hay material iluminado no se verá el efecto normal,
        // pero podemos eliminar la luz o dejarla si usan URP.

        // 9. UI Canvas (Start Menu & UI System)
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
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
            panelImg.color = Color.white; // No tint
        } else {
            panelImg.color = new Color(0.1f, 0.1f, 0.1f, 1f); // Dark fallback
        }
        
        RectTransform panelRT = panelObj.GetComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero; panelRT.anchorMax = Vector2.one; // Fullscreen
        panelRT.offsetMin = Vector2.zero; panelRT.offsetMax = Vector2.zero;

        // Boton START (Hacemos que sea transparente y cubra TODA la pantalla)
        GameObject btnObj = new GameObject("StartButton", typeof(RectTransform), typeof(Image), typeof(UnityEngine.UI.Button));
        btnObj.transform.SetParent(panelObj.transform, false);
        btnObj.layer = 5; // Layer UI
        Image btnImg = btnObj.GetComponent<Image>();
        btnImg.color = new Color(1f, 1f, 1f, 0f); // 100% Transparente
        RectTransform btnRTC = btnObj.GetComponent<RectTransform>();
        btnRTC.anchorMin = Vector2.zero; 
        btnRTC.anchorMax = Vector2.one; // Fullscreen
        btnRTC.offsetMin = Vector2.zero; 
        btnRTC.offsetMax = Vector2.zero;
        
        // Conectar el sistema con el Script que congela el juego
        var startScript = canvasObj.AddComponent<StartMenu>();
        startScript.startButton = btnObj.GetComponent<UnityEngine.UI.Button>();
        startScript.startMenuPanel = panelObj;

        // 10. Camera Configuration (The Fix for the Skybox)
        var cam = Camera.main;
        if (cam != null)
        {
            cam.orthographic = true;
            // Restauramos el orthographicSize a 5f
            cam.orthographicSize = 5f; 
            cam.clearFlags = CameraClearFlags.SolidColor; // REMOVES THE 3D HORIZON
            cam.backgroundColor = new Color(0.1f, 0.1f, 0.12f);
            
            // Clean up old follow component if exists
            var oldF = cam.gameObject.GetComponent<CameraFollow>();
            if (oldF != null) DestroyImmediate(oldF);

            var follow = cam.gameObject.AddComponent<CameraFollow>();
            follow.target = playerObj.transform;
            follow.smoothSpeed = 0.1f;
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(currentScene);
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
