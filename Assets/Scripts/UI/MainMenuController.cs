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
        
        [Header("Escenas")]
        [Tooltip("Escena que se cargará al dar clic en 'Nuevo Juego'")]
        public string newGameScene = "TutorialScene"; 
        [Tooltip("Escena de respaldo en caso de que el archivo de guardado no tenga escena")]
        public string fallbackScene = "ClassroomScene";

        [Header("Navegación con Mando / Teclado")]
        [Tooltip("Botón por defecto seleccionado en el panel principal")]
        public GameObject mainFirstButton;
        [Tooltip("Botón por defecto seleccionado en el panel de opciones")]
        public GameObject optionsFirstButton;
        [Tooltip("Botón por defecto seleccionado en el panel de carga")]
        public GameObject loadFirstButton;


        private const string VOLUME_KEY = "GameVolume";
        private const string FULLSCREEN_KEY = "Fullscreen";

        public void ApplySettingsToUI()
        {
            float savedVolume = PlayerPrefs.GetFloat(VOLUME_KEY, 1f);
            bool fullscreen = PlayerPrefs.GetInt(FULLSCREEN_KEY, 1) == 1;
            AudioListener.volume = savedVolume;
            Screen.fullScreen = fullscreen;

            if (volumeSlider != null)
                volumeSlider.SetValueWithoutNotify(savedVolume);

            if (fullscreenToggle != null)
                fullscreenToggle.SetIsOnWithoutNotify(fullscreen);
        }

        private void Start()
        {
            ApplySettingsToUI();

            // --- DIAGNÓSTICO DE MÚSICA EN MENÚ PRINCIPAL ---
            AudioSource[] allAudio = FindObjectsOfType<AudioSource>();
            Debug.Log($"[DIAGNÓSTICO MAIN MENU] Encontré {allAudio.Length} AudioSources.");
            foreach(var audio in allAudio) 
            {
                Debug.Log($"   -> AudioSource en: {audio.gameObject.name} | Sonando: {audio.isPlaying} | Volumen: {audio.volume} | Clip: {(audio.clip != null ? audio.clip.name : "NULO")}");
            }
            Debug.Log($"[DIAGNÓSTICO MAIN MENU] Volumen Global (AudioListener): {AudioListener.volume}");
            // ----------------------------------------------

            if (volumeSlider != null)
            {
                volumeSlider.onValueChanged.AddListener(SetVolume);
            }

            if (fullscreenToggle != null)
            {
                fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
            }

            SelectFirstSelectableInPanel(mainPanel, mainFirstButton);
        }

        public void SetVolume(float volume)
        {
            AudioListener.volume = volume;
            PlayerPrefs.SetFloat(VOLUME_KEY, volume);
            PlayerPrefs.Save();
            Debug.Log("[Settings] Volumen guardado: " + volume);
        }

        public void SetFullscreen(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
            PlayerPrefs.SetInt(FULLSCREEN_KEY, isFullscreen ? 1 : 0);
            PlayerPrefs.Save();
            Debug.Log("[Settings] Fullscreen guardado: " + isFullscreen);
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
                // Obtener la escena guardada
                string sceneToLoad = SaveManager.Instance.GetSavedSceneName();
                SaveManager.Instance.loadOnSceneLoad = true;
                SceneManager.LoadScene(sceneToLoad);
            }
            else
            {
                Debug.LogWarning("No hay archivo de guardado, iniciando juego nuevo...");
                SaveManager.Instance.loadOnSceneLoad = false;
                SceneManager.LoadScene(newGameScene);
            }
        }

        public void NewGame()
        {
            Debug.Log("Iniciando nuevo juego...");
            EnsureSaveManagerExists();
            SaveManager.Instance.CreateNewProfile();
            SaveManager.Instance.loadOnSceneLoad = false;
            // Al ser un nuevo juego, forzamos la escena del tutorial
            SceneManager.LoadScene(newGameScene);
        }

        public void LoadSpecificProfile(string profileName)
        {
            Debug.Log($"Cargando perfil específico: {profileName}");
            EnsureSaveManagerExists();
            
            // Establecer el perfil que queremos leer
            SaveManager.Instance.SetCurrentProfile(profileName);
            
            // Leer el nombre de la escena de ese perfil
            string sceneToLoad = SaveManager.Instance.GetSavedSceneName();
            
            SaveManager.Instance.loadOnSceneLoad = true;
            SceneManager.LoadScene(sceneToLoad);
        }

        public void LoadGame()
        {
            ShowLoadPanel();
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
                SelectFirstSelectableInPanel(loadPanel, loadFirstButton);
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

        public void ShowOptions()
        {
            if (mainPanel != null) mainPanel.SetActive(false);
            if (optionsPanel != null) optionsPanel.SetActive(true);
            SelectFirstSelectableInPanel(optionsPanel, optionsFirstButton);
        }

        public void ShowMainPanel()
        {
            if (optionsPanel != null) optionsPanel.SetActive(false);
            if (loadPanel != null) loadPanel.SetActive(false);
            if (mainPanel != null) mainPanel.SetActive(true);
            SelectFirstSelectableInPanel(mainPanel, mainFirstButton);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void Update()
        {
            if (UnityEngine.EventSystems.EventSystem.current != null && 
                UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == null)
            {
                if (Input.GetAxisRaw("Horizontal") != 0f || Input.GetAxisRaw("Vertical") != 0f ||
                    Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) ||
                    Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
                {
                    ReselectActivePanelFirstSelectable();
                }
            }
        }

        private void ReselectActivePanelFirstSelectable()
        {
            if (mainPanel != null && mainPanel.activeSelf)
            {
                SelectFirstSelectableInPanel(mainPanel, mainFirstButton);
            }
            else if (optionsPanel != null && optionsPanel.activeSelf)
            {
                SelectFirstSelectableInPanel(optionsPanel, optionsFirstButton);
            }
            else if (loadPanel != null && loadPanel.activeSelf)
            {
                SelectFirstSelectableInPanel(loadPanel, loadFirstButton);
            }
        }

        private void SelectFirstSelectableInPanel(GameObject panel, GameObject fallbackSelectable = null)
        {
            if (UnityEngine.EventSystems.EventSystem.current == null) return;

            GameObject toSelect = fallbackSelectable;
            if (toSelect == null && panel != null)
            {
                Selectable firstSel = panel.GetComponentInChildren<Selectable>(false);
                if (firstSel != null)
                {
                    toSelect = firstSel.gameObject;
                }
            }

            if (toSelect != null)
            {
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(toSelect);
            }
        }
    }
}