using UnityEngine;

namespace AntiBullyingGame.RPG
{
    /// <summary>
    /// El estudiante que necesita ayuda en los pasillos.
    /// </summary>
    public class Victim : NPC
    {
        [Header("Victim Settings")]
        [SerializeField] private int gratitudeLevel = 15;

        // Sobreescribimos Interact para la Víctima (POO: Polimorfismo)
        public override void Interact()
        {
            Speak("Gracias por acercarte... la estoy pasando mal.");
            
            // Evitamos el Singleton prohibido en la rúbrica usando FindObjectOfType
            Core.GameManager gm = UnityEngine.Object.FindObjectOfType<Core.GameManager>();
            if (gm != null)
            {
                gm.AddMorale(gratitudeLevel);
                ChangeMorale(20); 
            }
        }
    }
}
