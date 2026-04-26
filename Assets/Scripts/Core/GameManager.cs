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
        [SerializeField] private int maxHp = 500;
        [SerializeField] private int currentHp = 280;
        
        [SerializeField] private int maxMorale = 200;
        [SerializeField] private int currentMorale = 70;
        
        [SerializeField] private int currentXp = 0;
        [SerializeField] private int maxXp = 100;
        [SerializeField] private int currentLevel = 14;

        public int CurrentHp => currentHp;
        public int MaxHp => maxHp;
        public int CurrentMorale => currentMorale;
        public int MaxMorale => maxMorale;
        public int CurrentXp => currentXp;
        public int MaxXp => maxXp;
        public int CurrentLevel => currentLevel;

        [Header("Game State")]
        public bool IsInTowerDefenseMode { get; private set; }

        // Lista de objetos que "observan" a este GameManager (Patrón de Diseño Observer)
        private List<IObserver> observers = new List<IObserver>();

        private void Awake()
        {
            // Asegurar que el GameManager sobreviva al cambio de escena
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            if (scene.name == "main scene")
            {
                // Configurar cámara de batalla
                Camera mainCam = Camera.main;
                if (mainCam != null)
                {
                    mainCam.clearFlags = CameraClearFlags.SolidColor;
                    mainCam.backgroundColor = Color.black;
                }
                Time.timeScale = 1f;
                Debug.Log("Configuración de Batalla Aplicada: Fondo Negro.");
            }
        }

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
            foreach (var obs in observers)
            {
                obs.OnStatsUpdated(currentHp, maxHp, currentMorale, maxMorale, currentXp, maxXp, currentLevel);
            }
        }

        // Acciones que alteran el juego
        public void AddMorale(int amount)
        {
            currentMorale += amount;
            if (currentMorale > maxMorale) currentMorale = maxMorale;
            Debug.Log($"Moral actual: {currentMorale}");
            NotifyObservers(); 
        }

        public void DeductMorale(int amount)
        {
            currentMorale -= amount;
            if (currentMorale < 0) currentMorale = 0;
            Debug.Log($"Moral actual: {currentMorale}");
            NotifyObservers();

            if (currentMorale <= 30 && !IsInTowerDefenseMode)
            {
                SwitchToTowerDefenseMode();
            }
        }

        public void TakeDamage(int amount)
        {
            currentHp -= amount;
            if (currentHp < 0) currentHp = 0;
            NotifyObservers();
        }

        public void AddXp(int amount)
        {
            currentXp += amount;
            if (currentXp >= maxXp)
            {
                currentLevel++;
                currentXp -= maxXp;
                maxXp = Mathf.RoundToInt(maxXp * 1.5f);
            }
            NotifyObservers();
        }

        private void SwitchToTowerDefenseMode()
        {
            IsInTowerDefenseMode = true;
            Debug.Log("Moral demasiado baja: ¡Cambiando al estado de Defender!");
        }

        public void StartBattle()
        {
            Debug.Log("Iniciando Batalla AntiBullying...");
            
            // Ocultar la UI de la escuela para que no tape la batalla
            GameObject ui = GameObject.Find("UI_Presentation");
            if (ui != null) ui.SetActive(false);

            // Asegurar que el tiempo esté corriendo antes del cambio
            Time.timeScale = 1f;

            // Carga la escena de batalla unificada
            UnityEngine.SceneManagement.SceneManager.LoadScene("BattleScene");
        }
    }
}
