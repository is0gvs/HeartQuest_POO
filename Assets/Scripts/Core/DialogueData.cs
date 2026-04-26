using UnityEngine;

namespace HeartQuest.Core
{
    [System.Serializable]
    public struct DialogueLine
    {
        public string speakerName;
        [TextArea(3, 5)]
        public string text;
        public Sprite portrait;
    }

    [System.Serializable]
    public struct DialogueChoice
    {
        public string choiceText;
        public DialogueData nextDialogue; // Siguiente historia si elige esto
        public int moraleChange; // Cambio de moral al elegir esto
    }

    /// <summary>
    /// ScriptableObject para almacenar diálogos.
    /// Permite crear "archivos" de historia que se pueden asignar a cualquier NPC.
    /// </summary>
    [CreateAssetMenu(fileName = "NewDialogueData", menuName = "POO Game/Dialogue Data")]
    public class DialogueData : ScriptableObject
    {
        [Header("Contenido de la Historia")]
        public DialogueLine[] lines;

        [Header("Elecciones (Opcional)")]
        [Tooltip("Si dejas esto vacío, el diálogo simplemente terminará. Si pones opciones, el jugador deberá elegir.")]
        public DialogueChoice[] choices;

        [Header("Input Especial (Guardar Nombre)")]
        public bool requiresNameInput = false;
        public DialogueData nextDialogueAfterInput;

        [Header("Consecuencias por Defecto (Si no hay elecciones)")]
        [Tooltip("Cantidad de moral a sumar (positivo) o restar (negativo) al terminar de hablar.")]
        public int moraleChangeOnComplete = 0;

        [Header("Combate")]
        [Tooltip("Si es verdadero, al terminar el diálogo se cargará la escena de batalla.")]
        public bool triggersBattle = false;
    }
}
