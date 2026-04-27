using UnityEngine;
using UnityEngine.SceneManagement;
using AntiBullyingGame.Managers;

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

        private void EnsureSaveManagerExists()
        {
            if (SaveManager.Instance == null)
            {
                new GameObject("SaveManager").AddComponent<SaveManager>();
            }
        }

        public void ContinueGame()
        {
            EnsureSaveManagerExists();
            if (SaveManager.Instance.HasSaveFile())
            {
                Debug.Log("Continuando desde partida guardada...");
                SaveManager.Instance.loadOnSceneLoad = true;
                SceneManager.LoadScene(sceneToLoad);
            }
            else
            {
                Debug.LogWarning("No hay archivo de guardado, iniciando juego nuevo...");
                SaveManager.Instance.loadOnSceneLoad = false;
                SceneManager.LoadScene(sceneToLoad);
            }
        }

        public void NewGame()
        {
            Debug.Log("Iniciando nuevo juego...");
            EnsureSaveManagerExists();
            SaveManager.Instance.CreateNewProfile();
            SaveManager.Instance.loadOnSceneLoad = false;
            SceneManager.LoadScene(sceneToLoad);
        }

        public void LoadGame()
        {
            Debug.Log("Cargando partida...");
            // Por ahora asume que carga el mismo archivo que continuar
            ContinueGame();
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
