using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Collections.Generic;

public class BattleSceneSetup : Editor
{
    const string SCENE_PATH = "Assets/Scenes/BattleScene.unity";

    // [MenuItem("POO Game/Setup/Build Battle Scene (AntiBullying)")] // Removido a petición del usuario
    public static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        if (!Directory.Exists("Assets/Scenes")) Directory.CreateDirectory("Assets/Scenes");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ── Camera ────────────────────────────────────────────────────────────
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic = true; cam.orthographicSize = 5f;
        cam.backgroundColor = new Color(0.02f, 0.02f, 0.04f); // Fondo oscuro
        cam.transform.position = new Vector3(0, 0, -10);
        camGO.AddComponent<AudioListener>();

        // ── Sprites helper ────────────────────────────────────────────────────
        Sprite sq = MakeSolidSprite(4);
        Sprite circ = MakeCircleSprite(32);
        
        // Colores de la temática AntiBullying
        Color clrSpeak = new Color(0.9f, 0.3f, 0.3f); // Rojo suave - Hablar
        Color clrSupport = new Color(0.2f, 0.8f, 0.4f); // Verde esperanza - Apoyar
        Color clrBag = new Color(0.3f, 0.6f, 1f); // Azul - Mochila
        Color clrIgnore = new Color(0.8f, 0.7f, 0.2f); // Amarillo - Ignorar

        Sprite btnSpeak = MakeHollowButton(clrSpeak);
        Sprite btnSupport = MakeHollowButton(clrSupport);
        Sprite btnBag = MakeHollowButton(clrBag);
        Sprite btnIgnore = MakeHollowButton(clrIgnore);

        // ── BattleBox ─────────────────────────────────────────────────────────
        var boxGO = new GameObject("BattleBox");
        var boxSR = boxGO.AddComponent<SpriteRenderer>();
        // La caja de batalla tiene borde blanco y fondo negro
        boxSR.sprite = MakeHollowButton(Color.white); 
        boxSR.drawMode = SpriteDrawMode.Sliced;
        boxSR.size = new Vector2(11.5f, 3f); 
        boxSR.color = Color.white;
        boxGO.transform.position = new Vector3(0, -1.7f, 0);
        Wall("Top",    boxGO.transform, new Vector3(0,  1.5f,0),  new Vector2(11.5f,0.1f));
        Wall("Bottom", boxGO.transform, new Vector3(0, -1.5f,0),  new Vector2(11.5f,0.1f));
        Wall("Left",   boxGO.transform, new Vector3(-5.75f,0,0),  new Vector2(0.1f,3f));
        Wall("Right",  boxGO.transform, new Vector3(5.75f, 0,0),  new Vector2(0.1f,3f));

        // ── PlayerSoul ────────────────────────────────────────────────────────
        var soulGO = new GameObject("PlayerSoul");
        var soulSR = soulGO.AddComponent<SpriteRenderer>();

        // Usamos un sprite generado en código para asegurar el corazón perfecto sin depender del package
        soulSR.sprite = MakeHeartSprite();
        
        // Aplicar el color morado que enviaste (Magenta puro / #FF00FF)
        soulSR.color = new Color(1f, 0f, 1f);
        
        soulSR.sortingOrder = 10;
        soulGO.transform.position = new Vector3(0, -1.7f, 0);
        soulGO.transform.localScale = Vector3.one * 1f; // Ajuste para el nuevo sprite
        var soulRb = soulGO.AddComponent<Rigidbody2D>();
        soulRb.gravityScale = 0; soulRb.constraints = RigidbodyConstraints2D.FreezeRotation;
        soulGO.AddComponent<CircleCollider2D>();
        var pVars = soulGO.AddComponent<PlayerVars>();
        var pMove = soulGO.AddComponent<PlayerMovement>();
        pMove.speed = 5f;

        var pvSO = new SerializedObject(pVars);
        pvSO.FindProperty("soulSprite").objectReferenceValue = soulSR;
        pvSO.FindProperty("health").floatValue = 20f;
        pvSO.FindProperty("atkValue").floatValue = 10f;
        pvSO.FindProperty("maxTime").floatValue = 1.5f;
        pvSO.ApplyModifiedProperties();

