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
    public float damage;
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
        actingMgr.actingText.gameObject.SetActive(false);
        audioMgr.Selecting();
    }
    /// <summary>
    /// The item method, does nothing so far
    /// </summary>
    public void Item()
    {
        ItemManager.instance.isMenu = true;
        ItemManager.instance.canAct = true;
        isFighting = false;
        ItemManager.instance.itemObjects.SetActive(true);
        ItemManager.instance.useText.gameObject.SetActive(false);
        audioMgr.Selecting();
    }
    /// <summary>
    /// The mercy method, as of right now it ends the battle but really it doesn't do anything special
    /// </summary>
    public void Mercy()
    {
        if (actingMgr.totalMercy >= actingMgr.totalMercyMax)
        {
            DialogueManager.instance.enemyTxt = "*Blushes Deeply*";
            DialogueManager.instance.Talking(null);
            audioMgr.Selecting();
        }
    }

    void Start()
    {
        selectionInt = 0;
        maxSelectionInt = 3;
        minSelectionInt = 0;
        playerVariables = FindAnyObjectByType<PlayerVars>();

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

    void Update()
    {
        //when the player is not fighting nor acting nor in HABLAR submenu, these if statements get called
        if (!isFighting && !actingMgr.isActing && !ItemManager.instance.isMenu && !isHablando)
        {
            if (selectionInt > maxSelectionInt)
            {
                selectionInt = 0;
            }
            if (selectionInt < minSelectionInt)
            {
                selectionInt = 3;
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                selectionInt--;
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                selectionInt++;
            }
            Selection();

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.JoystickButton0))
            {
                Selected();
            }
        }
        // Bug fix: Only disable PlayerMovement during menu phase, NOT the whole GO.
        // The soul SpriteRenderer lives on the same GO as PlayerVars — calling
        // SetActive(false) was also hiding the selection cursor during menu navigation.
        if (playerVariables != null)
        {
            var pm = playerVariables.GetComponent<PlayerMovement>();
            if (pm != null) pm.enabled = isFighting;
        }

        // Bug fix: Null-guard to prevent NullReferenceException every frame when
        // any Inspector reference is missing.
        if (playerVariables != null && healthTxt != null && healthMeter != null)
        {
            float xScale = Mathf.Clamp01(playerVariables.health / 20f);
            healthTxt.text = playerVariables.health + "   /   20";
            healthMeter.transform.localScale = new Vector3(xScale,healthMeter.transform.localScale.y, healthMeter.transform.localScale.z);
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
            soul.transform.position = buttons[selectedInt].soulPosition.position;
            
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
            SafeResize(new Vector2(11.5f, 3), onBoxFinish);
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

        playerVariables.transform.position = new Vector2(0, -1.7f);
        SafeResize(new Vector2(3, 3), null);
        actingMgr.actingText.gameObject.SetActive(false);
        playerVariables.GetComponent<SpriteRenderer>().enabled = true;
        if (attackMgr != null && attackMgr.attacksScriptable != null)
        {
            attackMgr.StartAttack(attackMgr.attacksScriptable.GetAttack(), isFinished);
        }
        else
        {
            Debug.LogWarning("Falta AttackManager en la escena. Saltando turno del enemigo.");
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
            soul.enabled = true;
            soul.transform.position = buttons[1].soulPosition.position;
            SafeResize(new Vector2(11.5f, 3f), boxAction);
            actingMgr.isActing = false;
            isFighting = false;
            actingMgr.actingText.gameObject.SetActive(true);
            DialogueManager.instance.Talking(null);
            actingMgr.actObjects.SetActive(false);
            actingMgr.canAct = true;

        };
        yield return new WaitForSeconds(1);
        playerVariables.transform.position = new Vector2(0, -1.7f);
        SafeResize(new Vector2(3, 3), boxAction);
        actingMgr.actingText.gameObject.SetActive(false);
        actingMgr.isActing = false;
        isFighting = true;
        actingMgr.time = 0;
        if (attackMgr != null && attackMgr.attacksScriptable != null)
        {
            attackMgr.StartAttack(attackMgr.attacksScriptable.GetAttack(), isFinished);
        }
        else
        {
            Debug.LogWarning("Falta AttackManager en la escena. Saltando turno del enemigo.");
            isFinished?.Invoke();
        }
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
            soul.enabled = true;
            soul.transform.position = buttons[2].soulPosition.position;
            SafeResize(new Vector2(11.5f, 3f), null);
            actingMgr.isActing = false;
            isFighting = false;
            actingMgr.actingText.gameObject.SetActive(true);
            DialogueManager.instance.Talking(null);
            actingMgr.actObjects.SetActive(false);
            ItemManager.instance.itemObjects.SetActive(false);
            actingMgr.canAct = true;

        };
        yield return new WaitForSeconds(1);
        playerVariables.transform.position = new Vector2(0, -1.7f);
        SafeResize(new Vector2(3, 3), null);
        ItemManager.instance.itemObjects.SetActive(false);
        isFighting = true;
        ItemManager.instance.isMenu = false;
        ItemManager.instance.useText.text = "";
        actingMgr.time = 0;
        if (attackMgr != null && attackMgr.attacksScriptable != null)
        {
            attackMgr.StartAttack(attackMgr.attacksScriptable.GetAttack(), isFinished);
        }
        else
        {
            Debug.LogWarning("Falta AttackManager en la escena. Saltando turno del enemigo.");
            isFinished?.Invoke();
        }
    }
    /// <summary>
    /// Stops any active resize and starts a new one.
    /// </summary>
    void SafeResize(Vector2 targetSize, Action onFinish)
    {
        if (resizeCoroutine != null) StopCoroutine(resizeCoroutine);
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
        onFinish?.Invoke();
        
    }
}
