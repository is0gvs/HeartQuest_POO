using UnityEngine;
using AntiBullyingGame.Interfaces;

namespace AntiBullyingGame.RPG
{
    /// <summary>
    /// NPC que representa a un acosador escolar.
    /// Implementa IInteractable (Polimorfismo por Interfaz).
    /// </summary>
    public class Bully : NPC
    {
        private const string BullyResolvedNpcId = "bully_mateo_reformed";

        protected override void DefaultInteraction()
        {
            if (IsReformed())
            {
                Speak("Perdon por lo de antes. Ya devolvi la mochila y voy a intentar hacer las cosas bien.");
                return;
            }

            Speak("¿Qué me miras? ¡Largo de aquí!");
            var gm = Object.FindAnyObjectByType<Core.GameManager>();
            if (gm != null) gm.DeductMorale(5);
        }

        private bool IsReformed()
        {
            PlayerPrefs.DeleteKey("BullyMateoResolved");
            return AntiBullyingGame.Managers.SaveManager.Instance != null &&
                   AntiBullyingGame.Managers.SaveManager.Instance.interactedNPCs.Contains(BullyResolvedNpcId);
        }
    }
}