        // ── Managers root ─────────────────────────────────────────────────────
        var mgr = new GameObject("--- MANAGERS ---");
        var audioGO = new GameObject("AudioManager"); audioGO.transform.parent = mgr.transform;
        var audioMgr = audioGO.AddComponent<AudioManager>();
        audioGO.AddComponent<AudioSource>(); audioGO.AddComponent<AudioSource>();

        var atkGO = new GameObject("AttackManager"); atkGO.transform.parent = mgr.transform;
        var atkMgr = atkGO.AddComponent<AttackManager>();

        var dlgGO = new GameObject("DialogueManager"); dlgGO.transform.parent = mgr.transform;
        var dlgMgr = dlgGO.AddComponent<DialogueManager>();

        var itmGO = new GameObject("ItemManager"); itmGO.transform.parent = mgr.transform;
        var itmMgr = itmGO.AddComponent<ItemManager>();

        var actGO = new GameObject("ActingManager"); actGO.transform.parent = mgr.transform;
        var actMgr = actGO.AddComponent<ActingManager>();

        var bmGO = new GameObject("BattleManager"); bmGO.transform.parent = mgr.transform;
        var bm = bmGO.AddComponent<BattleManager>();

        var goMgrGO = new GameObject("GameOverManager"); goMgrGO.transform.parent = mgr.transform;
        var goMgr = goMgrGO.AddComponent<GameOverManager>();

        // ── UI root ───────────────────────────────────────────────────────────
        var ui = new GameObject("--- BATTLE UI ---");

        var bgGO = new GameObject("Background"); bgGO.transform.parent = ui.transform;
        var bgSR = bgGO.AddComponent<SpriteRenderer>();
        bgSR.sprite = sq; bgSR.drawMode = SpriteDrawMode.Sliced;
        bgSR.size = new Vector2(20,12); bgSR.color = new Color(0.02f, 0.02f, 0.04f); bgSR.sortingOrder = -10;

        var enemyGO = new GameObject("EnemySprite"); enemyGO.transform.parent = ui.transform;
        enemyGO.transform.position = new Vector3(0, 1.8f, 0); enemyGO.transform.localScale = Vector3.one * 2.5f;
        var enSR = enemyGO.AddComponent<SpriteRenderer>();
        enSR.color = Color.white; enSR.sortingOrder = 1;

        // Cargar el sprite y animador del Bully (blonde_man)
        string bullySpritePath = "Assets/Sprites/Characters/blonde_man.png";
        string bullyControllerPath = "Assets/Controllers/blonde_man_Controller.controller";
        
        var bullySprites = AssetDatabase.LoadAllAssetsAtPath(bullySpritePath);
        if (bullySprites.Length > 0)
        {
            enSR.sprite = System.Linq.Enumerable.FirstOrDefault(System.Linq.Enumerable.OfType<Sprite>(bullySprites));
            var anim = enemyGO.AddComponent<Animator>();
            var controller = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(bullyControllerPath);
            if (controller == null) 
            {
                // Si no existe el controlador, usamos tu script AnimationBuilder para crearlo
                controller = AnimationBuilder.GeneratePlayerAnimator(bullySpritePath);
            }
            anim.runtimeAnimatorController = controller;
        }
        else
        {
            enSR.sprite = sq; // Fallback si no encuentra el sprite
        }

        // ── Botones Principales (Nuevos Nombres e Imágenes) ───────────────────
        string[] btnNames  = {"HABLAR", "APOYAR", "MOCHILA", "IGNORAR"};
        // ¡Mayor espacio entre botones!
        float[]  btnX      = {-5.1f, -1.7f, 1.7f, 5.1f};
        Sprite[] btnSprites = {btnSpeak, btnSupport, btnBag, btnIgnore};

        var uiBtnRoot = new GameObject("MainButtons"); uiBtnRoot.transform.parent = ui.transform;
        var mainBtns = new List<Buttons>();
        
