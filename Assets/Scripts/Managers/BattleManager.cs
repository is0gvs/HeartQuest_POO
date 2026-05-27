using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class BattleManager : MonoBehaviour
{
    [HideInInspector]
    public bool isFighting;
    [HideInInspector]
    public bool isHablando;
    private Coroutine resizeCoroutine;
    private bool isResizing;
    private string actMenuPrevText = "";
    private static readonly Vector2 BoxRectangleSize = new Vector2(10.8f, 3.4f);
    [Tooltip("Desactiva para ajustar el layout manualmente en la escena")]
    [SerializeField] private bool overrideLayout = true;
    [HideInInspector]
    public static BattleManager battleInstance;
    private AttackManager attackMgr;
    public ActingManager actingMgr;
    public AudioManager audioMgr;
    void Awake() => battleInstance = this;
    public SpriteRenderer soul;
    public SpriteRenderer battleBox;
    public List<Buttons> buttons;
    int maxSelectionInt;
    int minSelectionInt;
    int selectionInt;
    public GameObject mercyMenu;
    public GameObject damageSprite;
    const float SIZE_INCREASE = 18f;
    public List<string> enemyDialogue;
    [HideInInspector]
    public Action isFinished;
    public Attacking attackingSys;
    PlayerVars playerVariables;
    public GameObject healthMeter;
    public TextMeshPro healthTxt;
    private SpriteRenderer healthFillRenderer;
    private SpriteRenderer healthBackRenderer;
    private EnemyVars enemyVariables;
    private const float HealthBarWidth = 4.2f;
    private const float HealthBarHeight = 0.35f;
    private const string BullyResolvedNpcId = "bully_mateo_reformed";
    public float damage;
    [Header("Configuración de Combate Dinámico")]
    public AntiBullyingGame.Core.EnemyCombatData activeEnemyData;
    /// <summary>
    /// Opens the HABLAR submenu. HablarManager auto-creates if not in scene.
    /// </summary>
    public void Hablar()
    {
        if (!isFighting)
        {
            actingMgr.actingText.gameObject.SetActive(false);
            AudioManager.instance.Selecting();
            HablarManager.instance.AbrirMenuHablar();
        }
    }
    /// <summary>
    /// The acting method, gets called when the player selects the act button, responsible for initiating acts
    /// </summary>
    public void Acting()
    {
        actingMgr.actObjects.SetActive(true);
        actingMgr.isActing = true;
        actingMgr.time = 0f; // Reiniciar tiempo al abrir
        SetMainButtonsVisible(false);
        if (actingMgr.actingText != null)
        {
            actMenuPrevText = actingMgr.actingText.text;
            actingMgr.actingText.gameObject.SetActive(true);
            actingMgr.actingText.text = "* Presiona X para volver.";
        }
        audioMgr.Selecting();
    }
    /// <summary>
    /// The item method, does nothing so far
    /// </summary>
    public void Item()
    {
        ItemManager.instance.isMenu = true;
        ItemManager.instance.canAct = true;
        ItemManager.instance.time = 0f; // Reiniciar tiempo al abrir
        isFighting = false;
        ItemManager.instance.itemObjects.SetActive(true);
        SetMainButtonsVisible(false);
        if (ItemManager.instance.useText != null)
        {
            ItemManager.instance.useText.gameObject.SetActive(true);
            ItemManager.instance.useText.text = "* Presiona X para volver.";
        }
        audioMgr.Selecting();
    }
    /// <summary>
    /// Muestra u oculta los botones principales (HABLAR, APOYAR, MOCHILA, IGNORAR).
    /// </summary>
    public void SetMainButtonsVisible(bool visible)
    {
        if (buttons == null) return;
        foreach (var btn in buttons)
            if (btn != null) btn.gameObject.SetActive(visible);
    }
    /// <summary>
    /// Cierra el menú de la MOCHILA y regresa a los botones principales sin usar item.
    /// </summary>
    public void CloseItemMenu()
    {
        if (ItemManager.instance != null)
        {
            ItemManager.instance.isMenu = false;
            ItemManager.instance.canAct = true;
            if (ItemManager.instance.itemObjects != null)
                ItemManager.instance.itemObjects.SetActive(false);
            if (ItemManager.instance.useText != null)
            {
                ItemManager.instance.useText.text = "";
                ItemManager.instance.useText.gameObject.SetActive(false);
            }
        }
        SetMainButtonsVisible(true);
        if (audioMgr != null) audioMgr.Selecting();
    }
    /// <summary>
    /// Cierra el submenú de APOYAR y regresa a los botones principales sin realizar un acto.
    /// Espejo de CloseItemMenu para mantener el mismo patrón de navegación.
    /// </summary>
    public void CloseActingMenu()
    {
        if (actingMgr != null)
        {
            actingMgr.isActing = false;
            actingMgr.canAct = true;
            if (actingMgr.actObjects != null)
                actingMgr.actObjects.SetActive(false);
            if (actingMgr.actingText != null)
            {
                actingMgr.actingText.text = actMenuPrevText;
                actingMgr.actingText.gameObject.SetActive(true);
            }
        }
        SetMainButtonsVisible(true);
        if (audioMgr != null) audioMgr.Selecting();
    }
    /// <summary>
    /// The mercy method, as of right now it ends the battle but really it doesn't do anything special
    /// </summary>
    public void Mercy()
    {
        if (actingMgr.totalMercy >= actingMgr.totalMercyMax)
        {
            CompleteBullyEncounter("* Mateo baja la mirada y te devuelve la mochila.");
            audioMgr.Selecting();
            return;
        }
        DialogueManager.instance.dialogueTxt = "* Intentas ignorar las palabras de Mateo.";
        DialogueManager.instance.shouldTalk = false;
        DialogueManager.instance.Talking(() => StartCoroutine(CalmSequence(
            "* Respiras hondo y no respondes a la provocacion.",
            "* Mateo se queda esperando una reaccion que no llega.",
            4)));
        audioMgr.Selecting();
    }
    void Start()
    {
        selectionInt = 0;
        maxSelectionInt = 3;
        minSelectionInt = 0;
        playerVariables = FindAnyObjectByType<PlayerVars>();
        // Cargar datos de combate dinámicamente
        string enemyName = PlayerPrefs.GetString("CurrentEnemy", "Mateo el Bully");
        if (activeEnemyData == null)
        {
            activeEnemyData = Resources.Load<AntiBullyingGame.Core.EnemyCombatData>("CombatData/" + enemyName);
        }
        // De respaldo, si no se encuentra el asset, creamos una configuración para Mateo
        if (activeEnemyData == null)
        {
            activeEnemyData = ScriptableObject.CreateInstance<AntiBullyingGame.Core.EnemyCombatData>();
            activeEnemyData.enemyName = enemyName;
            activeEnemyData.maxHP = 32f;
            activeEnemyData.attackValue = 5f;
            activeEnemyData.defendValue = 0f;
            activeEnemyData.resolvedSaveId = "bully_mateo_reformed";
            activeEnemyData.spareMessage = "* Mateo baja la mirada y te devuelve la mochila.";
            activeEnemyData.flavorTexts = new string[] {
                "* Intuitivamente, sientes la tension en el aire.",
                "* Mateo parece dudar por un segundo.",
                "* Tratas de mantener la calma."
            };
        }
        // Asegurar que exista el componente EnemyVars en la escena
        enemyVariables = FindAnyObjectByType<EnemyVars>();
        if (enemyVariables == null)
        {
            GameObject enemySpriteGO = GameObject.Find("EnemySprite");
            if (enemySpriteGO != null)
            {
                enemyVariables = enemySpriteGO.AddComponent<EnemyVars>();
            }
            else
            {
                GameObject varsGO = new GameObject("EnemyVars_Runtime");
                enemyVariables = varsGO.AddComponent<EnemyVars>();
            }
        }
        // Inicializar stats del enemigo
        if (enemyVariables != null)
        {
            enemyVariables.maxHP = activeEnemyData.maxHP;
            enemyVariables.curHP = activeEnemyData.maxHP;
            enemyVariables.attackValue = activeEnemyData.attackValue;
            enemyVariables.defendValue = activeEnemyData.defendValue;
        }
        // Actualizar sprite si está configurado
        if (activeEnemyData.enemySprite != null)
        {
            GameObject enemySpriteGO = GameObject.Find("EnemySprite");
            if (enemySpriteGO != null)
            {
                SpriteRenderer sr = enemySpriteGO.GetComponent<SpriteRenderer>();
                if (sr != null) sr.sprite = activeEnemyData.enemySprite;
            }
        }
        // Configurar ActingManager
        if (actingMgr != null)
        {
            actingMgr.spareMessage = activeEnemyData.spareMessage;
            actingMgr.flavorText = new List<string>(activeEnemyData.flavorTexts);
        }
        // Configurar diálogos del enemigo
        if (activeEnemyData.hablarOpciones != null && activeEnemyData.hablarOpciones.Length > 0)
        {
            enemyDialogue = new List<string>();
            foreach (var opt in activeEnemyData.hablarOpciones)
            {
                if (opt.startsBattle && !string.IsNullOrEmpty(opt.enemyResponse))
                {
                    enemyDialogue.Add(opt.enemyResponse);
                }
            }
        }
        if (enemyDialogue == null || enemyDialogue.Count == 0)
        {
            enemyDialogue = new List<string> {
                "¡No te metas donde no te llaman!",
                "¿Acaso quieres ser el siguiente?",
                "¡Déjanos en paz!"
            };
        }
        if (overrideLayout) NormalizeBattleLayout();
        NormalizeButtonFonts();
        // Inspector-first: use the singleton set in Awake.
        // Fallback only if Inspector ref was missed.
        attackMgr = AttackManager.instance;
        if (attackMgr == null)
        {
            attackMgr = FindAnyObjectByType<AttackManager>();
            if (attackMgr != null)
                Debug.LogWarning("BattleManager: 'attackMgr' no estaba asignado. Se encontró por fallback. Asígnalo en el Inspector.");
            else
                Debug.LogWarning("BattleManager: No existe AttackManager en la escena. El turno del enemigo se saltará.");
        }
        if (attackingSys == null)
        {
            attackingSys = FindAnyObjectByType<Attacking>();
            if (attackingSys != null)
                Debug.LogWarning("BattleManager: 'attackingSys' no estaba asignado. Se encontró por fallback. Asígnalo en el Inspector.");
            else
                Debug.LogWarning("BattleManager: No existe Attacking en la escena. El minijuego de ataque se saltará.");
        }
    }
    private bool axisInUse = false;
    void Update()
    {
        //when the player is not fighting nor acting nor in HABLAR submenu, these if statements get called
        if (!isFighting && !actingMgr.isActing && !ItemManager.instance.isMenu && !isHablando)
        {
            // Red de seguridad: en el menú principal la caja SIEMPRE debe ser rectangular.
            // Si quedó chica (resize interrumpido) y no hay animación en curso, la corregimos.
            if (!isResizing && battleBox != null && battleBox.size != BoxRectangleSize)
            {
                battleBox.size = BoxRectangleSize;
            }
            if (selectionInt > maxSelectionInt)
            {
                selectionInt = 0;
            }
            if (selectionInt < minSelectionInt)
            {
                selectionInt = 3;
            }
            // Joystick/Dpad Axis Navigation
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            bool leftPressed = Input.GetKeyDown(KeyCode.LeftArrow);
            bool rightPressed = Input.GetKeyDown(KeyCode.RightArrow);
            if (horizontalInput == 0f)
            {
                axisInUse = false;
            }
            else if (!axisInUse)
            {
                if (horizontalInput < -0.5f)
                {
                    leftPressed = true;
                    axisInUse = true;
                }
                else if (horizontalInput > 0.5f)
                {
                    rightPressed = true;
                    axisInUse = true;
                }
            }
            if (leftPressed)
            {
                selectionInt--;
            }
            if (rightPressed)
            {
                selectionInt++;
            }
            Selection();
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.JoystickButton0))
            {
                Selected();
            }
        }
        // HeartMinigame owns soul movement during enemy turns; PlayerMovement would
        // fight the gravity/jump controller and make the dodge minigame unstable.
        if (playerVariables != null)
        {
            var pm = playerVariables.GetComponent<PlayerMovement>();
            if (pm != null) pm.enabled = false;
        }
        // Bug fix: Null-guard to prevent NullReferenceException every frame when
        // any Inspector reference is missing.
        if (playerVariables != null && healthTxt != null && healthMeter != null)
        {
            float maxHealth = Mathf.Max(1f, playerVariables.maxHealth);
            float currentHealth = Mathf.Clamp(playerVariables.health, 0f, maxHealth);
            float ratio = Mathf.Clamp01(currentHealth / maxHealth);
            healthTxt.text = $"VIDA {Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
            if (healthFillRenderer != null)
            {
                healthFillRenderer.size = new Vector2(HealthBarWidth * ratio, HealthBarHeight);
                healthFillRenderer.transform.position = new Vector3(-HealthBarWidth * (1f - ratio) * 0.5f, -4.45f, 0f);
                healthFillRenderer.color = ratio > 0.45f ? new Color(0.2f, 0.9f, 0.35f, 1f) :
                    ratio > 0.2f ? new Color(1f, 0.75f, 0.15f, 1f) : new Color(1f, 0.15f, 0.25f, 1f);
            }
        }
    }
    private void NormalizeButtonFonts()
    {
        // Botones principales (HABLAR, APOYAR, MOCHILA, IGNORAR)
        if (buttons != null)
        {
            foreach (var btn in buttons)
            {
                if (btn == null) continue;
                TextMeshPro label = btn.GetComponentInChildren<TextMeshPro>();
                if (label != null)
                {
                    label.fontSize = 1.35f;
                    label.enableAutoSizing = true;
                    label.fontSizeMin = 0.82f;
                    label.fontSizeMax = 1.42f;
                    label.rectTransform.sizeDelta = new Vector2(2.25f, 0.8f);
                    label.alignment = TextAlignmentOptions.Center;
                }
            }
        }
        // Botones de items (MOCHILA submenu)
        if (ItemManager.instance != null && ItemManager.instance.itemObjects != null)
        {
            foreach (Transform child in ItemManager.instance.itemObjects.transform)
            {
                TextMeshPro label = child.GetComponentInChildren<TextMeshPro>();
                if (label != null)
                {
                    label.fontSize = 1.8f;
                    label.enableAutoSizing = true;
                    label.fontSizeMin = 1.0f;
                    label.fontSizeMax = 1.9f;
                    label.rectTransform.sizeDelta = new Vector2(2.35f, 1.35f);
                }
            }
        }
        // Botones de acts (APOYAR submenu)
        if (actingMgr != null && actingMgr.actObjects != null)
        {
            foreach (Transform child in actingMgr.actObjects.transform)
            {
                TextMeshPro label = child.GetComponentInChildren<TextMeshPro>();
                if (label != null)
                {
                    label.fontSize = 1.8f;
                    label.enableAutoSizing = true;
                    label.fontSizeMin = 1.0f;
                    label.fontSizeMax = 1.9f;
                    label.rectTransform.sizeDelta = new Vector2(2.35f, 1.35f);
                }
            }
        }
    }
    private void NormalizeBattleLayout()
    {
        if (soul != null)
        {
            soul.enabled = false;
            soul.transform.localScale = Vector3.one * 0.28f;
            soul.sortingOrder = 12;
        }
        if (battleBox != null)
        {
            battleBox.transform.position = new Vector3(0f, -1.2f, 0f);
            battleBox.size = BoxRectangleSize;
        }
        if (actingMgr != null && actingMgr.actingText != null && battleBox != null)
        {
            TextMeshPro text = actingMgr.actingText;
            text.transform.position = battleBox.transform.position + new Vector3(-3.65f, 0.78f, 0f);
            text.rectTransform.sizeDelta = new Vector2(7.45f, 1.7f);
            text.rectTransform.pivot = new Vector2(0f, 1f);
            text.alignment = TextAlignmentOptions.TopLeft;
            text.enableWordWrapping = true;
            text.fontSize = 1.75f;
            text.lineSpacing = -2f;
            text.enableAutoSizing = false;
        }
        if (healthMeter != null)
        {
            healthMeter.transform.position = new Vector3(0f, -4.45f, 0f);
            healthMeter.transform.localScale = Vector3.one;
            healthFillRenderer = healthMeter.GetComponent<SpriteRenderer>();
            if (healthFillRenderer != null)
            {
                healthFillRenderer.drawMode = SpriteDrawMode.Sliced;
                healthFillRenderer.size = new Vector2(HealthBarWidth, HealthBarHeight);
                healthFillRenderer.color = new Color(0.2f, 0.9f, 0.35f, 1f);
                healthFillRenderer.sortingOrder = 7;
                GameObject bg = new GameObject("HPBar_RuntimeBG");
                bg.transform.position = healthMeter.transform.position + new Vector3(0f, 0f, 0.05f);
                healthBackRenderer = bg.AddComponent<SpriteRenderer>();
                healthBackRenderer.sprite = healthFillRenderer.sprite;
                healthBackRenderer.drawMode = SpriteDrawMode.Sliced;
                healthBackRenderer.size = new Vector2(HealthBarWidth + 0.2f, HealthBarHeight + 0.18f);
                healthBackRenderer.color = new Color(0.05f, 0.05f, 0.05f, 1f);
                healthBackRenderer.sortingOrder = 6;
            }
        }
        if (healthTxt != null)
        {
            healthTxt.transform.position = new Vector3(-3.3f, -4.43f, 0f);
            healthTxt.fontSize = 1.1f;
            healthTxt.enableAutoSizing = false;
            healthTxt.color = Color.white;
            healthTxt.alignment = TextAlignmentOptions.Left;
            healthTxt.sortingOrder = 9;
        }
        PositionSubmenu(itemObjects: ItemManager.instance != null ? ItemManager.instance.itemObjects : null, y: -3.18f);
        PositionSubmenu(actingMgr != null ? actingMgr.actObjects : null, -3.18f);
        PositionMainButtons();
    }
    private void PositionMainButtons()
    {
        if (buttons == null || buttons.Count == 0) return;
        float[] xs = { -3.45f, -1.15f, 1.15f, 3.45f };
        for (int i = 0; i < buttons.Count && i < xs.Length; i++)
        {
            if (buttons[i] == null) continue;
            buttons[i].transform.position = new Vector3(xs[i], -3.62f, 0f);
            buttons[i].transform.localScale = new Vector3(0.72f, 0.72f, 1f);
            TextMeshPro label = buttons[i].GetComponentInChildren<TextMeshPro>();
            if (label != null)
            {
                label.fontSize = 1.35f;
                label.enableAutoSizing = true;
                label.fontSizeMin = 0.82f;
                label.fontSizeMax = 1.42f;
                label.rectTransform.sizeDelta = new Vector2(2.25f, 0.8f);
                label.alignment = TextAlignmentOptions.Center;
            }
            if (buttons[i].soulPosition != null)
            {
                buttons[i].soulPosition.position = buttons[i].transform.position + new Vector3(-0.65f, 0f, 0f);
            }
        }
    }
    private void PositionSubmenu(GameObject itemObjects, float y)
    {
        if (itemObjects == null) return;
        float[] xs = { -3.45f, -1.15f, 1.15f, 3.45f };
        for (int i = 0; i < itemObjects.transform.childCount && i < xs.Length; i++)
        {
            Transform child = itemObjects.transform.GetChild(i);
            child.position = new Vector3(xs[i], y, 0f);
            TextMeshPro label = child.GetComponentInChildren<TextMeshPro>();
            if (label != null)
            {
                label.fontSize = 1.8f;
                label.enableAutoSizing = true;
                label.fontSizeMin = 1.0f;
                label.fontSizeMax = 1.9f;
                label.rectTransform.sizeDelta = new Vector2(2.35f, 1.35f);
            }
        }
    }
    /// <summary>
    /// The method responsible for selecting, when a method is selected the soul will be positioned on the selected button.
    /// </summary>
    /// <param name="selectedInt"></param>
    void Selecting(int selectedInt)
    {
        if (buttons[selectedInt].selected)
        {
            buttons[selectedInt].currentSprite = buttons[selectedInt].buttonSelected;
            if (soul != null) soul.enabled = false;
        }
        else
        {
            buttons[selectedInt].currentSprite = buttons[selectedInt].buttonDeselected;
        }
    }
    /// <summary>
    /// The method responsible for deSelecting
    /// </summary>
    /// <param name="deselectionInt"></param>
    void Deselecting(int deselectionInt)
    {
        buttons[deselectionInt].selected = false;
        buttons[deselectionInt].currentSprite = buttons[deselectionInt].buttonDeselected;
    }
    /// <summary>
    /// The method that sets up the selection, a selectionInt of 0 means selecting the fight button, 1 is for acting, 2 for item, & 3 for mercy
    /// </summary>
    void Selection()
    {
        if (selectionInt == 0)
        {
            if (!buttons[selectionInt].selected)
            {
                //the "hover" sfx gets played
                audioMgr.Hovering();
            }
            buttons[selectionInt].selected = true;
            Selecting(0);
           
        }
        else
        {
            Deselecting(0);
        }
        if (selectionInt == 1)
        {
            if (!buttons[selectionInt].selected)
            {
                audioMgr.Hovering();
            }
            buttons[selectionInt].selected = true;
            Selecting(1);
        }
        else
        {
            Deselecting(1);
        }
        if (selectionInt == 2)
        {
            if (!buttons[selectionInt].selected)
            {
                audioMgr.Hovering();
            }
            buttons[selectionInt].selected = true;
            Selecting(2);
        }
        else
        {
            Deselecting(2);
        }
        if (selectionInt == 3)
        {
            if (!buttons[selectionInt].selected)
            {
                audioMgr.Hovering();
            }
            buttons[selectionInt].selected = true;
            Selecting(3);
        }
        else
        {
            Deselecting(3);
        }
    }
    /// <summary>
    /// The last method, this method calls the methods in the start to initiate each respective action of said button.
    /// </summary>
    void Selected()
    {
        if (selectionInt == 0)
        {
            Hablar();
        }
        if (selectionInt == 1)
        {
            Acting();
        }
        if (selectionInt == 2)
        {
            Item();
        }
        if (selectionInt == 3)
        {
            Mercy();
        }
    }
    /// <summary>
    /// The attack coroutine, responsible for initiating the attacks
    /// </summary>
    /// <returns></returns>
    IEnumerator AttackSequence()
    {
        isFighting = true;
        Action onBoxFinish = () =>
        {
            actingMgr.actingText.gameObject.SetActive(true);
        };
        isFinished = () =>
        {
            SafeResize(BoxRectangleSize, onBoxFinish);
            attackMgr.attackFinished = !attackMgr.attackFinished;
            isFighting = false;
        };
        playerVariables.GetComponent<SpriteRenderer>().enabled = false;
        if (attackingSys != null)
        {
            attackingSys.StartAttacking(playerVariables.atkValue);
            yield return new WaitForSeconds(attackingSys.maxTime);
        }
        else
        {
            Debug.LogWarning("No hay objeto Attacking en la escena. Saltando minijuego de ataque del jugador para evitar errores.");
            yield return new WaitForSeconds(1f);
        }
        if (enemyVariables != null && enemyVariables.curHP <= 0f)
        {
            CompleteBullyEncounter("* Mateo deja de pelear y te devuelve la mochila.");
            yield break;
        }
        playerVariables.transform.position = battleBox != null ? battleBox.transform.position : new Vector3(0, -1.7f, 0);
        SafeResize(new Vector2(3, 3), null);
        actingMgr.actingText.gameObject.SetActive(false);
        playerVariables.GetComponent<SpriteRenderer>().enabled = true;
        if (attackMgr != null && attackMgr.attacksScriptable != null)
        {
            yield return RunHeartDefense();
            isFinished?.Invoke();
        }
        else
        {
            Debug.LogWarning("Falta AttackManager en la escena. Usando defensa de palabras.");
            yield return RunHeartDefense();
            isFinished?.Invoke();
        }
    }
  public IEnumerator ActingSequence()
    {
        //Action that gets called once we finish resizing
        Action boxAction = () =>
        {
            //A pretty simple system, it checks for our mercy value, if we meet the requirement for sparing, a spare message appears, otherwise you'll get a new flavour text.
            if (actingMgr.totalMercy >= actingMgr.totalMercyMax)
            {
                DialogueManager.instance.dialogueTxt = actingMgr.spareMessage;
            }
            else
            {
                DialogueManager.instance.dialogueTxt = actingMgr.flavorText[UnityEngine.Random.Range(0, actingMgr.flavorText.Count)];
            }
            
        };
        //The selection soul, gets disabled in preperation for the attack this round.
        soul.enabled = false;
        //The acting menu gets disabled, as we are about to start the round.
        //Action that gets called once we finish the round.
        isFinished = () =>
        {
            soul.enabled = false;
            soul.transform.position = buttons[1].soulPosition.position;
            SafeResize(BoxRectangleSize, boxAction);
            actingMgr.isActing = false;
            isFighting = false;
            actingMgr.actingText.gameObject.SetActive(true);
            DialogueManager.instance.Talking(null);
            actingMgr.actObjects.SetActive(false);
            actingMgr.canAct = true;
        };
        yield return new WaitForSeconds(1);
        playerVariables.transform.position = battleBox != null ? battleBox.transform.position : new Vector3(0, -1.7f, 0);
        SafeResize(new Vector2(3, 3), boxAction);
        actingMgr.actingText.gameObject.SetActive(false);
        actingMgr.isActing = false;
        isFighting = true;
        actingMgr.time = 0;
        yield return RunHeartDefense();
        if (actingMgr != null && actingMgr.totalMercy >= actingMgr.totalMercyMax)
        {
            CompleteBullyEncounter("* Mateo entiende que se equivoco y te devuelve la mochila.");
            yield break;
        }
        isFinished?.Invoke();
    }
    public IEnumerator ItemSequence()
    {
        //The selection soul, gets disabled in preperation for the attack this round.
        soul.enabled = false;
        //The acting menu gets disabled, as we are about to start the round.
        //Action that gets called once we finish the round.
        isFinished = () =>
        {
            if (actingMgr.totalMercy >= actingMgr.totalMercyMax)
            {
                DialogueManager.instance.dialogueTxt = actingMgr.spareMessage;
            }
            else
            {
                DialogueManager.instance.dialogueTxt = actingMgr.flavorText[UnityEngine.Random.Range(0, actingMgr.flavorText.Count)];
            }
            ItemManager.instance.time = 0;
            soul.enabled = false;
            soul.transform.position = buttons[2].soulPosition.position;
            SafeResize(BoxRectangleSize, null);
            actingMgr.isActing = false;
            isFighting = false;
            actingMgr.actingText.gameObject.SetActive(true);
            DialogueManager.instance.Talking(null);
            actingMgr.actObjects.SetActive(false);
            ItemManager.instance.itemObjects.SetActive(false);
            actingMgr.canAct = true;
        };
        yield return new WaitForSeconds(1);
        // No hay minijuego al usar un item: la caja se mantiene rectangular.
        SafeResize(BoxRectangleSize, null);
        ItemManager.instance.itemObjects.SetActive(false);
        SetMainButtonsVisible(true);
        isFighting = true;
        ItemManager.instance.isMenu = false;
        ItemManager.instance.useText.text = "";
        actingMgr.time = 0;
        yield return CalmSequence(
            "* Te tomas un segundo para recuperar el aliento.",
            "* Mateo intenta molestarte, pero no logra sacarte de foco.",
            0);
    }
    private IEnumerator IgnoreSequence()
    {
        if (actingMgr != null && actingMgr.actingText != null)
        {
            actingMgr.actingText.gameObject.SetActive(false);
        }
        isFighting = true;
        SafeResize(new Vector2(3f, 3f), null);
        yield return RunHeartDefense();
        isFighting = false;
        if (soul != null)
        {
            soul.enabled = false;
            soul.transform.position = buttons[3].soulPosition.position;
        }
        SafeResize(BoxRectangleSize, () =>
        {
            if (actingMgr != null && actingMgr.actingText != null)
            {
                actingMgr.actingText.text = "* Mateo sigue intentando provocarte.";
                actingMgr.actingText.gameObject.SetActive(true);
            }
        });
    }
    public IEnumerator CalmSequence(string playerLine, string mateoLine, int mercyGain)
    {
        if (mercyGain > 0 && actingMgr != null)
        {
            actingMgr.totalMercy = Mathf.Min(actingMgr.totalMercy + mercyGain, actingMgr.totalMercyMax);
        }
        isFighting = false;
        if (soul != null) soul.enabled = false;
        DialogueManager.instance.shouldTalk = false;
        DialogueManager.instance.dialogueTxt = playerLine;
        bool firstDone = false;
        DialogueManager.instance.Talking(() => firstDone = true);
        yield return new WaitUntil(() => firstDone);
        DialogueManager.instance.dialogueTxt = mateoLine;
        bool secondDone = false;
        DialogueManager.instance.Talking(() => secondDone = true);
        yield return new WaitUntil(() => secondDone);
        if (actingMgr != null)
        {
            actingMgr.isActing = false;
            actingMgr.canAct = true;
            if (actingMgr.actObjects != null) actingMgr.actObjects.SetActive(false);
            if (actingMgr.actingText != null)
            {
                actingMgr.actingText.text = "* La tension baja un poco.";
                actingMgr.actingText.gameObject.SetActive(true);
            }
        }
        if (actingMgr != null && actingMgr.totalMercy >= actingMgr.totalMercyMax)
        {
            CompleteBullyEncounter("* Mateo entiende que se equivoco y te devuelve la mochila.");
            yield break;
        }
        if (ItemManager.instance != null)
        {
            ItemManager.instance.isMenu = false;
            ItemManager.instance.canAct = true;
            if (ItemManager.instance.itemObjects != null) ItemManager.instance.itemObjects.SetActive(false);
        }
    }
    private IEnumerator RunHeartDefense()
    {
        bool done = false;
        HeartMinigame.instance.StartMinigame(() => done = true);
        float timeout = HeartMinigame.instance.minigameDuration + 1.5f;
        float elapsed = 0f;
        while (!done && elapsed < timeout)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        if (!done)
        {
            Debug.LogWarning("BattleManager: el minijuego no reportó fin a tiempo. Cerrando turno por timeout.");
            HeartMinigame.instance.StopMinigame();
        }
    }
    public void CompleteBullyEncounter(string message)
    {
        StopAllCoroutines();
        isFighting = false;
        isHablando = false;
        string resolvedId = activeEnemyData != null ? activeEnemyData.resolvedSaveId : BullyResolvedNpcId;
        if (AntiBullyingGame.Managers.SaveManager.Instance != null &&
            !string.IsNullOrEmpty(resolvedId) &&
            !AntiBullyingGame.Managers.SaveManager.Instance.interactedNPCs.Contains(resolvedId))
        {
            AntiBullyingGame.Managers.SaveManager.Instance.interactedNPCs.Add(resolvedId);
        }
        if (soul != null) soul.enabled = false;
        if (actingMgr != null && actingMgr.actObjects != null) actingMgr.actObjects.SetActive(false);
        if (ItemManager.instance != null && ItemManager.instance.itemObjects != null) ItemManager.instance.itemObjects.SetActive(false);
        HeartMinigame.instance.StopMinigame();
        // Arranca tras StopAllCoroutines para que sobreviva y garantice el regreso al mundo.
        StartCoroutine(EndEncounterRoutine(message));
    }
    /// <summary>
    /// Muestra el mensaje de cierre y regresa a la escena del mundo. Usa un timeout
    /// para no quedarse atascado en la batalla si la narración falla.
    /// </summary>
    private IEnumerator EndEncounterRoutine(string message)
    {
        DialogueManager.instance.shouldTalk = false;
        DialogueManager.instance.dialogueTxt = message;
        bool shown = false;
        DialogueManager.instance.ForceTalk(() => shown = true);
        float elapsed = 0f;
        while (!shown && elapsed < 5f)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        // Pausa para que el jugador alcance a leer el mensaje antes de cambiar de escena.
        yield return new WaitForSecondsRealtime(1.5f);
        ReturnToPreviousScene();
    }
    private void ReturnToPreviousScene()
    {
        // Prioridad 1: la escena configurada en el EnemyCombatData del enemigo
        if (activeEnemyData != null && !string.IsNullOrWhiteSpace(activeEnemyData.nextSceneName))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(activeEnemyData.nextSceneName);
            return;
        }
        // Prioridad 2: la escena de donde vino el jugador (fallback)
        string previousScene = PlayerPrefs.GetString("PreviousScene", "ClassroomScene");
        if (string.IsNullOrWhiteSpace(previousScene) || previousScene == "BattleScene")
        {
            previousScene = "ClassroomScene";
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene(previousScene);
    }
    /// <summary>
    /// Stops any active resize and starts a new one.
    /// </summary>
    void SafeResize(Vector2 targetSize, Action onFinish)
    {
        if (resizeCoroutine != null) StopCoroutine(resizeCoroutine);
        isResizing = true;
        resizeCoroutine = StartCoroutine(ResizeBattleBox(targetSize, onFinish));
    }
    /// <summary>
    /// The coroutine behind the resizing system.
    /// </summary>
    IEnumerator ResizeBattleBox(Vector2 targetSize, Action onFinish)
    {
        Vector2 startSize = battleBox.size;
        float xSign = Mathf.Sign(targetSize.x - startSize.x);
        float ySign = Mathf.Sign(targetSize.y - startSize.y);
        Vector2 size = startSize;
        while (size.x != targetSize.x || size.y != targetSize.y)
        {
            size.x += xSign * SIZE_INCREASE * Time.deltaTime;
            size.y += ySign * SIZE_INCREASE * Time.deltaTime;
            if ((xSign == 1 && size.x > targetSize.x) || (xSign == -1 && size.x < targetSize.x))
            {
                size.x = targetSize.x;
            }
            if ((ySign == 1 && size.y > targetSize.y) || (ySign == -1 && size.y < targetSize.y))
            {
                size.y = targetSize.y;
            }
               
            battleBox.size = size;
            yield return null;
        }
        isResizing = false;
        onFinish?.Invoke();
    }
}