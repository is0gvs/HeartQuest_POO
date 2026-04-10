using UnityEngine;
using UnityEngine.UI;

namespace AntiBullyingGame.RPG
{
    public class StartMenu : MonoBehaviour
    {
        public Button startButton;
        public GameObject startMenuPanel;

        void Start()
        {
            // Detener el tiempo del juego para que el jugador y las físicas no se muevan
            Time.timeScale = 0f;
            
            if (startButton != null)
            {
                startButton.onClick.AddListener(StartGame);
            }
        }

        void StartGame()
        {
            // Reanudar el tiempo del juego
            Time.timeScale = 1f;

            // Esconder este menú (el panel blanco y su botón)
            if (startMenuPanel != null)
            {
                startMenuPanel.SetActive(false);
            }
        }
    }
}