        for (int i = 0; i < 4; i++)
        {
            var bGO = new GameObject("Btn_" + btnNames[i]);
            bGO.transform.parent = uiBtnRoot.transform;
            bGO.transform.position = new Vector3(btnX[i], -4f, 0);
            var bSR = bGO.AddComponent<SpriteRenderer>();
            bSR.sprite = btnSprites[i]; bSR.drawMode = SpriteDrawMode.Sliced;
            bSR.size = new Vector2(3.1f, 0.9f); bSR.sortingOrder = 3;

            var txtGO = new GameObject("Label"); txtGO.transform.parent = bGO.transform;
            txtGO.transform.localPosition = Vector3.zero;
            var tmp = txtGO.AddComponent<TMPro.TextMeshPro>();
            tmp.text = btnNames[i]; 
            tmp.rectTransform.sizeDelta = new Vector2(3f, 0.8f);
            tmp.enableAutoSizing = true; tmp.fontSizeMin = 2f; tmp.fontSizeMax = 6f;
            tmp.color = Color.white; tmp.alignment = TMPro.TextAlignmentOptions.Center;
            tmp.fontStyle = TMPro.FontStyles.Bold; tmp.sortingOrder = 4;

            var spGO = new GameObject("SoulPos"); spGO.transform.parent = bGO.transform;
            spGO.transform.localPosition = new Vector3(-1.3f, 0, 0);

            var btn = bGO.AddComponent<Buttons>();
            var btnSO = new SerializedObject(btn);
            btnSO.FindProperty("buttonDeselected").objectReferenceValue = btnSprites[i];
            
            // Creamos una versión "seleccionada" del botón que sea toda del color
            Sprite filledSprite = MakeHollowButton(Color.white); // Al seleccionar se pone blanco el borde
            btnSO.FindProperty("buttonSelected").objectReferenceValue   = filledSprite;
            btnSO.FindProperty("soulPosition").objectReferenceValue     = spGO.transform;
            btnSO.ApplyModifiedProperties();
            mainBtns.Add(btn);
        }

        // ── Sub-Botones de APOYAR (ACT) ──────────────────────────────────────
        var actObjRoot = new GameObject("ActButtons"); actObjRoot.transform.parent = ui.transform; actObjRoot.SetActive(false);
        string[] actNames = {"Consolar", "Defender", "Buscar Ayuda", "Escuchar"};
        float[]  actX     = {-3.5f, -1.0f, 1.5f, 4.0f};
        var actBtns = new List<ActingButtons>();
        for (int i = 0; i < 4; i++)
        {
            var aGO = new GameObject("Act_" + actNames[i]); aGO.transform.parent = actObjRoot.transform;
            aGO.transform.position = new Vector3(actX[i], -4.5f, 0);
            var aSR = aGO.AddComponent<SpriteRenderer>();
            aSR.sprite = MakeHollowButton(clrSupport); aSR.drawMode = SpriteDrawMode.Sliced;
            aSR.size = new Vector2(2.4f,0.7f); aSR.sortingOrder = 3;

            var aTxtGO = new GameObject("Label"); aTxtGO.transform.parent = aGO.transform; aTxtGO.transform.localPosition = Vector3.zero;
            var aTmp = aTxtGO.AddComponent<TMPro.TextMeshPro>();
            aTmp.text = actNames[i];
            aTmp.rectTransform.sizeDelta = new Vector2(2.3f, 0.6f);
            aTmp.enableAutoSizing = true; aTmp.fontSizeMin = 2f; aTmp.fontSizeMax = 5f;
            aTmp.color = Color.white; aTmp.alignment = TMPro.TextAlignmentOptions.Center; aTmp.sortingOrder = 4;

            var aspGO = new GameObject("SoulPos"); aspGO.transform.parent = aGO.transform; aspGO.transform.localPosition = new Vector3(-1.0f,0,0);

            var aVars = aGO.AddComponent<ActVars>();
            var aVarsSO = new SerializedObject(aVars);
            var actTxtProp = aVarsSO.FindProperty("actTxt"); actTxtProp.arraySize = 2;
            actTxtProp.GetArrayElementAtIndex(0).stringValue = $"* Decides {actNames[i].ToLower()} a tu compañero.";
            actTxtProp.GetArrayElementAtIndex(1).stringValue = $"* Intentas {actNames[i].ToLower()} una vez más.";
            var mvProp = aVarsSO.FindProperty("mercyValue"); mvProp.arraySize = 2;
            mvProp.GetArrayElementAtIndex(0).intValue = 25;
            mvProp.GetArrayElementAtIndex(1).intValue = 25;
            aVarsSO.FindProperty("mercyMax").intValue = 100;
            aVarsSO.ApplyModifiedProperties();

            var aBtn = aGO.AddComponent<ActingButtons>();
            var aBtnSO = new SerializedObject(aBtn);
            aBtnSO.FindProperty("soulPosition").objectReferenceValue = aspGO.transform;
            aBtnSO.ApplyModifiedProperties();
            actBtns.Add(aBtn);
        }

