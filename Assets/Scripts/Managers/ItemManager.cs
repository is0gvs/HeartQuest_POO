using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public List<ItemButtons> buttons;
    [HideInInspector]
    public static ItemManager instance;
    void Awake() => instance = this;
    int maxSelectionInt;
    int minSelectionInt;
    public int selectionInt;
    public SpriteRenderer soul;
    public TextMeshPro useText;
    public GameObject itemObjects;
    public float time;
    public bool isMenu;
    public bool canAct = true;
    void Start()
    {
        maxSelectionInt = 3;
        minSelectionInt = 0;
        // Fix: same off-screen pivot issue as ActingText
        if (useText != null)
        {
            useText.rectTransform.pivot = new Vector2(0f, 0.5f);
            useText.rectTransform.sizeDelta = new Vector2(8.1f, 1.7f);
            useText.fontSize = 1.55f;
            useText.enableAutoSizing = false;
            useText.enableWordWrapping = true;
        }
    }

    private bool axisInUse = false;

    void Update()
    {
        if (!BattleManager.battleInstance.isFighting && isMenu)
        {
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
                if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.JoystickButton1))
                {
                    BattleManager.battleInstance.CloseItemMenu();
                    return;
                }
                if (canAct && DialogueManager.instance.done)
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
        Action dialogue = () =>
        {
            DialogueManager.instance.shouldTalk = false;
            StartCoroutine(BattleManager.battleInstance.ItemSequence());
        };
        if (selectionInt == 0)
        {
            if (PlayerVars.instance.health < 20)
            {
                PlayerVars.instance.health += buttons[0].itemHeal;
            }
            soul.enabled = false;   
            DialogueManager.instance.dialogueTxt = "* Usaste " + buttons[0].itemName + ". Recuperaste " + buttons[0].itemHeal + " HP.";
            DialogueManager.instance.text.gameObject.SetActive(true);
            DialogueManager.instance.enemyTxt = BattleManager.battleInstance.enemyDialogue[UnityEngine.Random.Range(0, BattleManager.battleInstance.enemyDialogue.Count)];
            DialogueManager.instance.shouldTalk = true;
            DialogueManager.instance.Talking(dialogue);
            itemObjects.SetActive(false);
        }
        if (selectionInt == 1)
        {
            if (PlayerVars.instance.health < 20)
            {
                PlayerVars.instance.health += buttons[1].itemHeal;
            }
            if (PlayerVars.instance.health > 20)
            {
                PlayerVars.instance.health = 20;
            }
            soul.enabled = false;
            DialogueManager.instance.dialogueTxt = "* Usaste " + buttons[1].itemName + ". Recuperaste " + buttons[1].itemHeal + " HP.";
            DialogueManager.instance.text.gameObject.SetActive(true);
            DialogueManager.instance.enemyTxt = BattleManager.battleInstance.enemyDialogue[UnityEngine.Random.Range(0, BattleManager.battleInstance.enemyDialogue.Count)];
            DialogueManager.instance.shouldTalk = true;
            DialogueManager.instance.Talking(dialogue);
            itemObjects.SetActive(false);
        }
        if (selectionInt == 2)
        {
            if (PlayerVars.instance.health < 20)
            {
                PlayerVars.instance.health += buttons[2].itemHeal;
            }
            if (PlayerVars.instance.health > 20)
            {
                PlayerVars.instance.health = 20;
            }
            soul.enabled = false;
            DialogueManager.instance.dialogueTxt = "* Usaste " + buttons[2].itemName + ". Recuperaste " + buttons[2].itemHeal + " HP.";
            DialogueManager.instance.text.gameObject.SetActive(true);
            DialogueManager.instance.enemyTxt = BattleManager.battleInstance.enemyDialogue[UnityEngine.Random.Range(0, BattleManager.battleInstance.enemyDialogue.Count)];
            DialogueManager.instance.shouldTalk = true;
            DialogueManager.instance.Talking(dialogue);
            itemObjects.SetActive(false);
        }
        if (selectionInt == 3)
        {
            if (PlayerVars.instance.health < 20)
            {
                PlayerVars.instance.health += buttons[3].itemHeal;
            }
            if (PlayerVars.instance.health > 20)
            {
                PlayerVars.instance.health = 20;
            }
            soul.enabled = false;
            DialogueManager.instance.dialogueTxt = "* Usaste " + buttons[3].itemName + ". Recuperaste " + buttons[3].itemHeal + " HP.";
            DialogueManager.instance.text.gameObject.SetActive(true);
            DialogueManager.instance.enemyTxt = BattleManager.battleInstance.enemyDialogue[UnityEngine.Random.Range(0, BattleManager.battleInstance.enemyDialogue.Count)];
            DialogueManager.instance.shouldTalk = true;
            DialogueManager.instance.Talking(dialogue);
            itemObjects.SetActive(false);


        }

    }
}
