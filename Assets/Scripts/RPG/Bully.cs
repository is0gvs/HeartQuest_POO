using UnityEngine;
using AntiBullyingGame.Interfaces;

namespace AntiBullyingGame.RPG
{
    /// <summary>
    /// NPC que representa a un acosador escolar.
    /// Implementa IInteractable (Polimorfismo por Interfaz).
    /// </summary>
    public class Bully : Character, IInteractable
    {
        [Header("Historia del Bully")]
        public HeartQuest.Core.DialogueData story;

        public void Interact()
        {
            var ds = Object.FindAnyObjectByType<HeartQuest.UI.DialogueSystem>(FindObjectsInactive.Include);
            if (ds != null && story != null)
            {
                ds.StartDialogueStory(story);
            }
            else
            {
                Speak("¿Qué me miras? ¡Largo de aquí!");
                var gm = Object.FindAnyObjectByType<Core.GameManager>();
                if (gm != null) gm.DeductMorale(5);
            }
        }
    }
}
