using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Manages the HABLAR dialogue-choice submenu.
/// Shows dialogue options as text INSIDE the battle box using the existing
/// actingText TMP reference. No extra scene GameObjects needed.
/// (POO: Responsabilidad Única — solo maneja el submenú de HABLAR)
/// </summary>
public class HablarManager : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────────────────────
    public static HablarManager instance
    {
        get
        {
            if (_instance == null)
            {
                BattleManager bm = BattleManager.battleInstance;
                if (bm != null)
                    _instance = bm.gameObject.AddComponent<HablarManager>();
                else
                {
                    GameObject go = new GameObject("HablarManager_Runtime");
                    _instance = go.AddComponent<HablarManager>();
                }
            }
            return _instance;
        }
    }
    private static HablarManager _instance;

    // ── Opciones del submenú ──────────────────────────────────────────────
    private static readonly string[] optionLabels = new[]
    {
        "Hablar con calma",
        "Preguntar que le pasa",
        "Defender a la victima",
        "Pedir la mochila",
        "Avisar a un adulto"
    };

    private static readonly string[][] dialogueLines = new[]
    {
        new[] { "* Le dices que sus palabras duelen y que puede parar ahora." },
        new[] { "* Le preguntas si esta enojado por algo y le ofreces hablar." },
        new[] { "* Te pones firme: nadie merece ser humillado." },
        new[] { "* Le pides que devuelva la mochila y termine esto sin mas dano." },
        new[] { "* Le dices que si sigue, buscaras ayuda de un adulto." }
    };

    private readonly int[] mercyValues = { 22, 28, 12, 35, 8 };
    private readonly bool[] startsBattle = { false, false, true, false, true };
    private readonly string[] mateoResponses =
    {
        "* Mateo baja la voz por primera vez.",
        "* Mateo mira al suelo. Parece que eso le llego.",
        "* Mateo se molesta y vuelve a atacarte.",
        "* Mateo aprieta la mochila, pero empieza a dudar.",
        "* Mateo se pone a la defensiva y lanza otra provocacion."
    };
    private int[] rotationIdx;

    // ── Layout (ajustable desde el Inspector) ────────────────────────────
    [Header("Layout del submenú")]
    [Tooltip("Posición mundial absoluta del texto de opciones")]
    public Vector3 menuPosition      = new Vector3(-5.15f, -1.74f, 0f);
    [Tooltip("Ancho y alto del área de texto")]
    public Vector2 menuSize          = new Vector2(11f, 3.8f);
    [Tooltip("Tamaño de fuente de las opciones")]
    public float   menuFontSize      = 3f;
    [Tooltip("Espaciado entre líneas")]
    public float   menuLineSpacing   = -10f;

    // ── Estado ────────────────────────────────────────────────────────────
    private bool        isOpen;
    private int         selectionInt;
    private float       inputDelay;
    private TextMeshPro displayText;
    private float       originalFontSize;
    private TextAlignmentOptions originalAlignment;
    private bool        originalWordWrap;
    private bool        originalAutoSizing;
    private float       originalLineSpacing;
    private string      cachedFlavorText;

    // ── Unity ─────────────────────────────────────────────────────────────
    private int GetOptionsCount()
    {
        BattleManager bm = BattleManager.battleInstance;
        if (bm != null && bm.activeEnemyData != null && bm.activeEnemyData.hablarOpciones != null && bm.activeEnemyData.hablarOpciones.Length > 0)
        {
            return bm.activeEnemyData.hablarOpciones.Length;
        }
        return optionLabels.Length;
    }

    private string GetOptionLabel(int idx)
    {
        BattleManager bm = BattleManager.battleInstance;
        if (bm != null && bm.activeEnemyData != null && bm.activeEnemyData.hablarOpciones != null && bm.activeEnemyData.hablarOpciones.Length > 0)
        {
            if (idx >= 0 && idx < bm.activeEnemyData.hablarOpciones.Length)
                return bm.activeEnemyData.hablarOpciones[idx].label;
        }
        return (idx >= 0 && idx < optionLabels.Length) ? optionLabels[idx] : "";
    }

    void Awake()
    {
        if (_instance == null) _instance = this;
        else if (_instance != this) { Destroy(gameObject); return; }
        rotationIdx = new int[20];
    }

    private bool axisInUse = false;

    void Update()
    {
        if (!isOpen) return;

        // Joystick/Dpad Axis Navigation
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        bool leftPressed = Input.GetKeyDown(KeyCode.LeftArrow);
        bool rightPressed = Input.GetKeyDown(KeyCode.RightArrow);
        bool upPressed = Input.GetKeyDown(KeyCode.UpArrow);
        bool downPressed = Input.GetKeyDown(KeyCode.DownArrow);

        if (horizontalInput == 0f && verticalInput == 0f)
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
            else if (verticalInput < -0.5f)
            {
                downPressed = true;
                axisInUse = true;
            }
            else if (verticalInput > 0.5f)
            {
                upPressed = true;
                axisInUse = true;
            }
        }

        int count = GetOptionsCount();

        // Navegación
        if (upPressed)
            selectionInt = Mathf.Max(0, selectionInt - 1);

        if (downPressed)
            selectionInt = Mathf.Min(count - 1, selectionInt + 1);

        if (rightPressed)
            selectionInt = selectionInt switch { 0 => (count > 3 ? 3 : selectionInt), 1 => (count > 4 ? 4 : selectionInt), _ => selectionInt };

        if (leftPressed)
            selectionInt = selectionInt switch { 3 => 0, 4 => 1, _ => selectionInt };

        RefreshDisplay();

        // Retroceder / Cancelar
        if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.JoystickButton1))
        {
            CloseSubmenu();
            return;
        }

        // Confirmar
        inputDelay += Time.deltaTime;
        if (inputDelay > 0.25f && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.JoystickButton0)))
        {
            int confirmedIdx = selectionInt;
            CloseSubmenu();
            OnConfirm(confirmedIdx);
        }
    }

    // ── API pública ───────────────────────────────────────────────────────

    /// <summary>
    /// Abre el submenú HABLAR mostrando las opciones dentro del recuadro.
    /// </summary>
    public void AbrirMenuHablar()
    {
        BattleManager bm = BattleManager.battleInstance;
        if (bm == null) return;

        bm.SetMainButtonsVisible(false); // Ocultar botones principales

        displayText = bm.actingMgr.actingText;
        if (displayText == null)
        {
            Debug.LogWarning("[HablarManager] actingText es null. Verifica la referencia en ActingManager.");
            return;
        }

        // Guardar configuración original del TMP
        originalFontSize  = displayText.fontSize;
        originalAlignment = displayText.alignment;
        originalWordWrap  = displayText.enableWordWrapping;
        originalAutoSizing = displayText.enableAutoSizing;
        originalLineSpacing = displayText.lineSpacing;

        // Aplicar formato del submenú
        displayText.transform.position = menuPosition;
        displayText.rectTransform.sizeDelta = menuSize;

        displayText.alignment          = TextAlignmentOptions.Left;
        displayText.enableWordWrapping = false;
        displayText.fontSize           = menuFontSize;
        displayText.lineSpacing        = menuLineSpacing;
        displayText.enableAutoSizing   = false;
        displayText.gameObject.SetActive(true);

        // Inicializar estado — selección en 0 ANTES de abrir
        selectionInt  = 0;
        inputDelay    = 0f;
        bm.isHablando = true;

        // Cachear el texto de sabor UNA sola vez al abrir el menú
        cachedFlavorText = "* Intuitivamente, sientes la tension en el aire.";
        if (bm.activeEnemyData != null)
        {
            var flavorTexts = bm.activeEnemyData.flavorTexts;
            if (flavorTexts != null && flavorTexts.Length > 0)
                cachedFlavorText = flavorTexts[UnityEngine.Random.Range(0, flavorTexts.Length)];
        }

        // Dibujar con cursor en posición correcta ANTES de activar input
        isOpen = false;
        RefreshDisplay();
        isOpen = true;

        Debug.Log("[HablarManager] Submenú HABLAR abierto.");
    }

    // ── Privados ──────────────────────────────────────────────────────────

    /// <summary>Cierra el submenú y restaura la configuración del TMP.</summary>
    private void CloseSubmenu()
    {
        isOpen = false;

        BattleManager bm = BattleManager.battleInstance;
        if (bm != null)
        {
            bm.isHablando = false;
            bm.SetMainButtonsVisible(true); // Volver a mostrar botones principales
        }

        if (displayText != null)
        {
            displayText.fontSize          = originalFontSize;
            displayText.alignment         = originalAlignment;
            displayText.enableWordWrapping = originalWordWrap;
            displayText.enableAutoSizing  = originalAutoSizing;
            displayText.lineSpacing       = originalLineSpacing;
            displayText.text              = "";
        }
    }

    /// <summary>Dibuja las opciones con el cursor ♥ en la opción seleccionada.</summary>
    private void RefreshDisplay()
    {
        if (displayText == null) return;

        int count = GetOptionsCount();
        string result = "<color=#8d8d8d>HABLAR</color>\n";

        // Dibujar en 2 columnas
        result += OptionLine(0, count > 3 ? 3 : -1);
        result += OptionLine(1, count > 4 ? 4 : -1);
        if (count > 2)
        {
            result += OptionLine(2, -1);
        }

        result += $"\n<size=62%><pos=36%><color=#ffffff>{cachedFlavorText}</color></size>";
        displayText.text = result;
    }

    private string OptionLine(int leftIdx, int rightIdx)
    {
        string left = FormatOption(leftIdx);
        if (rightIdx < 0) return left + "\n";
        return left + "<pos=70%>" + FormatOption(rightIdx) + "\n";
    }

    private string FormatOption(int idx)
    {
        string labelText = GetOptionLabel(idx);
        string prefix = idx == selectionInt ? "> " : "  ";
        string color = idx == selectionInt ? "#f5e642" : "#d7d7d7";
        return $"<color={color}>{prefix}{labelText}</color>";
    }

    /// <summary>Se ejecuta cuando el jugador confirma una opción.</summary>
    private void OnConfirm(int idx)
    {
        BattleManager bm = BattleManager.battleInstance;
        if (bm == null) return;

        int count = GetOptionsCount();
        if (idx < 0 || idx >= count) return;

        string playerLine = "";
        int mercyVal = 0;
        bool triggersCombat = false;
        string response = "";

        if (bm.activeEnemyData != null && bm.activeEnemyData.hablarOpciones != null && bm.activeEnemyData.hablarOpciones.Length > 0)
        {
            var opt = bm.activeEnemyData.hablarOpciones[idx];
            playerLine = opt.playerLine;
            mercyVal = opt.mercyValue;
            triggersCombat = opt.startsBattle;
            response = opt.enemyResponse;
        }
        else
        {
            // Fallbacks de Mateo
            int rot = rotationIdx[idx];
            playerLine = dialogueLines[idx][rot % dialogueLines[idx].Length];
            rotationIdx[idx] = (rot + 1) % dialogueLines[idx].Length;
            mercyVal = mercyValues[idx];
            triggersCombat = startsBattle[idx];
            response = mateoResponses[idx];
        }

        // Mercy
        bm.actingMgr.totalMercy = Mathf.Min(bm.actingMgr.totalMercy + mercyVal, bm.actingMgr.totalMercyMax);
        Debug.Log($"[HablarManager] Opción {idx} — Mercy +{mercyVal} → total {bm.actingMgr.totalMercy}");

        // shouldTalk = false siempre — nunca activar EnemyTalking ni el panel flotante
        DialogueManager.instance.shouldTalk = false;
        if (bm.actingMgr.totalMercy >= bm.actingMgr.totalMercyMax)
        {
            string endMessage = bm.activeEnemyData != null ? bm.activeEnemyData.spareMessage : "* Mateo entiende el dano que hizo y decide cambiar.";
            bm.StartCoroutine(bm.CalmSequence(
                playerLine,
                endMessage,
                0));
        }
        else if (triggersCombat)
        {
            DialogueManager.instance.dialogueTxt = playerLine;
            Action afterPlayerTalk = () =>
            {
                DialogueManager.instance.shouldTalk = false;
                DialogueManager.instance.dialogueTxt = response;
                DialogueManager.instance.Talking(() => bm.StartCoroutine(bm.ActingSequence()));
            };
            DialogueManager.instance.Talking(afterPlayerTalk);
        }
        else
        {
            bm.StartCoroutine(bm.CalmSequence(
                playerLine,
                response,
                0));
        }
    }
}

