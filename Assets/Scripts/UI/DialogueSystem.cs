using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

namespace HeartQuest.UI
{
    /// <summary>
    /// Sistema de diálogos estilo Undertale con estética cyberpunk.
    /// Soporta texto enriquecido (colores), efecto máquina de escribir,
    /// avance con tecla Z y sistema de elecciones.
    /// </summary>
    public class DialogueSystem : MonoBehaviour
    {
        [Header("── Componentes UI ──")]
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private Image portraitImage;
        [SerializeField] private GameObject dialogueBox;

        [Header("── Configuración ──")]
        [SerializeField] private float charactersPerSecond = 45f;
        [SerializeField] private float pauseOnPunctuation = 0.15f;

        [Header("── Sonido (Opcional) ──")]
        [SerializeField] private AudioSource typingAudioSource;
        [SerializeField] private AudioClip typingSound;

        // Estado interno de la conversación
        private HeartQuest.Core.DialogueData currentStory;
        private int currentLineIndex = 0;

        private Coroutine typingCoroutine;
        private bool isTyping = false;
        private string currentFullText = "";
        
        // Estado de Elecciones
        private List<GameObject> choiceButtons = new List<GameObject>();

        // Estado abierto/cerrado independiente de activeSelf, para que la verificación
        // sea fiable aunque el GameObject del componente nunca se desactive.
        private bool isOpen;

