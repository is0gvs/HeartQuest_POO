using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems; // Para controlar la selección de UI
using AntiBullyingGame.Managers; // Para acceder a SaveManager

namespace AntiBullyingGame.UI
{
    public class PauseMenu : MonoBehaviour
    {
        [Header("UI Elements")]
        [Tooltip("Asigna aquí el Panel del menú de pausa")]
        public GameObject pauseMenuUI;

        [Tooltip("Asigna aquí el botón de Reanudar para seleccionarlo automáticamente (útil para teclado/mando)")]
        public GameObject firstButton;

        public static bool GameIsPaused = false;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (GameIsPaused)
                {
                    Resume();
                }
                else
                {
                    Pause();
                }
            }
        }

        public void Resume()
        {
            pauseMenuUI.SetActive(false);
            Time.timeScale = 1f;
            GameIsPaused = false;
        }

        void Pause()
        {
            pauseMenuUI.SetActive(true);
            Time.timeScale = 0f;
            GameIsPaused = true;

            // Limpiar selección actual y seleccionar el primer botón
            if (firstButton != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(firstButton);
            }
        }

        public void SaveGame()
        {
            Debug.Log("[PauseMenu] Guardando partida...");
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.SaveCurrentGameState();
            }
            else
            {
                Debug.LogError("[PauseMenu] No se encontró SaveManager en la escena.");
            }
        }

        public void LoadMenu()
        {
            Time.timeScale = 1f;
            GameIsPaused = false;
            // Asegúrate de cambiar "MainMenu" por el nombre exacto de la escena de tu menú principal
            SceneManager.LoadScene("MainMenu"); 
        }

        public void QuitGame()
        {
            Debug.Log("[PauseMenu] Saliendo del juego...");
            Application.Quit();
        }
    }
}
