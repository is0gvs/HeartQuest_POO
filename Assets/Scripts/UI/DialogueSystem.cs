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

        private void Start()
        {
            if (dialogueBox != null)
            {
                dialogueBox.SetActive(false);
            }
        }

        private void Update()
        {
            // Ignoramos input de teclado si estamos mostrando botones de elección
            if (choiceButtons.Count > 0) return;

            // Presionar Z, Enter o Space para avanzar/completar el diálogo
            if (dialogueBox != null && dialogueBox.activeSelf)
            {
                if (Input.GetKeyDown(KeyCode.Z) || 
                    Input.GetKeyDown(KeyCode.Return) || 
                    Input.GetKeyDown(KeyCode.Space))
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
            if (story == null || story.lines == null || story.lines.Length == 0) return;

            // Limpiar botones viejos si los hubiera
            ClearChoices();

            currentStory = story;
            currentLineIndex = 0;
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
            if (dialogueBox == null || dialogueText == null) return;

            currentFullText = text;
            dialogueBox.SetActive(true);

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

            typingCoroutine = StartCoroutine(TypewriterEffect(text));
        }

        public void HideDialogue()
        {
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
            isTyping = true;
            
            // TextMeshPro permite parsear el Rich Text (ej: <color=...>) primero,
            // y luego revelar los caracteres 1 a 1 usando maxVisibleCharacters.
            dialogueText.text = text;
            dialogueText.maxVisibleCharacters = 0;
            dialogueText.ForceMeshUpdate(); // Construye el texto con etiquetas

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

            isTyping = false;
        }
    }
}