        // ── Sub-Botones de MOCHILA (ITEM) ────────────────────────────────────
        var itmObjRoot = new GameObject("ItemButtons"); itmObjRoot.transform.parent = ui.transform; itmObjRoot.SetActive(false);
        string[] itmNames = {"Botella Agua", "Galletas", "Sándwich", "Jugo"};
        float[]  itmHeal  = {5f, 10f, 15f, 8f};
        var itmBtns = new List<ItemButtons>();
        for (int i = 0; i < 4; i++)
        {
            var iGO = new GameObject("Item_" + itmNames[i]); iGO.transform.parent = itmObjRoot.transform;
            iGO.transform.position = new Vector3(btnX[i], -4.5f, 0);
            var iSR = iGO.AddComponent<SpriteRenderer>();
            iSR.sprite = MakeHollowButton(clrBag); iSR.drawMode = SpriteDrawMode.Sliced;
            iSR.size = new Vector2(2.5f,0.7f); iSR.sortingOrder = 3;

            var iTxtGO = new GameObject("Label"); iTxtGO.transform.parent = iGO.transform; iTxtGO.transform.localPosition = Vector3.zero;
            var iTmp = iTxtGO.AddComponent<TMPro.TextMeshPro>();
            iTmp.text = itmNames[i];
            iTmp.rectTransform.sizeDelta = new Vector2(2.4f, 0.6f);
            iTmp.enableAutoSizing = true; iTmp.fontSizeMin = 2f; iTmp.fontSizeMax = 5f;
            iTmp.color = Color.white; iTmp.alignment = TMPro.TextAlignmentOptions.Center; iTmp.sortingOrder = 4;

            var ispGO = new GameObject("SoulPos"); ispGO.transform.parent = iGO.transform; ispGO.transform.localPosition = new Vector3(-1.0f,0,0);

            var iBtn = iGO.AddComponent<ItemButtons>();
            var iBtnSO = new SerializedObject(iBtn);
            iBtnSO.FindProperty("soulPosition").objectReferenceValue = ispGO.transform;
            iBtnSO.FindProperty("itemName").stringValue = itmNames[i];
            iBtnSO.FindProperty("itemHeal").floatValue  = itmHeal[i];
            iBtnSO.ApplyModifiedProperties();
            itmBtns.Add(iBtn);
        }

        // ── Resto de la UI (Textos, Barra HP, etc) ───────────────────────────
        var hpBgGO = new GameObject("HPBar_BG"); hpBgGO.transform.parent = ui.transform; hpBgGO.transform.position = new Vector3(0.5f,-4.7f,0);
        var hpBgSR = hpBgGO.AddComponent<SpriteRenderer>(); hpBgSR.sprite = sq; hpBgSR.drawMode = SpriteDrawMode.Sliced; hpBgSR.size = new Vector2(3f,0.25f); hpBgSR.color = new Color(0.2f,0f,0f); hpBgSR.sortingOrder = 2;

        var hpFillGO = new GameObject("HPBar_Fill"); hpFillGO.transform.parent = ui.transform; hpFillGO.transform.position = new Vector3(0.5f,-4.7f,0);
        var hpFillSR = hpFillGO.AddComponent<SpriteRenderer>(); hpFillSR.sprite = sq; hpFillSR.drawMode = SpriteDrawMode.Sliced; hpFillSR.size = new Vector2(3f,0.25f); hpFillSR.color = new Color(0.2f,0.8f,0.4f); hpFillSR.sortingOrder = 3;

