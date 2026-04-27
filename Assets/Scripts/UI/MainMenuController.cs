using UnityEngine;
using UnityEngine.SceneManagement;

namespace AntiBullyingGame.UI
{
    public class MainMenuController : MonoBehaviour
    {
        public GameObject mainPanel;
        public GameObject optionsPanel;
        public string sceneToLoad = "ClassroomScene";

        public void PlayGame()
        {
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
