using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Manages the HABLAR dialogue-choice submenu.
/// Shows dialogue options as text INSIDE the battle box using the existing
/// actingText TMP reference. No extra scene GameObjects needed.
/// </summary>
public class HablarManager : MonoBehaviour
{
    /// <summary>Singleton instance. Auto-creates on BattleManager's GameObject if missing.</summary>
    public static HablarManager instance
    {
        get
        {
            if (_instance == null)
            {
                // Attach to BattleManager's GameObject so it lives in the scene
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

    // ── Built-in dialogue options ─────────────────────────────────────────
    private static readonly string[] optionLabels = new[]
    {
        "Hablar con calma",
        "Preguntar por qué",
        "Pedir que pare"
    };

    private static readonly string[][] dialogueLines = new[]
    {
        new[] { "Le dices que sus palabras duelen.",  "Intentas entenderlo." },
        new[] { "Le pides que se detenga.",           "Le hablas con calma." },
        new[] { "Compartes cómo te sientes.",         "Mantienes la calma."  }
    };

    private readonly int[] mercyValues = { 10, 8, 12 };
    private int[] rotationIdx;

    // ── State ─────────────────────────────────────────────────────────────
    private bool   isOpen;
    private int    selectionInt;
    private float  inputDelay;
    private TextMeshPro displayText; // the actingText inside the battle box

    void Awake()
    {
        if (_instance == null) _instance = this;
        else if (_instance != this) { Destroy(gameObject); return; }
        rotationIdx = new int[dialogueLines.Length];
    }

    void Update()
    {
        if (!isOpen) return;

        // ── Navigation ────────────────────────────────────────────────────
        if (Input.GetKeyDown(KeyCode.LeftArrow))  selectionInt = Mathf.Max(0, selectionInt - 1);
        if (Input.GetKeyDown(KeyCode.RightArrow)) selectionInt = Mathf.Min(optionLabels.Length - 1, selectionInt + 1);
        if (Input.GetKeyDown(KeyCode.UpArrow))    selectionInt = Mathf.Max(0, selectionInt - 1);
        if (Input.GetKeyDown(KeyCode.DownArrow))  selectionInt = Mathf.Min(optionLabels.Length - 1, selectionInt + 1);

        RefreshDisplay();

        // ── Confirm ───────────────────────────────────────────────────────
        inputDelay += Time.deltaTime;
        if (inputDelay > 0.25f && Input.GetKeyDown(KeyCode.Return))
        {
            isOpen = false;
            BattleManager.battleInstance.isHablando = false;
            OnConfirm(selectionInt);
        }
    }

    // ── Public API ────────────────────────────────────────────────────────

    /// <summary>
    /// Opens the HABLAR submenu. Displays the 3 options as text inside the
    /// battle box using the existing actingText TMP reference.
    /// </summary>
    public void AbrirMenuHablar()
    {
        BattleManager bm = BattleManager.battleInstance;
        if (bm == null) return;

        // Use the actingText TMP to display options inside the battle box
        displayText = bm.actingMgr.actingText;
        if (displayText == null)
        {
            Debug.LogWarning("HablarManager: actingText es null. No se puede mostrar el submenú.");
            return;
        }

        displayText.gameObject.SetActive(true);
        selectionInt = 0;
        inputDelay   = 0f;
        isOpen       = true;
        bm.isHablando = true;

        RefreshDisplay();
        Debug.Log("HablarManager: submenú HABLAR abierto.");
    }

    // ── Internal ──────────────────────────────────────────────────────────

    /// <summary>Shows the option list with a cursor on the selected one.</summary>
    private void RefreshDisplay()
    {
        if (displayText == null) return;

        string result = "";
        for (int i = 0; i < optionLabels.Length; i++)
        {
            string cursor = (i == selectionInt) ? "<color=yellow>♥ " : "  ";
            string end    = (i == selectionInt) ? "</color>" : "";
            result += cursor + optionLabels[i] + end + "\n";
        }
        displayText.text = result;
    }

    /// <summary>Fires when the player confirms a HABLAR choice.</summary>
    private void OnConfirm(int idx)
    {
        BattleManager bm = BattleManager.battleInstance;
        if (bm == null) return;

        // ── Get player dialogue line (rotating) ──────────────────────────
        int rot = rotationIdx[idx];
        string playerLine = dialogueLines[idx][rot % dialogueLines[idx].Length];
        rotationIdx[idx] = (rot + 1) % dialogueLines[idx].Length;

        int mercyGain = mercyValues[idx];

        // ── Apply mercy ──────────────────────────────────────────────────
        bm.actingMgr.totalMercy += mercyGain;
        Debug.Log($"HablarManager: opción {idx}. Mercy +{mercyGain} → total {bm.actingMgr.totalMercy}");

        // ── Set dialogue text ────────────────────────────────────────────
        DialogueManager.instance.dialogueTxt = playerLine;

        if (bm.enemyDialogue != null && bm.enemyDialogue.Count > 0)
            DialogueManager.instance.enemyTxt =
                bm.enemyDialogue[UnityEngine.Random.Range(0, bm.enemyDialogue.Count)];
        else
            DialogueManager.instance.enemyTxt = "...";

        DialogueManager.instance.shouldTalk = true;

        // ── Chain: dialogue → heart minigame → ActingSequence ────────────
        Action doneTalking = () =>
        {
            DialogueManager.instance.shouldTalk = false;
            HeartMinigame.instance.StartMinigame(() =>
            {
                bm.StartCoroutine(bm.ActingSequence());
            });
        };

        DialogueManager.instance.Talking(doneTalking);
    }
}
