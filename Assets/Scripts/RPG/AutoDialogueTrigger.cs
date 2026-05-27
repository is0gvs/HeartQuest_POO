using UnityEngine;
using System.Collections;
using HeartQuest.Core;
using HeartQuest.UI;

namespace AntiBullyingGame.RPG
{
    /// <summary>
    /// Componente para iniciar un diálogo automáticamente al cargar la escena
    /// sin requerir interacción física del jugador.
    /// </summary>
    public class AutoDialogueTrigger : MonoBehaviour
    {
        [Header("Configuración de Diálogo")]
        [Tooltip("El ScriptableObject de DialogueData que se reproducirá automáticamente.")]
        [SerializeField] private DialogueData dialogueData;

        [Tooltip("Tiempo de espera (en segundos) antes de iniciar el diálogo tras cargar la escena.")]
        [SerializeField] private float delay = 0.2f;

        [Header("Control de Frecuencia (Opcional)")]
        [Tooltip("Si está marcado, este diálogo solo se reproducirá una vez en toda la partida.")]
        [SerializeField] private bool playOnlyOnce = false;

        [Tooltip("ID único para guardar el estado de reproducción de este diálogo. Obligatorio si 'playOnlyOnce' está activo.")]
        [SerializeField] private string triggerId;

        private IEnumerator Start()
        {
            if (dialogueData == null)
            {
                Debug.LogWarning("[AutoDialogueTrigger] No se asignó dialogueData en " + gameObject.name);
                yield break;
            }

            // Si es de reproducción única, comprobar el estado guardado
            if (playOnlyOnce && !string.IsNullOrEmpty(triggerId))
            {
                if (PlayerPrefs.GetInt(triggerId + "_Played", 0) == 1)
                {
                    Debug.Log($"[AutoDialogueTrigger] Diálogo {triggerId} ya fue reproducido antes. Omitiendo.");
                    yield break;
                }
            }

            // Pequeña espera para asegurar que todo el canvas y DialogueSystem de la escena se hayan inicializado
            yield return new WaitForSeconds(delay);

            var ds = DialogueSystem.Instance;
            if (ds != null)
            {
                Debug.Log($"[AutoDialogueTrigger] Iniciando diálogo automático: {dialogueData.name}");
                ds.StartDialogueStory(dialogueData);

                // Si es de reproducción única, marcar como reproducido
                if (playOnlyOnce && !string.IsNullOrEmpty(triggerId))
                {
                    PlayerPrefs.SetInt(triggerId + "_Played", 1);
                    PlayerPrefs.Save();
                }
            }
            else
            {
                Debug.LogError("[AutoDialogueTrigger] No se pudo encontrar una instancia de DialogueSystem en la escena.");
            }
        }
    }
}
