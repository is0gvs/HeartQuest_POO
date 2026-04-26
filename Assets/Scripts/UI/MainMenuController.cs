using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AntiBullyingGame.UI
{
    /// <summary>
    /// Controlador para la Interfaz del Menú Principal.
    /// Administra la navegación entre el menú principal, opciones y la carga de escenas.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Escena a Cargar")]
        [Tooltip("El nombre exacto de la escena que se cargará al dar click en Jugar (Ej: 'ClassroomScene')")]
        [SerializeField] private string sceneToLoad = "ClassroomScene";

        [Header("Paneles de Interfaz")]
        [Tooltip("Referencia al panel principal del menú")]
        [SerializeField] private GameObject mainPanel;
        
        [Tooltip("Referencia al panel de opciones o configuraciones")]
        [SerializeField] private GameObject optionsPanel;

        private void Start()
        {
            // Asegurarse de que al iniciar el menú, el panel principal esté activo y opciones esté apagado.
            ShowMainPanel();
        }

        /// <summary>
        /// Método llamado por el botón de Jugar.
        /// Carga la escena del juego principal.
        /// </summary>
        public void PlayGame()
        {
            Debug.Log($"Cargando escena: {sceneToLoad}");
            // Nota: ¡Asegúrate de haber añadido la escena a los Build Settings (File > Build Settings)!
            SceneManager.LoadScene(sceneToLoad);
        }

        /// <summary>
        /// Método llamado por el botón de Opciones.
        /// Oculta el panel principal y muestra el de opciones.
        /// </summary>
        public void ShowOptions()
        {
            if (mainPanel != null) mainPanel.SetActive(false);
            if (optionsPanel != null) optionsPanel.SetActive(true);
        }

        /// <summary>
        /// Método llamado por el botón de Volver (en el menú de opciones).
        /// Oculta el panel de opciones y muestra el principal.
        /// </summary>
        public void ShowMainPanel()
        {
            if (mainPanel != null) mainPanel.SetActive(true);
            if (optionsPanel != null) optionsPanel.SetActive(false);
        }

        /// <summary>
        /// Método llamado por el botón de Salir.
        /// Cierra la aplicación (solo funciona en builds exportadas).
        /// </summary>
        public void QuitGame()
        {
            Debug.Log("Saliendo de la aplicación...");
            // Si estamos en el editor de Unity, detenemos el modo Play
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            // Si es un build real (juego compilado), cierra la app
            Application.Quit();
#endif
        }
    }
}
