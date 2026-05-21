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

    // ── Unity ─────────────────────────────────────────────────────────────
    void Awake()
    {
        if (_instance == null) _instance = this;
        else if (_instance != this) { Destroy(gameObject); return; }
        rotationIdx = new int[dialogueLines.Length];
    }

    void Update()
    {
        if (!isOpen) return;

        // Navegación
        if (Input.GetKeyDown(KeyCode.UpArrow))
            selectionInt = Mathf.Max(0, selectionInt - 1);

        if (Input.GetKeyDown(KeyCode.DownArrow))
            selectionInt = Mathf.Min(optionLabels.Length - 1, selectionInt + 1);

        if (Input.GetKeyDown(KeyCode.RightArrow))
            selectionInt = selectionInt switch { 0 => 3, 1 => 4, _ => selectionInt };

        if (Input.GetKeyDown(KeyCode.LeftArrow))
            selectionInt = selectionInt switch { 3 => 0, 4 => 1, _ => selectionInt };

        RefreshDisplay();

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
    /// Abre el submenú HABLAR mostrando las 3 opciones dentro del recuadro.
    /// </summary>
    public void AbrirMenuHablar()
    {
        BattleManager bm = BattleManager.battleInstance;
        if (bm == null) return;

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
        if (bm.battleBox != null)
        {
            displayText.transform.position = bm.battleBox.transform.position + new Vector3(-3.55f, 0.78f, 0f);
            displayText.rectTransform.sizeDelta = new Vector2(7.55f, 1.95f);
        }

        displayText.alignment          = TextAlignmentOptions.Left;
        displayText.enableWordWrapping = false;
        displayText.fontSize           = 1.48f;
        displayText.lineSpacing        = -10f;
        displayText.enableAutoSizing   = false;
        displayText.gameObject.SetActive(true);

        // Inicializar estado — selección en 0 ANTES de abrir
        selectionInt  = 0;
        inputDelay    = 0f;
        bm.isHablando = true;

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
        if (bm != null) bm.isHablando = false;

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

    /// <summary>Dibuja las 3 opciones con el cursor ♥ en la opción seleccionada.</summary>
    private void RefreshDisplay()
    {
        if (displayText == null) return;

        string result =
            "<color=#8d8d8d>HABLAR</color>\n" +
            OptionLine(0, 3) +
            OptionLine(1, 4) +
            OptionLine(2, -1) +
            "\n<size=62%><pos=36%><color=#ffffff>* Intuitivamente, sientes la tension en el aire.</color></size>";
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
        string prefix = idx == selectionInt ? "> " : "  ";
        string color = idx == selectionInt ? "#f5e642" : "#d7d7d7";
        return $"<color={color}>{prefix}{optionLabels[idx]}</color>";
    }

    /// <summary>Se ejecuta cuando el jugador confirma una opción.</summary>
    private void OnConfirm(int idx)
    {
        BattleManager bm = BattleManager.battleInstance;
        if (bm == null) return;

        if (idx < 0 || idx >= optionLabels.Length) return;

        // Línea del jugador (rotación)
        int rot = rotationIdx[idx];
        string playerLine = dialogueLines[idx][rot % dialogueLines[idx].Length];
        rotationIdx[idx] = (rot + 1) % dialogueLines[idx].Length;

        // Mercy
        bm.actingMgr.totalMercy = Mathf.Min(bm.actingMgr.totalMercy + mercyValues[idx], bm.actingMgr.totalMercyMax);
        Debug.Log($"[HablarManager] Opción {idx} — Mercy +{mercyValues[idx]} → total {bm.actingMgr.totalMercy}");

        // shouldTalk = false siempre — nunca activar EnemyTalking ni el panel flotante
        DialogueManager.instance.shouldTalk = false;
        if (bm.actingMgr.totalMercy >= bm.actingMgr.totalMercyMax)
        {
            bm.StartCoroutine(bm.CalmSequence(
                playerLine,
                "* Mateo entiende el dano que hizo y decide cambiar.",
                0));
        }
        else if (startsBattle[idx])
        {
            DialogueManager.instance.dialogueTxt = playerLine;
            Action afterPlayerTalk = () =>
            {
                DialogueManager.instance.shouldTalk = false;
                DialogueManager.instance.dialogueTxt = mateoResponses[idx];
                DialogueManager.instance.Talking(() => bm.StartCoroutine(bm.ActingSequence()));
            };
            DialogueManager.instance.Talking(afterPlayerTalk);
        }
        else
        {
            bm.StartCoroutine(bm.CalmSequence(
                playerLine,
                mateoResponses[idx],
                0));
        }
    }
}