        // ── Singleton con auto-creación ───────────────────────────────────────
        // Garantiza que SIEMPRE exista un DialogueSystem funcional en la escena del
        // mundo. Si no hay uno (o el de la escena quedó inactivo), se crea uno en runtime.
        private static DialogueSystem _instance;
        public static DialogueSystem Instance
        {
            get
            {
                // Solo reutilizamos la instancia cacheada si sigue siendo válida
                // (no destruida y con sus referencias de UI asignadas).
                if (!IsUsable(_instance))
                {
                    _instance = null;

                    // Buscar una en escena que SÍ tenga sus referencias asignadas.
                    foreach (var ds in FindObjectsByType<DialogueSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                    {
                        if (IsUsable(ds)) { _instance = ds; break; }
                    }

                    // Si no hay ninguna usable, crear una en runtime con refs válidas.
                    if (_instance == null)
                        _instance = CreateRuntime();
                }
                return _instance;
            }
        }

        /// <summary>True si la instancia existe (no destruida) y tiene sus refs de UI.</summary>
        private static bool IsUsable(DialogueSystem ds)
        {
            return ds != null && ds.dialogueBox != null && ds.dialogueText != null;
        }

        private void Awake()
        {
            if (_instance == null) _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        private void Start()
        {
            if (dialogueBox != null && !isOpen)
            {
                dialogueBox.SetActive(false);
            }
        }

        public bool IsDialogueActive()
        {
            return isOpen;
        }

        /// <summary>
        /// Crea en runtime un Canvas + caja de diálogo + texto si la escena no tiene
        /// un DialogueSystem. El componente vive en el Canvas (siempre activo) y solo
        /// se activa/desactiva el panel hijo (la caja).
        /// </summary>
        private static DialogueSystem CreateRuntime()
        {
            GameObject canvasGO = new GameObject("DialogueSystem_Runtime",
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            DialogueSystem ds = canvasGO.AddComponent<DialogueSystem>();

            // Caja (panel hijo que se togglea)
            GameObject box = new GameObject("DialogueBox", typeof(RectTransform), typeof(Image));
            box.transform.SetParent(canvasGO.transform, false);
            box.GetComponent<Image>().color = new Color(0.05f, 0.05f, 0.1f, 0.95f);
            RectTransform brt = box.GetComponent<RectTransform>();
            brt.anchorMin = new Vector2(0.1f, 0.05f);
            brt.anchorMax = new Vector2(0.9f, 0.05f);
            brt.pivot = new Vector2(0.5f, 0f);
            brt.sizeDelta = new Vector2(0, 220);
            brt.anchoredPosition = new Vector2(0, 20);
            var outline = box.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0.898f, 1f, 1f);
            outline.effectDistance = new Vector2(3, -3);

            // Texto
            GameObject txt = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            txt.transform.SetParent(box.transform, false);
            RectTransform trt = txt.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(25, 20); trt.offsetMax = new Vector2(-25, -20);
            TextMeshProUGUI tmp = txt.GetComponent<TextMeshProUGUI>();
            tmp.text = ""; tmp.fontSize = 35; tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.TopLeft;

            ds.dialogueText = tmp;
            ds.dialogueBox = box;
            box.SetActive(false);

            return ds;
        }

        private void Update()
        {
            // Ignoramos input de teclado si estamos mostrando botones de elección
            if (choiceButtons.Count > 0) return;

            // Presionar Z, Enter, Space o Botón A de control (JoystickButton0) para avanzar/completar el diálogo
            if (dialogueBox != null && dialogueBox.activeSelf)
            {
                if (Input.GetKeyDown(KeyCode.Z) || 
                    Input.GetKeyDown(KeyCode.Return) || 
                    Input.GetKeyDown(KeyCode.Space) ||
                    Input.GetKeyDown(KeyCode.JoystickButton0))
                {
                    if (isTyping)
                    {
                        // Completar el texto inmediatamente
                        CompleteText();
                    }
                    else
                    {
                        // Avanzar a la siguiente línea de la historia
                        ShowNextLine();
                    }
                }
            }
        }

        // ═══════════════════════════════════════
        // API PÚBLICA
        // ═══════════════════════════════════════

        public void StartDialogueStory(HeartQuest.Core.DialogueData story)
        {
            if (story == null) 
            {
                Debug.LogWarning("[DialogueSystem] El DialogueData recibido es NULL.");
                return;
            }
            if (story.lines == null || story.lines.Length == 0) 
            {
                Debug.LogWarning($"[DialogueSystem] El DialogueData '{story.name}' no tiene líneas de texto configuradas.");
                return;
            }

            Debug.Log($"[DialogueSystem] Iniciando diálogo: {story.name} con {story.lines.Length} líneas.");

            // Limpiar botones viejos si los hubiera
            ClearChoices();

            currentStory = story;
            currentLineIndex = 0;
            isOpen = true;
            dialogueBox.SetActive(true);

            ShowNextLine();
        }

        private void ShowNextLine()
        {
            if (currentStory != null && currentLineIndex < currentStory.lines.Length)
            {
                var line = currentStory.lines[currentLineIndex];
                string parsedText = line.text.Replace("{PLAYER_NAME}", PlayerPrefs.GetString("PlayerName", "Jugador"));
                ShowDialogue($"<color=#00E5FF>{line.speakerName}</color>\n{parsedText}", line.portrait);
                currentLineIndex++;
            }
            else if (currentStory != null)
            {
                SpawnChoices();
            }
            else
            {
                HideDialogue();
            }
        }

        public void ShowDialogue(string text, Sprite portrait = null)
        {
            Debug.Log($"[DialogueSystem] ShowDialogue llamado con texto: {text}");
            if (dialogueBox == null || dialogueText == null) 
            {
                Debug.LogWarning("[DialogueSystem] Error: dialogueBox o dialogueText es nulo.");
                return;
            }

            currentFullText = text;
            isOpen = true;
            dialogueBox.SetActive(true);
            Debug.Log($"[DialogueSystem] dialogueBox fue activado. activeInHierarchy: {dialogueBox.activeInHierarchy}");

            if (portraitImage != null)
            {
                if (portrait != null)
                {
                    portraitImage.sprite = portrait;
                    portraitImage.gameObject.SetActive(true);
                }
                else
                {
                    portraitImage.gameObject.SetActive(false);
                }
            }

            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }

            dialogueText.text = text;
            dialogueText.maxVisibleCharacters = 0;

            typingCoroutine = StartCoroutine(TypewriterEffect(text));
        }

        public void HideDialogue()
        {
            Debug.Log("[DialogueSystem] HideDialogue ha sido llamado. Ocultando diálogo.");
            isOpen = false;
            if (dialogueBox != null)
            {
                dialogueBox.SetActive(false);
            }
            isTyping = false;
            ClearChoices();
        }

        public void CompleteText()
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }

            if (dialogueText != null)
            {
                // Forzamos a que todo el texto sea visible
                dialogueText.maxVisibleCharacters = 99999;
            }

            isTyping = false;
        }

        // ═══════════════════════════════════════
        // SISTEMA DE ELECCIONES
        // ═══════════════════════════════════════

