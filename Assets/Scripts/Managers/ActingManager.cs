using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ActingManager : MonoBehaviour
{
    int maxSelectionInt;
    int minSelectionInt;
    public int selectionInt;
    public string spareMessage;
    public List<ActingButtons> buttons;
    public SpriteRenderer soul;
    public TextMeshPro actingText;
    public bool isActing;
    public GameObject actObjects;
    public int totalMercy;
    public int totalMercyMax;
    public List<string> flavorText;
    public float time;
    public bool canAct = true;
    void Start()
    {
        maxSelectionInt = 3;
        minSelectionInt = 0;
        // Fix: default TMP pivot (0.5,0.5) with position x=-5.2 pushed the left edge
        // off-screen (~x=-10.2). Left-anchor pivot keeps text inside the battle box.
        if (actingText != null)
        {
            actingText.rectTransform.pivot = new Vector2(0f, 0.5f);
            actingText.fontSize = 1.55f;
            actingText.enableAutoSizing = false;
        }
    }
    private bool axisInUse = false;

    void Update()
    {
        if (!BattleManager.battleInstance.isFighting && isActing)
        {
            // Volver a los botones principales (igual que MOCHILA).
            if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.JoystickButton1))
            {
                if (canAct)
                {
                    BattleManager.battleInstance.CloseActingMenu();
                    return;
                }
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

            if (leftPressed)
            {
                selectionInt--;
            }
            if (rightPressed)
            {
                selectionInt++;
            }
            if (upPressed)
            {
                selectionInt -= 2;
            }
            if (downPressed)
            {
                selectionInt += 2;
            }
            Selection();
            time += Time.deltaTime;
            if (time > 0.25f)
            {
                if(canAct)
                {
                    if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.JoystickButton0))
                    {
                        canAct = false;
                        Selected();
                    }
                }
            }
        }
    }

    void Selecting(int selectedInt)
    {
        if (buttons[selectedInt].selected)
        {
            if (soul != null) soul.enabled = false;
            SetButtonVisual(selectedInt, true);
        }
    }
    void Deselecting(int deselectionInt)
    {
        buttons[deselectionInt].selected = false;
        SetButtonVisual(deselectionInt, false);
    }

    /// <summary>
    /// Resalta la opción seleccionada (color/escala/negrita), espejo del de ItemManager,
    /// para que APOYAR tenga el mismo seleccionador visual que el menú principal.
    /// </summary>
    private void SetButtonVisual(int index, bool selected)
    {
        if (index < 0 || index >= buttons.Count || buttons[index] == null) return;

        Transform option = buttons[index].transform;
        // Resaltado por contorno/halo: NO se altera el color base del botón.
        option.localScale = selected ? Vector3.one * 1.1f : Vector3.one;

        BattleSelectionHalo.Apply(option, selected);

        TextMeshPro label = option.GetComponentInChildren<TextMeshPro>();
        if (label != null)
        {
            label.fontStyle = selected ? FontStyles.Bold : FontStyles.Normal;
        }
    }
    void Selection()
    {

        if (selectionInt == 0)
        {
            buttons[selectionInt].selected = true;
            Selecting(0);
        }
        else
        {
            Deselecting(0);
        }
        if (selectionInt == 1)
        {
            buttons[selectionInt].selected = true;
            Selecting(1);
        }
        else
        {
            Deselecting(1);
        }
        if (selectionInt == 2)
        {
            buttons[selectionInt].selected = true;
            Selecting(2);
        }
        else
        {
            Deselecting(2);
        }
        if (selectionInt == 3)
        {
            buttons[selectionInt].selected = true;
            Selecting(3);
        }
        else
        {
            Deselecting(3);
        }
    }

    void Selected()
    {
        if (selectionInt == 0)
        {
            OnActing(0);
            totalMercy += buttons[0].actVars.curMercy;
        }
        if (selectionInt == 1)
        {
            OnActing(1);
            totalMercy += buttons[1].actVars.curMercy;
            totalMercyMax += buttons[1].actVars.mercyMax;
        }
        if (selectionInt == 2)
        {
            OnActing(2);
            totalMercy += buttons[2].actVars.curMercy;
            totalMercyMax += buttons[2].actVars.mercyMax;
        }
        if (selectionInt == 3)
        {
            OnActing(3);
            totalMercy += buttons[3].actVars.curMercy;
            totalMercyMax += buttons[3].actVars.mercyMax;
        }
    }
    public void OnActing(int selectedInt)
    {
        canAct = false;
        buttons[selectedInt].actVars.curMercy += buttons[selectedInt].actVars.mercyValue[0];
        actingText.gameObject.SetActive(true);

        string playerLine = buttons[selectedInt].actVars.actTxt[0];
        if (selectedInt == 1)
        {
            DialogueManager.instance.dialogueTxt = playerLine;
            Action doneTalking = () =>
            {
                Debug.Log("action initiated");
                DialogueManager.instance.shouldTalk = false;
                StartCoroutine(BattleManager.battleInstance.ActingSequence());
            };
            DialogueManager.instance.enemyTxt = BattleManager.battleInstance.enemyDialogue[UnityEngine.Random.Range(0, BattleManager.battleInstance.enemyDialogue.Count)];
            DialogueManager.instance.shouldTalk = true;
            DialogueManager.instance.Talking(doneTalking);
        }
        else
        {
            DialogueManager.instance.shouldTalk = false;
            string eName = BattleManager.battleInstance.activeEnemyData != null ? BattleManager.battleInstance.activeEnemyData.enemyName : "El enemigo";
            StartCoroutine(BattleManager.battleInstance.CalmSequence(
                playerLine,
                $"* {eName} baja un poco el tono.",
                0));
        }

        actObjects.SetActive(false);
        BattleManager.battleInstance.SetMainButtonsVisible(true);
        if (buttons[selectedInt].actVars.actTxt.Count <= 2 || buttons[selectedInt].actVars.mercyValue.Count <= 2)
        {
            Debug.Log("We added");
            buttons[selectedInt].actVars.actTxt.Add(buttons[selectedInt].actVars.actTxt[0]);
            buttons[selectedInt].actVars.mercyValue.Add(buttons[selectedInt].actVars.mercyValue[0]);
        }
        else
        {
            buttons[selectedInt].actVars.actTxt.RemoveAt(0);
            buttons[selectedInt].actVars.mercyValue.RemoveAt(0);
        }
    }


}