        var hpTxtGO = new GameObject("HPText"); hpTxtGO.transform.parent = ui.transform; hpTxtGO.transform.position = new Vector3(-2f,-4.7f,0);
        var hpTmp = hpTxtGO.AddComponent<TMPro.TextMeshPro>(); hpTmp.text = "ESTADO  20/20"; hpTmp.fontSize = 2f; hpTmp.color = Color.white; hpTmp.sortingOrder = 4;

        var actTxtGO = new GameObject("ActingText"); actTxtGO.transform.parent = ui.transform; actTxtGO.transform.position = new Vector3(-5.2f,-3.2f,0);
        var actTmp = actTxtGO.AddComponent<TMPro.TextMeshPro>(); actTmp.text = "* Te interpones entre Mateo y su víctima."; actTmp.rectTransform.sizeDelta = new Vector2(10f, 2f); actTmp.enableAutoSizing = true; actTmp.fontSizeMin = 2f; actTmp.fontSizeMax = 3.5f; actTmp.color = Color.white; actTmp.sortingOrder = 4;

        var pdTxtGO = new GameObject("PlayerDlgText"); pdTxtGO.transform.parent = ui.transform; pdTxtGO.transform.position = new Vector3(-5.2f,-4.9f,0);
        var pdTmp = pdTxtGO.AddComponent<TMPro.TextMeshPro>(); pdTmp.rectTransform.sizeDelta = new Vector2(10f, 2f); pdTmp.enableAutoSizing = true; pdTmp.fontSizeMin = 2f; pdTmp.fontSizeMax = 3.5f; pdTmp.color = Color.white; pdTmp.sortingOrder = 4;

        var edBgGO = new GameObject("EnemyDlgBG"); edBgGO.transform.parent = ui.transform; edBgGO.transform.position = new Vector3(0f,0.4f,0);
        var edBgSR = edBgGO.AddComponent<SpriteRenderer>(); edBgSR.sprite = MakeHollowButton(Color.white); edBgSR.drawMode = SpriteDrawMode.Sliced; edBgSR.size = new Vector2(8f,1.5f); edBgSR.color = Color.white; edBgSR.sortingOrder = 4; edBgGO.SetActive(false);
        
        var edTxtGO = new GameObject("EnemyDlgText"); edTxtGO.transform.parent = edBgGO.transform; edTxtGO.transform.localPosition = Vector3.zero;
        var edTmp = edTxtGO.AddComponent<TMPro.TextMeshPro>(); edTmp.rectTransform.sizeDelta = new Vector2(7f, 1f); edTmp.enableAutoSizing = true; edTmp.fontSizeMin = 2f; edTmp.fontSizeMax = 4f; edTmp.color = Color.white; edTmp.alignment = TMPro.TextAlignmentOptions.Center; edTmp.sortingOrder = 5;

        var dmgGO = new GameObject("DamageFlash"); dmgGO.transform.parent = ui.transform; var dmgSR = dmgGO.AddComponent<SpriteRenderer>(); dmgSR.sprite = sq; dmgSR.drawMode = SpriteDrawMode.Sliced; dmgSR.size = new Vector2(20,12); dmgSR.color = new Color(1,0,0,0.4f); dmgSR.sortingOrder = 20; dmgGO.SetActive(false);
        var mercyGO = new GameObject("MercyMenu"); mercyGO.transform.parent = ui.transform; mercyGO.SetActive(false);
        var goScreenGO = new GameObject("GameOverScreen"); goScreenGO.transform.parent = ui.transform; var goSR = goScreenGO.AddComponent<SpriteRenderer>(); goSR.sprite = sq; goSR.drawMode = SpriteDrawMode.Sliced; goSR.size = new Vector2(20,12); goSR.color = Color.black; goSR.sortingOrder = 30; goScreenGO.SetActive(false);

