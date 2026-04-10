using UnityEngine;
using System.Collections.Generic;

namespace AntiBullyingGame.Core
{
    /// <summary>
    /// GameManager maneja el estado global del juego.
    /// Utiliza el PATRÓN DE DISEÑO OBSERVER estrictamente, eliminando el Singleton 
    /// (Para cumplir el requisito 2.c de la cátedra educativa).
    /// </summary>
    public class GameManager : MonoBehaviour, ISubject
    {
        [Header("Player Stats")]
        [SerializeField] private int currentMorale = 100;
        public int CurrentMorale => currentMorale; // Encapsulamiento

        [Header("Game State")]
        public bool IsInTowerDefenseMode { get; private set; }

        // Lista de objetos que "observan" a este GameManager (Patrón de Diseño Observer)
        private List<IObserver> observers = new List<IObserver>();

        // Métodos de la interfaz ISubject
        public void Attach(IObserver observer)
        {
            if (!observers.Contains(observer))
                observers.Add(observer);
        }

        public void Detach(IObserver observer)
        {
            observers.Remove(observer);
        }

        public void NotifyObservers()
        {
            // Notificamos a todos los que estén suscritos (ej. el sistema de salud en pantalla)
            foreach (var obs in observers)
            {
                obs.OnMoraleUpdated(currentMorale);
            }
        }

        // Acciones que alteran el juego
        public void AddMorale(int amount)
        {
            currentMorale += amount;
            Debug.Log($"Moral actual: {currentMorale}");
            NotifyObservers(); 
        }

        public void DeductMorale(int amount)
        {
            currentMorale -= amount;
            Debug.Log($"Moral actual: {currentMorale}");
            NotifyObservers();

            if (currentMorale <= 30 && !IsInTowerDefenseMode)
            {
                SwitchToTowerDefenseMode();
            }
        }

        private void SwitchToTowerDefenseMode()
        {
            IsInTowerDefenseMode = true;
            Debug.Log("Moral demasiado baja: ¡Cambiando al estado de Defender!");
        }
    }
}