        private void SpawnChoices()
        {
            if (currentStory.requiresNameInput)
            {
                SpawnNameInput();
                return;
            }

            if (currentStory.choices == null || currentStory.choices.Length == 0)
            {
                FinishDialogueStory();
                return;
            }

            // Ocultamos el retrato y mostramos un texto de elección
            if (portraitImage != null) portraitImage.gameObject.SetActive(false);
            
            dialogueText.text = "<color=#9D4DFF>¿Qué vas a hacer?</color>";
            dialogueText.maxVisibleCharacters = 99999;

            for (int i = 0; i < currentStory.choices.Length; i++)
            {
                var choice = currentStory.choices[i];
                GameObject btnObj = new GameObject("ChoiceBtn_" + i, typeof(RectTransform), typeof(Image), typeof(Button));
                btnObj.transform.SetParent(dialogueBox.transform, false);
                
                RectTransform rt = btnObj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0); rt.anchorMax = new Vector2(0.5f, 0);
                rt.pivot = new Vector2(0.5f, 0);
                rt.sizeDelta = new Vector2(400, 45);
                
                // Apilarlos desde abajo hacia arriba
                float yPos = 30 + ((currentStory.choices.Length - 1 - i) * 55);
                rt.anchoredPosition = new Vector2(0, yPos);

                Image img = btnObj.GetComponent<Image>();
                img.color = new Color(0.12f, 0.16f, 0.25f, 1f); // Azul oscuro cyberpunk

                // Borde neón al botón
                var outline = btnObj.AddComponent<Outline>();
                outline.effectColor = new Color(0.6f, 0.3f, 1f, 1f); // Morado neón
                outline.effectDistance = new Vector2(2, -2);

                Button btn = btnObj.GetComponent<Button>();
                int index = i; // Closure
                btn.onClick.AddListener(() => OnChoiceSelected(index));

                // Texto del botón
                GameObject txtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                txtObj.transform.SetParent(btnObj.transform, false);
                RectTransform trt = txtObj.GetComponent<RectTransform>();
                trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
                trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;

                TextMeshProUGUI tmp = txtObj.GetComponent<TextMeshProUGUI>();
                tmp.text = choice.choiceText;
                tmp.color = Color.white;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.fontSize = 24;

                choiceButtons.Add(btnObj);
            }
        }

        private void OnChoiceSelected(int index)
        {
            var choice = currentStory.choices[index];
            ClearChoices();

            // Aplicar moral de la elección
            if (choice.moraleChange != 0)
            {
                var gm = Object.FindAnyObjectByType<AntiBullyingGame.Core.GameManager>();
                if (gm != null)
                {
                    if (choice.moraleChange > 0) gm.AddMorale(choice.moraleChange);
                    else gm.DeductMorale(-choice.moraleChange);
                }
            }

            // Continuar historia
            if (choice.nextDialogue != null)
            {
                StartDialogueStory(choice.nextDialogue);
            }
            else
            {
                HideDialogue();
                currentStory = null;
            }
        }

        private void ClearChoices()
        {
            foreach(var b in choiceButtons)
            {
                if (b != null) Destroy(b);
            }
            choiceButtons.Clear();
        }

        private void FinishDialogueStory()
        {
            HideDialogue();
            
            bool shouldTriggerBattle = false;
            
            if (currentStory != null)
            {
                if (currentStory.moraleChangeOnComplete != 0)
                {
                    var gm = Object.FindAnyObjectByType<AntiBullyingGame.Core.GameManager>();
                    if (gm != null)
                    {
                        if (currentStory.moraleChangeOnComplete > 0) gm.AddMorale(currentStory.moraleChangeOnComplete);
                        else gm.DeductMorale(-currentStory.moraleChangeOnComplete);
                    }
                }
                
                if (currentStory.triggersBattle)
                {
                    shouldTriggerBattle = true;
                }
            }
            
            currentStory = null;
            
            if (shouldTriggerBattle)
            {
                var gm = Object.FindAnyObjectByType<AntiBullyingGame.Core.GameManager>();
                if (gm != null)
                {
                    gm.StartBattle();
                }
            }
        }

        private void SpawnNameInput()
        {
            if (portraitImage != null) portraitImage.gameObject.SetActive(false);
            
            dialogueText.text = "<color=#00E5FF>¿Cuál es tu nombre?</color>";
            dialogueText.maxVisibleCharacters = 99999;

            GameObject inputObj = new GameObject("NameInput", typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
            inputObj.transform.SetParent(dialogueBox.transform, false);
            
            RectTransform rt = inputObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(400, 50);
            rt.anchoredPosition = new Vector2(0, -30);

            Image img = inputObj.GetComponent<Image>();
            img.color = new Color(0.12f, 0.16f, 0.25f, 1f);

            var outline = inputObj.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0.898f, 1f, 1f); // Cian
            outline.effectDistance = new Vector2(2, -2);

            GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(inputObj.transform, false);
            RectTransform trt = textObj.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(10, 0); trt.offsetMax = Vector2.zero;

            TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
            tmp.color = Color.white;
            tmp.fontSize = 30;

            TMP_InputField inputField = inputObj.GetComponent<TMP_InputField>();
            inputField.textComponent = tmp;

            choiceButtons.Add(inputObj);

            // Botón Confirmar
            GameObject btnObj = new GameObject("ConfirmBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(dialogueBox.transform, false);
            RectTransform brt = btnObj.GetComponent<RectTransform>();
            brt.anchorMin = new Vector2(0.5f, 0.5f); brt.anchorMax = new Vector2(0.5f, 0.5f);
            brt.sizeDelta = new Vector2(200, 45);
            brt.anchoredPosition = new Vector2(0, -90);

            Image bimg = btnObj.GetComponent<Image>();
            bimg.color = new Color(0f, 0.898f, 1f, 1f);

            GameObject bTextObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            bTextObj.transform.SetParent(btnObj.transform, false);
            RectTransform btrt = bTextObj.GetComponent<RectTransform>();
            btrt.anchorMin = Vector2.zero; btrt.anchorMax = Vector2.one;
            btrt.offsetMin = Vector2.zero; btrt.offsetMax = Vector2.zero;
            TextMeshProUGUI btmp = bTextObj.GetComponent<TextMeshProUGUI>();
            btmp.text = "CONFIRMAR";
            btmp.color = Color.black;
            btmp.alignment = TextAlignmentOptions.Center;
            btmp.fontSize = 24;

            Button btn = btnObj.GetComponent<Button>();
            btn.onClick.AddListener(() => 
            {
                if (!string.IsNullOrEmpty(inputField.text))
                {
                    PlayerPrefs.SetString("PlayerName", inputField.text);
                    PlayerPrefs.Save();
                    
                    var gm = Object.FindAnyObjectByType<AntiBullyingGame.Core.GameManager>();
                    if (gm != null) {
                        // Cambiamos temporalmente el nombre de entidad si queremos, pero en prefs está guardado
                    }

                    ClearChoices();
                    if (currentStory.nextDialogueAfterInput != null)
                        StartDialogueStory(currentStory.nextDialogueAfterInput);
                    else
                        FinishDialogueStory();
                }
            });

            choiceButtons.Add(btnObj);
        }

        // ═══════════════════════════════════════
        // CORRUTINAS
        // ═══════════════════════════════════════

        private IEnumerator TypewriterEffect(string text)
        {
            Debug.Log("[DialogueSystem] Iniciando TypewriterEffect...");
            isTyping = true;
            
            // TRUCO: Esperar 1 frame. Si el DialogueBox acaba de ser activado, TextMeshPro necesita 1 frame para inicializarse.
            yield return null;
            
            try 
            {
                // TextMeshPro permite parsear el Rich Text (ej: <color=...>) primero,
                // y luego revelar los caracteres 1 a 1 usando maxVisibleCharacters.
                dialogueText.ForceMeshUpdate(); // Construye el texto con etiquetas
            } 
            catch (System.Exception e) 
            {
                Debug.LogError($"[DialogueSystem] Error en TypewriterEffect al preparar el texto: {e.Message}");
            }

            int totalCharacters = dialogueText.textInfo.characterCount;
            float charDelay = 1f / charactersPerSecond;

            for (int i = 1; i <= totalCharacters; i++)
            {
                dialogueText.maxVisibleCharacters = i;

                // Sonido
                if (typingAudioSource != null && typingSound != null && i % 2 == 0)
                {
                    typingAudioSource.PlayOneShot(typingSound);
                }

                // Pausa extra si hay puntuación
                char c = dialogueText.textInfo.characterInfo[i - 1].character;
                if (c == '.' || c == ',' || c == '!' || c == '?' || c == ':')
                {
                    yield return new WaitForSeconds(pauseOnPunctuation);
                }
                else
                {
                    yield return new WaitForSeconds(charDelay);
                }
            }

            Debug.Log("[DialogueSystem] TypewriterEffect completado.");
            isTyping = false;
        }
    }
}
