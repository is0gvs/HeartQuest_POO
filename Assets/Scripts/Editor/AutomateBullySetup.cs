using UnityEngine;
using UnityEditor;
using AntiBullyingGame.RPG;
using System.Linq;

/// <summary>
/// Automatiza la creación del Bully y le asigna el Battle Trigger
/// para que no tengas que hacerlo manualmente.
/// </summary>
public class AutomateBullySetup : Editor
{
    // [MenuItem("POO Game/Setup/3. Auto-Configurar Bully")] // Removido a petición del usuario
    public static void AutoSetupBully()
    {
        // 1. Buscamos si ya hay un Bully en la escena
        GameObject bullyObj = GameObject.Find("Mateo_Bully");
        
        // Si no existe, lo creamos
        if (bullyObj == null)
        {
            bullyObj = new GameObject("Mateo_Bully");
            bullyObj.transform.position = new Vector3(-2.5f, -0.5f, 0); // Lo ponemos a la izquierda de la clase
            
            // Le damos su imagen (el rubio)
            var sr = bullyObj.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 9;
            string spritePath = "Assets/Sprites/Characters/blonde_man.png";
            sr.sprite = AssetDatabase.LoadAllAssetsAtPath(spritePath).OfType<Sprite>().FirstOrDefault();

            // Le damos colisión para que el jugador pueda interactuar con él usando la 'E'
            var col = bullyObj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.8f, 0.8f);
            col.offset = new Vector2(0, 0.4f);
        }

        // 2. Le agregamos el BattleTrigger para que inicie la pelea
        if (bullyObj.GetComponent<BattleTrigger>() == null)
        {
            var trigger = bullyObj.AddComponent<BattleTrigger>();
            trigger.battleSceneName = "BattleScene";
            trigger.enemyName = "Mateo";
            trigger.canStartBattle = true;
        }

        // 3. Lo seleccionamos en el editor para que el usuario lo vea
        Selection.activeGameObject = bullyObj;
        
        // Guardar la escena
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        
        Debug.Log("✅ ¡Mateo (Bully) configurado automáticamente con BattleTrigger! Dale Play y presiona E cerca de él.");
    }
}
