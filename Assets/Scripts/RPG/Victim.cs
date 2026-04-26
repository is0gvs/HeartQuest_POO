using UnityEngine;
using AntiBullyingGame.Interfaces;

namespace AntiBullyingGame.RPG
{
    /// <summary>
    /// NPC que representa a un estudiante siendo acosado.
    /// Implementa IInteractable (Polimorfismo por Interfaz).
    /// </summary>
    public class Victim : Character, IInteractable
    {
        [Header("Historia de la Víctima")]
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
                Speak("Gracias por acercarte... la estoy pasando mal.");
                var gm = Object.FindAnyObjectByType<Core.GameManager>();
                if (gm != null) gm.AddMorale(10);
            }
        }
    }
}
