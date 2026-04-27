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

        private string currentSaveFileName = "save_default.json";
        private string CurrentSaveFilePath => Path.Combine(Application.persistentDataPath, currentSaveFileName);
        
        public bool loadOnSceneLoad = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                
                SetMostRecentProfile();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void SetMostRecentProfile()
        {
            if (!Directory.Exists(Application.persistentDataPath)) return;

            string[] files = Directory.GetFiles(Application.persistentDataPath, "save_*.json");
            if (files.Length > 0)
            {
                // Ordenar por fecha de modificación descendente (el más reciente primero)
                System.Array.Sort(files, (a, b) => File.GetLastWriteTime(b).CompareTo(File.GetLastWriteTime(a)));
                currentSaveFileName = Path.GetFileName(files[0]);
                Debug.Log($"[SaveManager] Perfil más reciente encontrado: {currentSaveFileName}");
            }
            else
            {
                currentSaveFileName = "save_default.json";
                Debug.Log("[SaveManager] No se encontraron perfiles. Usando nombre por defecto.");
            }
        }

        public void CreateNewProfile()
        {
            currentSaveFileName = $"save_{System.DateTime.Now:yyyyMMdd_HHmmss}.json";
            Debug.Log($"[SaveManager] Nuevo perfil asignado: {currentSaveFileName}");
        }

        public string[] GetAllProfiles()
        {
            if (!Directory.Exists(Application.persistentDataPath)) return new string[0];
            string[] files = Directory.GetFiles(Application.persistentDataPath, "save_*.json");
            System.Array.Sort(files, (a, b) => File.GetLastWriteTime(b).CompareTo(File.GetLastWriteTime(a)));
            
            string[] fileNames = new string[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                fileNames[i] = Path.GetFileName(files[i]);
            }
            return fileNames;
        }

        public void SetCurrentProfile(string fileName)
        {
            currentSaveFileName = fileName;
            Debug.Log($"[SaveManager] Perfil activo cambiado a: {currentSaveFileName}");
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
                File.WriteAllText(CurrentSaveFilePath, json);
                Debug.Log($"[SaveManager] Partida guardada exitosamente en: {CurrentSaveFilePath}");
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
                    string json = File.ReadAllText(CurrentSaveFilePath);
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
                    File.Delete(CurrentSaveFilePath);
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
            return File.Exists(CurrentSaveFilePath);
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
            }

            // Guardar Moral desde el GameManager, que controla la UI
            GameManager gm = FindAnyObjectByType<GameManager>();
            if (gm != null) 
            {
                data.morale = gm.CurrentMorale;
            } 
            else if (player != null) 
            {
                data.morale = player.Morale;
            }
            else
            {
                Debug.LogWarning("[SaveManager] No se encontró al jugador ni al GameManager en la escena actual para guardar su estado.");
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
                    player.SetMorale(data.morale);
                }

                // Sincronizar con el GameManager para que la UI se actualice
                GameManager gm = FindAnyObjectByType<GameManager>();
                if (gm != null)
                {
                    gm.SetMorale(data.morale);
                }

                if (player != null || gm != null)
                {
                    Debug.Log("[SaveManager] Estado del jugador aplicado exitosamente.");
                }
                else
                {
                    Debug.LogWarning("[SaveManager] No se encontró al jugador ni al GameManager en la escena para aplicar el estado.");
                }
            }
        }

        #endregion
    }
}
