using UnityEngine;
using AntiBullyingGame.Core;
using AntiBullyingGame.RPG;

namespace AntiBullyingGame
{
    /// <summary>
    /// Este script es una demostración rápida para ejecutar la lógica de 
    /// Programación Orientada a Objetos en la Consola de Unity sin necesidad 
    /// de controlar físicamente al personaje aún.
    /// </summary>
    public class SimulationDemo : MonoBehaviour
    {
        void Start()
        {
            Debug.Log("<color=yellow>--- INICIANDO SIMULACIÓN DE POO ---</color>");

            // 1. Instanciamos el Sujeto del Patrón Observer (Game Manager)
            GameManager gm = gameObject.AddComponent<GameManager>();

            // 2. Instanciamos las clases polimórficas
            Bully elBully = gameObject.AddComponent<Bully>();
            Victim laVictima = gameObject.AddComponent<Victim>();

            // 3. Probamos la interacción: Polimorfismo en acción
            Debug.Log("---> Interacción con el Acosador:");
            elBully.Interact(); 
            // Esperado: Restará puntos de moral usando el GameManager.

            Debug.Log("---> Interacción con la Víctima:");
            laVictima.Interact();
            // Esperado: Sumará puntos de moral usando el GameManager.

            Debug.Log($"Resultado de la Moral Final del colegio: <color=green>{gm.CurrentMorale}</color>");
        }
    }
}
