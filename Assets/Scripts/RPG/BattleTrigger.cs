using UnityEngine;
using UnityEngine.SceneManagement;

namespace AntiBullyingGame.RPG
{
    /// <summary>
    /// Puente entre el mundo de exploración y la escena de batalla estilo Undertale.
    /// Se adjunta al NPC Bully para iniciar el combate al interactuar con él.
    /// (POO: Principio de Responsabilidad Única - SRP)
    /// </summary>
    public class BattleTrigger : MonoBehaviour, AntiBullyingGame.Interfaces.IInteractable
    {
        [Header("Configuración de Batalla")]
        [Tooltip("Nombre de la escena de batalla a cargar (debe estar en Build Settings)")]
        public string battleSceneName = "BattleScene";

        [Tooltip("Nombre del enemigo que aparecerá en la batalla")]
        public string enemyName = "Mateo el Bully";

        [Tooltip("Si está activo, este NPC puede iniciar una batalla")]
        public bool canStartBattle = true;

        [Header("Diálogo previo a la batalla (Opcional)")]
        [Tooltip("Si se asigna, primero muestra este diálogo y LUEGO inicia la batalla")]
        public HeartQuest.Core.DialogueData preBattleDialogue;

        private bool battleStarted = false;

        public void Interact()
        {
            if (!canStartBattle || battleStarted) return;

            if (preBattleDialogue != null)
            {
                // Si hay diálogo previo, lo mostramos primero.
                // La batalla se inicia cuando el diálogo termina.
                var ds = Object.FindAnyObjectByType<HeartQuest.UI.DialogueSystem>(FindObjectsInactive.Include);
                if (ds != null)
                {
                    // Nos suscribimos al evento de fin de diálogo para lanzar la batalla
                    ds.StartDialogueStory(preBattleDialogue);
                    // Iniciamos la batalla con delay para esperar al diálogo
                    StartCoroutine(WaitForDialogueThenBattle(ds));
                    return;
                }
            }

            // Sin diálogo previo: iniciamos batalla directamente
            LaunchBattle();
        }

        private System.Collections.IEnumerator WaitForDialogueThenBattle(HeartQuest.UI.DialogueSystem ds)
        {
            // Esperamos hasta que el cuadro de diálogo se cierre
            yield return new WaitUntil(() => ds == null || !ds.gameObject.activeSelf);
            yield return new WaitForSeconds(0.3f); // Pequeño respiro
            LaunchBattle();
        }

        private void LaunchBattle()
        {
            if (battleStarted) return;
            battleStarted = true;

            // Guardamos el nombre del enemigo para que la escena de batalla lo use
            PlayerPrefs.SetString("CurrentEnemy", enemyName);
            // Guardamos la escena actual para poder regresar después
            PlayerPrefs.SetString("PreviousScene", SceneManager.GetActiveScene().name);
            PlayerPrefs.Save();

            Debug.Log($"[BattleTrigger] ¡Iniciando batalla contra {enemyName}!");

            // Cargamos la escena de batalla
            if (Application.CanStreamedLevelBeLoaded(battleSceneName))
            {
                SceneManager.LoadScene(battleSceneName);
            }
            else
            {
                Debug.LogError($"[BattleTrigger] La escena '{battleSceneName}' no existe en Build Settings. " +
                               $"Ve a File > Build Settings y agrégala.");
                battleStarted = false;
            }
        }
    }
}
