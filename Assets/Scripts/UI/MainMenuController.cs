using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

namespace HeartQuest.UI
{
    /// <summary>
    /// Controlador principal del Menú Cyberpunk de HeartQuest.
    /// Maneja navegación por teclado/mouse, transiciones entre paneles,
    /// y el fade-in inicial de la interfaz.
    /// </summary>
    public class MenuController : MonoBehaviour
    {
        [Header("── Escenas ──")]
        [Tooltip("Nombre de la escena que se carga al presionar Continuar o Nuevo Juego")]
        [SerializeField] private string sceneToLoad = "Escuela";

        [Header("── Paneles Principales ──")]
        public GameObject leftMenu;
        public GameObject rightPanel;
        public GameObject centerVisual;
        public GameObject topBar;
        public GameObject bottomDialogue;

        [Header("── Fade In ──")]
        public CanvasGroup fadeOverlay;
        public float fadeDuration = 1.5f;

        [Header("── Navegación por Teclado ──")]
        public List<Button> menuButtons = new List<Button>();
        public int currentIndex = 0;

        private void Start()
        {
            // Inicia con fade-in si hay overlay
            if (fadeOverlay != null)
            {
                StartCoroutine(FadeIn());
            }

            // Seleccionar el primer botón
            if (menuButtons.Count > 0)
            {
                SelectButton(0);
            }
        }

        private void Update()
        {
            HandleKeyboardNavigation();
        }

        /// <summary>
        /// Navegación con flechas del teclado y Enter/Space para confirmar.
        /// </summary>
        private void HandleKeyboardNavigation()
        {
            if (menuButtons.Count == 0) return;

            // NO procesar input si el menú no está visible
            if (!gameObject.activeInHierarchy) return;
            // Verificar que el panel padre esté activo también
            Transform p = transform;
            while (p != null)
            {
                if (!p.gameObject.activeSelf) return;
                p = p.parent;
            }

            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                currentIndex = (currentIndex + 1) % menuButtons.Count;
                SelectButton(currentIndex);
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                currentIndex = (currentIndex - 1 + menuButtons.Count) % menuButtons.Count;
                SelectButton(currentIndex);
            }
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            {
                if (menuButtons[currentIndex] != null)
                {
                    menuButtons[currentIndex].onClick.Invoke();
                }
            }
        }

        private void SelectButton(int index)
        {
            if (index >= 0 && index < menuButtons.Count && menuButtons[index] != null)
            {
                EventSystem.current.SetSelectedGameObject(menuButtons[index].gameObject);
            }
        }

        // ═══════════════════════════════════════
        // MÉTODOS PÚBLICOS PARA BOTONES
        // ═══════════════════════════════════════

        /// <summary>
        /// Continuar partida / Nuevo Juego → carga la escena del juego.
        /// </summary>
        public void OnContinueGame()
        {
            Debug.Log($"[HeartQuest] Cargando escena: {sceneToLoad}");
            SceneManager.LoadScene(sceneToLoad);
        }

        /// <summary>
        /// Nuevo Juego → misma funcionalidad por ahora, expansible.
        /// </summary>
        public void OnNewGame()
        {
            Debug.Log("[HeartQuest] Iniciando Nuevo Juego...");
            SceneManager.LoadScene(sceneToLoad);
        }

        /// <summary>
        /// Cargar Partida → placeholder, muestra mensaje.
        /// </summary>
        public void OnLoadGame()
        {
            Debug.Log("[HeartQuest] Sistema de guardado no implementado aún.");
        }

        /// <summary>
        /// Opciones → placeholder.
        /// </summary>
        public void OnOptions()
        {
            Debug.Log("[HeartQuest] Menú de opciones no implementado aún.");
        }

        /// <summary>
        /// Extras → placeholder.
        /// </summary>
        public void OnExtras()
        {
            Debug.Log("[HeartQuest] Extras no implementado aún.");
        }

        /// <summary>
        /// Salir del juego.
        /// </summary>
        public void OnExitGame()
        {
            Debug.Log("[HeartQuest] Saliendo...");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // ═══════════════════════════════════════
        // CORRUTINAS
        // ═══════════════════════════════════════

        /// <summary>
        /// Fade in suave al inicio del menú.
        /// </summary>
        private IEnumerator FadeIn()
        {
            fadeOverlay.alpha = 1f;
            fadeOverlay.blocksRaycasts = true;

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                fadeOverlay.alpha = 1f - (elapsed / fadeDuration);
                yield return null;
            }

            fadeOverlay.alpha = 0f;
            fadeOverlay.blocksRaycasts = false;
        }
    }
}
