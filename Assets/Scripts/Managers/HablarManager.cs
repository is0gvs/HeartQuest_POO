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
        "Preguntar por qué",
        "Pedir que pare"
    };

    private static readonly string[][] dialogueLines = new[]
    {
        new[] { "Le dices que sus palabras duelen.", "Intentas entenderlo." },
        new[] { "Le pides que se detenga.",          "Le hablas con calma." },
        new[] { "Compartes cómo te sientes.",        "Mantienes la calma."  }
    };

    private readonly int[] mercyValues = { 10, 8, 12 };
    private int[] rotationIdx;

    // ── Estado ────────────────────────────────────────────────────────────
    private bool        isOpen;
    private int         selectionInt;
    private float       inputDelay;
    private TextMeshPro displayText;
    private float       originalFontSize;
    private TextAlignmentOptions originalAlignment;
    private bool        originalWordWrap;

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
        if (Input.GetKeyDown(KeyCode.UpArrow)   || Input.GetKeyDown(KeyCode.LeftArrow))
            selectionInt = Mathf.Max(0, selectionInt - 1);

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.RightArrow))
            selectionInt = Mathf.Min(optionLabels.Length - 1, selectionInt + 1);

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

        // Aplicar formato del submenú
        if (bm.battleBox != null)
        {
            displayText.transform.position = bm.battleBox.transform.position + new Vector3(-3.65f, 0.5f, 0f);
            displayText.rectTransform.sizeDelta = new Vector2(7.2f, 2.2f);
        }

        displayText.alignment          = TextAlignmentOptions.Left;
        displayText.enableWordWrapping = false;
        displayText.fontSize           = 2.0f;
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
            displayText.text              = "";
        }
    }

    /// <summary>Dibuja las 3 opciones con el cursor ♥ en la opción seleccionada.</summary>
    private void RefreshDisplay()
    {
        if (displayText == null) return;

        string result = "";
        for (int i = 0; i < optionLabels.Length; i++)
        {
            if (i == selectionInt)
                result += "<color=yellow>> </color>" + optionLabels[i] + "\n";
            else
                result += "  " + optionLabels[i] + "\n";
        }
        displayText.text = result;
    }

    /// <summary>Se ejecuta cuando el jugador confirma una opción.</summary>
    private void OnConfirm(int idx)
    {
        BattleManager bm = BattleManager.battleInstance;
        if (bm == null) return;

        // Línea del jugador (rotación)
        int rot = rotationIdx[idx];
        string playerLine = dialogueLines[idx][rot % dialogueLines[idx].Length];
        rotationIdx[idx] = (rot + 1) % dialogueLines[idx].Length;

        // Mercy
        bm.actingMgr.totalMercy += mercyValues[idx];
        Debug.Log($"[HablarManager] Opción {idx} — Mercy +{mercyValues[idx]} → total {bm.actingMgr.totalMercy}");

        // shouldTalk = false siempre — nunca activar EnemyTalking ni el panel flotante
        DialogueManager.instance.shouldTalk = false;
        if (idx == 2)
        {
            DialogueManager.instance.dialogueTxt = playerLine;
            Action afterPlayerTalk = () =>
            {
                DialogueManager.instance.shouldTalk = false;
                DialogueManager.instance.dialogueTxt = "No me digas que hacer.";
                DialogueManager.instance.Talking(() => bm.StartCoroutine(bm.ActingSequence()));
            };
            DialogueManager.instance.Talking(afterPlayerTalk);
        }
        else
        {
            bm.StartCoroutine(bm.CalmSequence(
                playerLine,
                idx == 0 ? "* Mateo duda un momento al escucharte." : "* Mateo no sabe bien que responder.",
                0));
        }
    }
}
