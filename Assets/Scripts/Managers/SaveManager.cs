using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using AntiBullyingGame.Core;
using AntiBullyingGame.RPG;
using System.Collections.Generic;

namespace AntiBullyingGame.Managers
{
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        private string currentSaveFileName = "save_default.json";
        private string CurrentSaveFilePath => Path.Combine(Application.persistentDataPath, currentSaveFileName);
        
        public bool loadOnSceneLoad = false;
        
        // Estado temporal en memoria
        public List<string> interactedNPCs = new List<string>();
        public List<string> escenasConIntroVista = new List<string>();

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
            
            interactedNPCs.Clear();
            escenasConIntroVista.Clear();
            PlayerPrefs.DeleteKey("BullyMateoResolved");
            PlayerPrefs.DeleteKey("ShouldRestoreBattleReturn");
            PlayerPrefs.DeleteKey("BattleReturnX");
            PlayerPrefs.DeleteKey("BattleReturnY");
            PlayerPrefs.DeleteKey("BattleReturnZ");
            PlayerPrefs.Save();
            
            Debug.Log($"[SaveManager] Nuevo perfil asignado: {currentSaveFileName}");

            InventoryManager inv = FindAnyObjectByType<InventoryManager>();
            if (inv != null)
            {
                inv.ResetInventory();
            }
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
                loadOnSceneLoad = false; 
            }
        }

        #region CRUD Operations

        public void SaveGame(SaveData data)
        {
            try
            {
                string json = JsonUtility.ToJson(data, true); 
                File.WriteAllText(CurrentSaveFilePath, json);
                Debug.Log($"[SaveManager] Partida guardada exitosamente en: {CurrentSaveFilePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SaveManager] Error al guardar la partida: {e.Message}");
            }
        }

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

        public bool HasSaveFile()
        {
            return File.Exists(CurrentSaveFilePath);
        }

        // --- NUEVO MÉTODO PARA LEER LA ESCENA ANTES DE CARGAR ---
        public string GetSavedSceneName()
        {
            SaveData data = LoadGame();
            if (data != null && !string.IsNullOrEmpty(data.sceneName))
            {
                return data.sceneName;
            }
            return "ClassroomScene"; // Escena por defecto si algo falla
        }

        #endregion

        #region Game Integration

        public void SaveCurrentGameState()
        {
            SaveData data = new SaveData();

            // 0. GUARDAR ESCENA ACTUAL
            data.sceneName = SceneManager.GetActiveScene().name;

            data.position = new float[3];

            // 1. VIDA
            if (PlayerVars.instance != null)
            {
                data.health = (int)PlayerVars.instance.health;
            }

            // 2. POSICIÓN
            Player player = FindAnyObjectByType<Player>();
            if (player != null)
            {
                data.position[0] = player.transform.position.x;
                data.position[1] = player.transform.position.y;
                data.position[2] = player.transform.position.z;
            }

            // 3. MORAL
            GameManager gm = FindAnyObjectByType<GameManager>();
            if (gm != null)
            {
                data.morale = gm.CurrentMorale;
            }
            else if (player != null)
            {
                data.morale = player.Morale;
            }

            // 4. INVENTARIO 
            InventoryManager inv = FindAnyObjectByType<InventoryManager>();
            if (inv != null)
            {
                data.inventory = inv.inventory.ToSaveData();
            }

            // 5. NPCs INTERACTUADOS
            data.interactedNPCs = new List<string>(this.interactedNPCs);

            // 6. ESCENAS Interactuadas
            data.escenasConIntroVista = new List<string>(this.escenasConIntroVista);

            // GUARDAR TODO
            SaveGame(data);
        }

        public void LoadCurrentGameState()
        {
            SaveData data = LoadGame();

            if (data != null)
            {
                if (PlayerVars.instance != null)
                {
                    PlayerVars.instance.health = data.health;
                }

                Player player = FindAnyObjectByType<Player>();
                if (player != null)
                {
                    if (data.position != null && data.position.Length >= 3)
                    {
                        player.transform.position = new Vector3(
                            data.position[0],
                            data.position[1],
                            data.position[2]
                        );
                    }
                    player.SetMorale(data.morale);
                }

                GameManager gm = FindAnyObjectByType<GameManager>();
                if (gm != null)
                {
                    gm.SetMorale(data.morale);
                }

                InventoryManager inv = FindAnyObjectByType<InventoryManager>();
                if (inv != null)
                {
                    inv.LoadInventory(data.inventory);
                }

                if (data.interactedNPCs != null)
                {
                    this.interactedNPCs = new List<string>(data.interactedNPCs);
                }
                else
                {
                    this.interactedNPCs.Clear();
                }

                if (data.escenasConIntroVista != null)
                {
                    this.escenasConIntroVista = new List<string>(data.escenasConIntroVista);
                }
                else
                {
                    this.escenasConIntroVista.Clear();
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
