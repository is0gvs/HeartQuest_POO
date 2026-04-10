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

        // 2. Configure Slicing (CRITICAL FIX FOR ASSETS)
        string tilesetPath = "Assets/Sprites/school_tileset.png";
        ConfigureSlicing(tilesetPath, 8, 8, true);
        string p1Path = "Assets/Sprites/Characters/blonde_man.png";
        ConfigureSlicing(p1Path, 32, 32, false);
        string p2Path = "Assets/Sprites/Characters/blue_haired_woman.png";
        ConfigureSlicing(p2Path, 32, 32, false);

        // 3. Cleanup Scene safely
        var currentScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
        foreach (var obj in currentScene.GetRootGameObjects())
        {
            if (obj.name != "Main Camera" && obj.name != "Directional Light")
                DestroyImmediate(obj);
        }

        // 4. Grid & 2D Preparation
        GameObject gridObj = new GameObject("Environment_Grid", typeof(Grid));
        // Use 1x1 cells. Our PPU is 8 for 8x8 tiles, so 1 unit = 1 tile exactly.
        gridObj.GetComponent<Grid>().cellSize = new Vector3(1, 1, 0);
        
        GameObject floorObj = new GameObject("Floor", typeof(Tilemap), typeof(TilemapRenderer));
        floorObj.transform.SetParent(gridObj.transform);
        Tilemap floorMap = floorObj.GetComponent<Tilemap>();
        floorObj.GetComponent<TilemapRenderer>().sortingOrder = -20;

        GameObject wallObj = new GameObject("Walls", typeof(Tilemap), typeof(TilemapRenderer));
        wallObj.transform.SetParent(gridObj.transform);
        Tilemap wallMap = wallObj.GetComponent<Tilemap>();
        wallObj.GetComponent<TilemapRenderer>().sortingOrder = -5; // Walls behind player

        // 5. Intelligent Tile Selection
        Object[] tiles = AssetDatabase.LoadAllAssetsAtPath(tilesetPath);
        List<Sprite> s = tiles.OfType<Sprite>().OrderBy(sprite => sprite.name).ToList();
        
        if (s.Count < 50) {
            Debug.LogError("Slicing error: Not enough tiles found. Script will stop to prevent empty worlds.");
            return;
        }

        // Pick specific wood floor (row 1 is generally wood)
        Tile floorTile = ScriptableObject.CreateInstance<Tile>();
        floorTile.sprite = s[1]; 

        Tile wallTile = ScriptableObject.CreateInstance<Tile>();
        wallTile.sprite = s[17];

        Tile boardTile = ScriptableObject.CreateInstance<Tile>();
        boardTile.sprite = s[41];

        // Paint 20x20 Room
        for (int x = -10; x <= 10; x++)
        {
            for (int y = -10; y <= 10; y++)
            {
                floorMap.SetTile(new Vector3Int(x, y, 0), floorTile);
                if (y == 10 || y == -10 || x == -10 || x == 10)
                    wallMap.SetTile(new Vector3Int(x, y, 0), wallTile);
                if (y == 10 && x >= -2 && x <= 2)
                    wallMap.SetTile(new Vector3Int(x, y, 0), boardTile);
            }
        }

        // 6. Player
        GameObject playerObj = new GameObject("PlayerAccountant");
        playerObj.transform.position = Vector3.zero;
        var sr = playerObj.AddComponent<SpriteRenderer>();
        var anim = playerObj.AddComponent<Animator>();
        var pClass = playerObj.AddComponent<Player>();
        
        sr.sortingOrder = 10;
        playerObj.tag = "Player";
        anim.runtimeAnimatorController = AnimationBuilder.GeneratePlayerAnimator(p1Path);
        sr.sprite = AssetDatabase.LoadAllAssetsAtPath(p1Path).OfType<Sprite>().FirstOrDefault();

        // 7. NPC
        GameObject npcObj = new GameObject("Classmate_Sofia");
        npcObj.transform.position = new Vector3(3, 3, 0); // Spaced appropriately
        npcObj.AddComponent<SpriteRenderer>().sortingOrder = 9;
        npcObj.AddComponent<Animator>().runtimeAnimatorController = AnimationBuilder.GeneratePlayerAnimator(p2Path);
        npcObj.AddComponent<Victim>();
        npcObj.GetComponent<SpriteRenderer>().sprite = AssetDatabase.LoadAllAssetsAtPath(p2Path).OfType<Sprite>().FirstOrDefault();

        // 8. Quality Lighting
        GameObject lightObj = new GameObject("GlobalLight");
        var lightType = System.Type.GetType("UnityEngine.Rendering.Universal.Light2D, Unity.RenderPipelines.Universal.Runtime");
        if (lightType != null) {
            var lComponent = lightObj.AddComponent(lightType);
            var colorProp = lightType.GetProperty("color");
            var intensityProp = lightType.GetProperty("intensity");
            if (colorProp != null) colorProp.SetValue(lComponent, new Color(1, 0.98f, 0.9f, 1)); 
            if (intensityProp != null) intensityProp.SetValue(lComponent, 1.1f);
        }

        // 9. UI Canvas (Refined for visibility and scaling)
        GameObject canvasObj = new GameObject("UI_Presentation", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        GameObject panelObj = new GameObject("BackgroundPanel", typeof(RectTransform), typeof(Image));
        panelObj.transform.SetParent(canvasObj.transform, false);
        Image panelImg = panelObj.GetComponent<Image>();
        panelImg.color = new Color(0, 0, 0, 0.85f);
        RectTransform panelRT = panelObj.GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0, 1); panelRT.anchorMax = new Vector2(1, 1);
        panelRT.pivot = new Vector2(0.5f, 1);
        panelRT.sizeDelta = new Vector2(0, 120);

        GameObject titleObj = new GameObject("ProjectTitle", typeof(RectTransform), typeof(Text));
        titleObj.transform.SetParent(panelObj.transform, false);
        Text titleText = titleObj.GetComponent<Text>();
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.text = "PROYECTO POO: ANTI-BULLYING RPG\n(Walk con WASD o Flechas)";
        titleText.fontSize = 40;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = new Color(1f, 0.9f, 0.2f, 1f); // Golden yellow
        RectTransform titleRT = titleObj.GetComponent<RectTransform>();
        titleRT.anchorMin = Vector2.zero; titleRT.anchorMax = Vector2.one;
        titleRT.offsetMin = Vector2.zero; titleRT.offsetMax = Vector2.zero;

        // 10. Camera Configuration (The Fix for the Skybox)
        var cam = Camera.main;
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = 6f; // See more of the room
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
