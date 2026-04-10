using UnityEngine;
using AntiBullyingGame.Interfaces;

namespace AntiBullyingGame.RPG
{
    /// <summary>
    /// Personaje no jugable base.
    /// Hereda de Character e implementa IInteractable (POO: Herencia con Interfaces).
    /// </summary>
    public abstract class NPC : Character, IInteractable
    {
        // Método exigido por la interfaz IInteractable
        // Declarado virtual para que tipos específicos de NPC reaccionen distinto (Polimorfismo)
        public virtual void Interact()
        {
            Debug.Log($"[{EntityName}] te mira pero no dice nada.");
        }
    }
}
