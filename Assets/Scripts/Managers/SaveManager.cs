using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using AntiBullyingGame.Core;
using AntiBullyingGame.RPG;

namespace AntiBullyingGame.Managers
{
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        private string saveFilePath;
        public bool loadOnSceneLoad = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                
                // Define la ruta del archivo. Application.persistentDataPath es un directorio seguro.
                saveFilePath = Path.Combine(Application.persistentDataPath, "save.json");
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (loadOnSceneLoad)
            {
                LoadCurrentGameState();
                loadOnSceneLoad = false; // Reset the flag after loading
            }
        }

        #region CRUD Operations

        // CREATE / UPDATE
        public void SaveGame(SaveData data)
        {
            try
            {
                string json = JsonUtility.ToJson(data, true); // true para formato bonito/legible
                File.WriteAllText(saveFilePath, json);
                Debug.Log($"[SaveManager] Partida guardada exitosamente en: {saveFilePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SaveManager] Error al guardar la partida: {e.Message}");
            }
        }

        // READ
        public SaveData LoadGame()
        {
            if (HasSaveFile())
            {
                try
                {
                    string json = File.ReadAllText(saveFilePath);
                    SaveData data = JsonUtility.FromJson<SaveData>(json);
                    Debug.Log("[SaveManager] Partida cargada exitosamente.");
                    return data;
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[SaveManager] Error al cargar la partida: {e.Message}");
                    return null;
                }
            }

            Debug.LogWarning("[SaveManager] No se encontró ningún archivo de guardado.");
            return null;
        }

        // DELETE
        public void DeleteSave()
        {
            if (HasSaveFile())
            {
                try
                {
                    File.Delete(saveFilePath);
                    Debug.Log("[SaveManager] Partida eliminada.");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[SaveManager] Error al eliminar la partida: {e.Message}");
                }
            }
        }

        // VERIFY
        public bool HasSaveFile()
        {
            return File.Exists(saveFilePath);
        }

        #endregion

        #region Game Integration

        // Método para ser llamado desde un menú o botón (Guarda el estado actual)
        public void SaveCurrentGameState()
        {
            SaveData data = new SaveData();

            // 1. Obtener Vida
            if (PlayerVars.instance != null)
            {
                data.health = PlayerVars.instance.health;
            }

            // 2. Obtener Posición y Moral
            Player player = FindAnyObjectByType<Player>();
            if (player != null)
            {
                data.position[0] = player.transform.position.x;
                data.position[1] = player.transform.position.y;
                data.position[2] = player.transform.position.z;
                
                data.morale = player.Morale;
            }
            else
            {
                Debug.LogWarning("[SaveManager] No se encontró al jugador en la escena actual para guardar su estado.");
            }

            SaveGame(data);
        }

        // Método para ser llamado desde el botón "Cargar Partida" (Carga el estado actual)
        public void LoadCurrentGameState()
        {
            SaveData data = LoadGame();

            if (data != null)
            {
                // 1. Aplicar Vida
                if (PlayerVars.instance != null)
                {
                    // Asignamos directamente la vida. Se sugiere crear un método SetHealth si es necesario encapsular.
                    PlayerVars.instance.health = data.health;
                }

                // 2. Aplicar Posición y Moral
                Player player = FindAnyObjectByType<Player>();
                if (player != null)
                {
                    player.transform.position = new Vector3(data.position[0], data.position[1], data.position[2]);
                    
                    // Ajustar la moral.
                    player.SetMorale(data.morale);
                    
                    Debug.Log("[SaveManager] Estado del jugador aplicado exitosamente.");
                }
                else
                {
                    Debug.LogWarning("[SaveManager] No se encontró al jugador en la escena para aplicar el estado.");
                }
            }
        }

        #endregion
    }
}
