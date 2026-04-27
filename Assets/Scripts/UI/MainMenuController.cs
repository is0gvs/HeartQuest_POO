using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AntiBullyingGame.UI
{
    public class MainMenuController : MonoBehaviour
    {
        public GameObject mainPanel;
        public GameObject optionsPanel;
        public Slider volumeSlider;
        public Toggle fullscreenToggle;
        public string sceneToLoad = "ClassroomScene";

        private void Start()
        {
            // Sincronizar los controles UI con los valores actuales al iniciar
            if (volumeSlider != null)
            {
                volumeSlider.value = AudioListener.volume;
                volumeSlider.onValueChanged.AddListener(SetVolume);
            }

            if (fullscreenToggle != null)
            {
                fullscreenToggle.isOn = Screen.fullScreen;
                fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
            }
        }

        public void PlayGame()
        {
            SceneManager.LoadScene(sceneToLoad);
        }

        public void StartGame()
        {
            Debug.Log("Start pressed...");
            // You can load a scene or open a sub-menu here
            SceneManager.LoadScene(sceneToLoad);
        }

        public void ContinueGame()
        {
            Debug.Log("Continuar juego...");
        }

        public void NewGame()
        {
            Debug.Log("Nuevo juego...");
            SceneManager.LoadScene(sceneToLoad);
        }

        public void LoadGame()
        {
            Debug.Log("Cargar juego...");
        }

        public void ShowOptions()
        {
            if (mainPanel != null) mainPanel.SetActive(false);
            if (optionsPanel != null) optionsPanel.SetActive(true);
        }

        public void ShowMainPanel()
        {
            if (optionsPanel != null) optionsPanel.SetActive(false);
            if (mainPanel != null) mainPanel.SetActive(true);
        }

        public void SetVolume(float volume)
        {
            AudioListener.volume = volume;
        }

        public void SetFullscreen(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
