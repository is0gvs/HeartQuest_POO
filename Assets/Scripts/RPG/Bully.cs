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
        protected override void DefaultInteraction()
        {
            Speak("¿Qué me miras? ¡Largo de aquí!");
            var gm = Object.FindAnyObjectByType<Core.GameManager>();
            if (gm != null) gm.DeductMorale(5);
        }
    }
}
