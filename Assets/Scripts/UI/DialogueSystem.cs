using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

namespace HeartQuest.UI
{
    /// <summary>
    /// Sistema de diálogos estilo Undertale con estética cyberpunk.
    /// Produce un efecto de "máquina de escribir" (typewriter) para
    /// revelar texto carácter por carácter con sonido opcional.
    /// </summary>
    public class DialogueSystem : MonoBehaviour
    {
        [Header("── Componentes UI ──")]
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private Image portraitImage;
        [SerializeField] private GameObject dialogueBox;

        [Header("── Configuración ──")]
        [SerializeField] private float charactersPerSecond = 30f;
        [SerializeField] private float pauseOnPunctuation = 0.15f;

        [Header("── Sonido (Opcional) ──")]
        [SerializeField] private AudioSource typingAudioSource;
        [SerializeField] private AudioClip typingSound;

        [Header("── Auto Avance ──")]
        [SerializeField] private bool autoAdvance = false;
        [SerializeField] private float autoAdvanceDelay = 2f;

        // Estado interno de la conversación
        private HeartQuest.Core.DialogueData currentStory;
        private int currentLineIndex = 0;

        private Coroutine typingCoroutine;
        private bool isTyping = false;
        private string currentFullText = "";

        private void Start()
        {
            if (dialogueBox != null)
            {
                dialogueBox.SetActive(false);
            }
        }

        private void Update()
        {
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

        /// <summary>
        /// Inicia una historia completa usando un ScriptableObject (DialogueData)
        /// </summary>
        public void StartDialogueStory(HeartQuest.Core.DialogueData story)
        {
            if (story == null || story.lines == null || story.lines.Length == 0) return;

            currentStory = story;
            currentLineIndex = 0;
            dialogueBox.SetActive(true);
            
            ShowNextLine();
        }

        /// <summary>
        /// Avanza la historia. Si se acabó, cierra la caja y aplica consecuencias.
        /// </summary>
        private void ShowNextLine()
        {
            if (currentStory != null && currentLineIndex < currentStory.lines.Length)
            {
                var line = currentStory.lines[currentLineIndex];
                ShowDialogue($"<color=#00E5FF>{line.speakerName}</color>\n{line.text}", line.portrait);
                currentLineIndex++;
            }
            else
            {
                HideDialogue();
                
                // Aplicar consecuencias al terminar
                if (currentStory != null && currentStory.moraleChangeOnComplete != 0)
                {
                    var gm = UnityEngine.Object.FindAnyObjectByType<AntiBullyingGame.Core.GameManager>();
                    if (gm != null)
                    {
                        if (currentStory.moraleChangeOnComplete > 0)
                            gm.AddMorale(currentStory.moraleChangeOnComplete);
                        else
                            gm.DeductMorale(-currentStory.moraleChangeOnComplete);
                    }
                }
                currentStory = null;
            }
        }

        /// <summary>
        /// Muestra un diálogo individual con efecto de typewriter.
        /// </summary>
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

            // Detener typewriting anterior si hay uno en curso
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }

            typingCoroutine = StartCoroutine(TypewriterEffect(text));
        }

        /// <summary>
        /// Oculta la caja de diálogo.
        /// </summary>
        public void HideDialogue()
        {
            if (dialogueBox != null)
            {
                dialogueBox.SetActive(false);
            }
            isTyping = false;
        }

        /// <summary>
        /// Muestra el texto completo de inmediato, sin animación.
        /// </summary>
        public void CompleteText()
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }

            if (dialogueText != null)
            {
                dialogueText.text = currentFullText;
            }

            isTyping = false;
        }

        // ═══════════════════════════════════════
        // CORRUTINAS
        // ═══════════════════════════════════════

        /// <summary>
        /// Efecto de máquina de escribir carácter por carácter.
        /// </summary>
        private IEnumerator TypewriterEffect(string text)
        {
            isTyping = true;
            dialogueText.text = "";

            float charDelay = 1f / charactersPerSecond;

            foreach (char c in text)
            {
                dialogueText.text += c;

                // Reproducir sonido de tipeo
                if (typingAudioSource != null && typingSound != null && c != ' ')
                {
                    typingAudioSource.PlayOneShot(typingSound);
                }

                // Pausa extra en signos de puntuación
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

            // Auto avance si está habilitado
            if (autoAdvance)
            {
                yield return new WaitForSeconds(autoAdvanceDelay);
                HideDialogue();
            }
        }
    }
}