        // ══════════════════════════════════════════════════════════════════════
        // CONECTAR TODOS LOS MANAGERS
        // ══════════════════════════════════════════════════════════════════════
        var bmSO = new SerializedObject(bm);
        bmSO.FindProperty("soul").objectReferenceValue        = soulSR;
        bmSO.FindProperty("battleBox").objectReferenceValue   = boxSR;
        bmSO.FindProperty("healthMeter").objectReferenceValue = hpFillGO;
        bmSO.FindProperty("healthTxt").objectReferenceValue   = hpTmp;
        bmSO.FindProperty("damageSprite").objectReferenceValue= dmgGO;
        bmSO.FindProperty("mercyMenu").objectReferenceValue   = mercyGO;
        bmSO.FindProperty("actingMgr").objectReferenceValue   = actMgr;
        bmSO.FindProperty("audioMgr").objectReferenceValue    = audioMgr;
        bmSO.FindProperty("damage").floatValue                = 5f;
        var edProp = bmSO.FindProperty("enemyDialogue"); edProp.arraySize = 3;
        edProp.GetArrayElementAtIndex(0).stringValue = "¡No te metas donde no te llaman!";
        edProp.GetArrayElementAtIndex(1).stringValue = "¿Acaso quieres ser el siguiente?";
        edProp.GetArrayElementAtIndex(2).stringValue = "¡Déjanos en paz!";
        var mainBtnProp = bmSO.FindProperty("buttons"); mainBtnProp.arraySize = 4;
        for (int i = 0; i < 4; i++) mainBtnProp.GetArrayElementAtIndex(i).objectReferenceValue = mainBtns[i];
        bmSO.ApplyModifiedProperties();

        var amSO = new SerializedObject(actMgr);
        amSO.FindProperty("soul").objectReferenceValue        = soulSR;
        amSO.FindProperty("actingText").objectReferenceValue  = actTmp;
        amSO.FindProperty("actObjects").objectReferenceValue  = actObjRoot;
        amSO.FindProperty("spareMessage").stringValue         = "* Mateo respira profundo y baja los brazos. Se ha calmado.";
        amSO.FindProperty("totalMercyMax").intValue           = 100;
        var ftProp = amSO.FindProperty("flavorText"); ftProp.arraySize = 3;
        ftProp.GetArrayElementAtIndex(0).stringValue = "* Intuitivamente, sientes la tensión en el aire.";
        ftProp.GetArrayElementAtIndex(1).stringValue = "* Mateo parece dudar por un segundo.";
        ftProp.GetArrayElementAtIndex(2).stringValue = "* Tratas de mantener la calma.";
        var actBtnProp = amSO.FindProperty("buttons"); actBtnProp.arraySize = 4;
        for (int i = 0; i < 4; i++) actBtnProp.GetArrayElementAtIndex(i).objectReferenceValue = actBtns[i];
        amSO.ApplyModifiedProperties();

        var imSO = new SerializedObject(itmMgr);
        imSO.FindProperty("soul").objectReferenceValue        = soulSR;
        imSO.FindProperty("itemObjects").objectReferenceValue = itmObjRoot;
        var itmBtnProp = imSO.FindProperty("buttons"); itmBtnProp.arraySize = 4;
        for (int i = 0; i < 4; i++) itmBtnProp.GetArrayElementAtIndex(i).objectReferenceValue = itmBtns[i];
        var useTxtGO = new GameObject("UseText"); useTxtGO.transform.parent = ui.transform; useTxtGO.transform.position = new Vector3(-5.2f,-4.9f,0);
        var useTmp = useTxtGO.AddComponent<TMPro.TextMeshPro>(); useTmp.rectTransform.sizeDelta = new Vector2(10f, 2f); useTmp.enableAutoSizing = true; useTmp.fontSizeMin = 2f; useTmp.fontSizeMax = 3.5f; useTmp.color = Color.white; useTmp.sortingOrder = 4;
        imSO.FindProperty("useText").objectReferenceValue = useTmp;
        imSO.ApplyModifiedProperties();

        var dmSO = new SerializedObject(dlgMgr);
        dmSO.FindProperty("text").objectReferenceValue              = pdTmp;
        dmSO.FindProperty("textEnemy").objectReferenceValue         = edTmp;
        dmSO.FindProperty("enemyTextBackground").objectReferenceValue = edBgGO;
        dmSO.FindProperty("talkingSpeed").floatValue                = 0.04f;
        dmSO.ApplyModifiedProperties();

