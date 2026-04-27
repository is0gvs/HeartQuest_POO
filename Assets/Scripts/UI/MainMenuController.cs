using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using AntiBullyingGame.Managers;

namespace AntiBullyingGame.UI
{
    public class MainMenuController : MonoBehaviour
    {
        public GameObject mainPanel;
        public GameObject optionsPanel;
        public GameObject loadPanel;
        public Transform loadContentTransform;
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

        public void ShowLoadPanel()
        {
            if (mainPanel != null) mainPanel.SetActive(false);
            if (optionsPanel != null) optionsPanel.SetActive(false);
            
            if (loadPanel == null)
            {
                CreateRuntimeLoadPanel();
            }

            if (loadPanel != null) 
            {
                loadPanel.SetActive(true);
                PopulateLoadList();
            }
        }

        private void CreateRuntimeLoadPanel()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            loadPanel = new GameObject("RuntimeLoadPanel", typeof(RectTransform), typeof(Image));
            loadPanel.transform.SetParent(canvas.transform, false);
            
            RectTransform panelRT = loadPanel.GetComponent<RectTransform>();
            panelRT.anchorMin = Vector2.zero;
            panelRT.anchorMax = Vector2.one;
            panelRT.offsetMin = Vector2.zero;
            panelRT.offsetMax = Vector2.zero;
            
            Image img = loadPanel.GetComponent<Image>();
            img.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);
            
            // Título
            GameObject titleObj = new GameObject("Title", typeof(RectTransform), typeof(Text));
            titleObj.transform.SetParent(loadPanel.transform, false);
            Text titleTxt = titleObj.GetComponent<Text>();
            titleTxt.text = "CARGAR PARTIDA";
            titleTxt.fontSize = 80;
            titleTxt.fontStyle = FontStyle.Bold;
            titleTxt.alignment = TextAnchor.MiddleCenter;
            titleTxt.color = Color.white;
            titleTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            
            RectTransform titleRT = titleObj.GetComponent<RectTransform>();
            titleRT.sizeDelta = new Vector2(800, 100);
            titleRT.anchoredPosition = new Vector2(0, 350);

            // Botón Volver
            GameObject backBtnObj = new GameObject("BackBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            backBtnObj.transform.SetParent(loadPanel.transform, false);
            RectTransform backRT = backBtnObj.GetComponent<RectTransform>();
            backRT.sizeDelta = new Vector2(300, 80);
            backRT.anchoredPosition = new Vector2(0, -400);
            backBtnObj.GetComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f, 1f); 
            
            Button backBtn = backBtnObj.GetComponent<Button>();
            backBtn.onClick.AddListener(ShowMainPanel);
            
            GameObject backTxtObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
            backTxtObj.transform.SetParent(backBtnObj.transform, false);
            Text backTxt = backTxtObj.GetComponent<Text>();
            backTxt.text = "VOLVER";
            backTxt.fontSize = 30;
            backTxt.alignment = TextAnchor.MiddleCenter;
            backTxt.color = Color.white;
            backTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            
            RectTransform backTxtRT = backTxtObj.GetComponent<RectTransform>();
            backTxtRT.anchorMin = Vector2.zero;
            backTxtRT.anchorMax = Vector2.one;
            backTxtRT.offsetMin = Vector2.zero;
            backTxtRT.offsetMax = Vector2.zero;

            // Área de contenido
            GameObject contentObj = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup));
            contentObj.transform.SetParent(loadPanel.transform, false);
            loadContentTransform = contentObj.transform;
            
            RectTransform contentRT = contentObj.GetComponent<RectTransform>();
            contentRT.sizeDelta = new Vector2(600, 500);
            contentRT.anchoredPosition = new Vector2(0, 0);

            VerticalLayoutGroup vlg = contentObj.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 15;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlHeight = false;
            vlg.childControlWidth = false;
        }

        private void PopulateLoadList()
        {
            EnsureSaveManagerExists();

            // Limpiar lista actual
            if (loadContentTransform != null)
            {
                foreach (Transform child in loadContentTransform)
                {
                    Destroy(child.gameObject);
                }
            }

            string[] profiles = SaveManager.Instance.GetAllProfiles();

            if (profiles.Length == 0)
            {
                Debug.Log("No hay perfiles guardados.");
                return;
            }

            foreach (string profile in profiles)
            {
                GameObject btnObj = new GameObject($"Btn_{profile}", typeof(RectTransform), typeof(Image), typeof(Button));
                btnObj.transform.SetParent(loadContentTransform, false);
                
                RectTransform rt = btnObj.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(400, 60);

                Image img = btnObj.GetComponent<Image>();
                img.color = new Color(0.15f, 0.25f, 0.45f, 1f);

                GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
                textObj.transform.SetParent(btnObj.transform, false);
                Text txt = textObj.GetComponent<Text>();
                txt.text = profile.Replace(".json", "");
                txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                txt.fontSize = 24;
                txt.alignment = TextAnchor.MiddleCenter;
                txt.color = Color.white;

                RectTransform textRt = textObj.GetComponent<RectTransform>();
                textRt.anchorMin = Vector2.zero;
                textRt.anchorMax = Vector2.one;
                textRt.offsetMin = Vector2.zero;
                textRt.offsetMax = Vector2.zero;

                Button btn = btnObj.GetComponent<Button>();
                string profileToLoad = profile;
                btn.onClick.AddListener(() => LoadSpecificProfile(profileToLoad));
            }
        }

        public void LoadSpecificProfile(string profileName)
        {
            Debug.Log($"Cargando perfil específico: {profileName}");
            EnsureSaveManagerExists();
            SaveManager.Instance.SetCurrentProfile(profileName);
            SaveManager.Instance.loadOnSceneLoad = true;
            SceneManager.LoadScene(sceneToLoad);
        }

        public void LoadGame()
        {
            ShowLoadPanel();
        }

        public void ShowOptions()
        {
            if (mainPanel != null) mainPanel.SetActive(false);
            if (optionsPanel != null) optionsPanel.SetActive(true);
        }

        public void ShowMainPanel()
        {
            if (optionsPanel != null) optionsPanel.SetActive(false);
            if (loadPanel != null) loadPanel.SetActive(false);
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
