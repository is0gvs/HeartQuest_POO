using UnityEngine;
using AntiBullyingGame.Interfaces;

namespace AntiBullyingGame.RPG
{
    /// <summary>
    /// NPC que representa a un adulto (profesor, padre, consejero, etc).
    /// Implementa IInteractable (Polimorfismo por Interfaz).
    /// </summary>
    public class Adult : NPC
    {
        protected override void DefaultInteraction()
        {
            Speak("Hola joven, ¿todo está bien? Si necesitas ayuda, aquí estoy.");
            
            // Podemos añadir algo de moral o dejarlo así como consejo.
            var gm = Object.FindAnyObjectByType<Core.GameManager>();
            if (gm != null) gm.AddMorale(5);
        }
    }
}
