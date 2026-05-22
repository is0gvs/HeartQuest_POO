using UnityEngine;
using AntiBullyingGame.Interfaces;
using AntiBullyingGame.Managers;

namespace AntiBullyingGame.RPG
{
    /// <summary>
    /// Personaje no jugable base.
    /// Hereda de Character e implementa IInteractable (POO: Herencia con Interfaces).
    /// </summary>
    public abstract class NPC : Character, IInteractable
    {
        [Header("Configuración de Guardado")]
        [Tooltip("ID único para guardar si ya se habló con este NPC. Ejemplo: 'Victima_Pasillo1'")]
        public string npcId;

        [Header("Historias del NPC")]
        public HeartQuest.Core.DialogueData firstEncounterStory;
        public HeartQuest.Core.DialogueData repeatEncounterStory;

        public virtual void Interact()
        {
            var ds = HeartQuest.UI.DialogueSystem.Instance;

            if (ds != null)
            {
                bool hasInteracted = HasInteracted();
                Debug.Log($"[NPC] Interactuando con {EntityName} (ID: {npcId}). hasInteracted = {hasInteracted}");

                if (!hasInteracted && firstEncounterStory != null)
                {
                    Debug.Log($"[NPC] Mostrando firstEncounterStory. Guardando estado...");
                    MarkAsInteracted();
                    ds.StartDialogueStory(firstEncounterStory);
                }
                else if (hasInteracted && repeatEncounterStory != null)
                {
                    Debug.Log($"[NPC] Mostrando repeatEncounterStory.");
                    ds.StartDialogueStory(repeatEncounterStory);
                }
                else
                {
                    Debug.Log($"[NPC] No hay historia para este estado. Usando DefaultInteraction.");
                    DefaultInteraction();
                }
            }
            else
            {
                Debug.Log($"[NPC] No se encontró DialogueSystem. Usando DefaultInteraction.");
                DefaultInteraction();
            }
        }

        protected virtual void DefaultInteraction()
        {
            Debug.Log($"[{EntityName}] te mira pero no dice nada.");
        }

        protected bool HasInteracted()
        {
            if (string.IsNullOrEmpty(npcId)) return false;
            if (SaveManager.Instance != null)
            {
                return SaveManager.Instance.interactedNPCs.Contains(npcId);
            }
            return false;
        }

        protected void MarkAsInteracted()
        {
            if (string.IsNullOrEmpty(npcId)) return;
            if (SaveManager.Instance != null && !SaveManager.Instance.interactedNPCs.Contains(npcId))
            {
                SaveManager.Instance.interactedNPCs.Add(npcId);
            }
        }
    }
}