        var goSO = new SerializedObject(goMgr);
        goSO.FindProperty("battleObjects").objectReferenceValue  = ui;
        goSO.FindProperty("gameOverScreen").objectReferenceValue = goScreenGO;
        goSO.ApplyModifiedProperties();

        EditorSceneManager.SaveScene(scene, SCENE_PATH);
        EditorUtility.DisplayDialog("✅ BattleScene Sobreescrita", "He actualizado toda la interfaz con la temática de AntiBullying. ¡Nuevos botones y diálogos listos!", "¡Perfecto!");
    }

    static void Wall(string name, Transform parent, Vector3 pos, Vector2 size)
    {
        var go = new GameObject("Wall_" + name); go.transform.SetParent(parent); go.transform.localPosition = pos;
        var col = go.AddComponent<BoxCollider2D>(); col.size = size;
    }

    static Sprite MakeSolidSprite(int size)
    {
        var tex = new Texture2D(size, size); var pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
        tex.SetPixels(pixels); tex.Apply();
        return Sprite.Create(tex, new Rect(0,0,size,size), new Vector2(0.5f,0.5f), 1f);
    }

    static Sprite MakeCircleSprite(int size)
    {
        var tex = new Texture2D(size, size); var pixels = new Color[size * size];
        var c = new Vector2(size/2f, size/2f); float r = size/2f - 1f;
        for (int i = 0; i < pixels.Length; i++) {
            int x = i % size, y = i / size;
            pixels[i] = Vector2.Distance(new Vector2(x,y),c) <= r ? Color.white : Color.clear;
        }
        tex.SetPixels(pixels); tex.Apply();
        return Sprite.Create(tex, new Rect(0,0,size,size), new Vector2(0.5f,0.5f), size);
    }

    // Crea un botón hueco estilo RPG con borde de color
    static Sprite MakeHollowButton(Color borderColor)
    {
        int w = 128; int h = 32; int border = 4;
        var tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Point;
        var pixels = new Color[w * h];
        for (int y = 0; y < h; y++) {
            for (int x = 0; x < w; x++) {
                if (x < border || x >= w - border || y < border || y >= h - border)
                    pixels[y * w + x] = borderColor;
                else
                    pixels[y * w + x] = new Color(0.05f, 0.05f, 0.08f, 1f); // Fondo gris muy oscuro
            }
        }
        tex.SetPixels(pixels); tex.Apply();
        return Sprite.Create(tex, new Rect(0,0,w,h), new Vector2(0.5f,0.5f), 32f, 0, SpriteMeshType.FullRect, new Vector4(border, border, border, border));
    }

    static Sprite MakeHeartSprite()
    {
        int w = 15; int h = 15;
        var tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Point;
        Color t = Color.clear;
        Color p = Color.white; // Lo pintaremos de morado después
        Color[] pixels = new Color[] {
            t,t,p,p,p,t,t,t,t,t,p,p,p,t,t,
            t,p,p,p,p,p,t,t,t,p,p,p,p,p,t,
            p,p,p,p,p,p,p,t,p,p,p,p,p,p,p,
            p,p,p,p,p,p,p,p,p,p,p,p,p,p,p,
            p,p,p,p,p,p,p,p,p,p,p,p,p,p,p,
            p,p,p,p,p,p,p,p,p,p,p,p,p,p,p,
            t,p,p,p,p,p,p,p,p,p,p,p,p,p,t,
            t,t,p,p,p,p,p,p,p,p,p,p,p,t,t,
            t,t,t,p,p,p,p,p,p,p,p,p,t,t,t,
            t,t,t,t,p,p,p,p,p,p,p,t,t,t,t,
            t,t,t,t,t,p,p,p,p,p,t,t,t,t,t,
            t,t,t,t,t,t,p,p,p,t,t,t,t,t,t,
            t,t,t,t,t,t,t,p,t,t,t,t,t,t,t,
            t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,
            t,t,t,t,t,t,t,t,t,t,t,t,t,t,t
        };
        System.Array.Reverse(pixels);
        tex.SetPixels(pixels); tex.Apply();
        return Sprite.Create(tex, new Rect(0,0,w,h), new Vector2(0.5f,0.5f), 15f);
    }
}
