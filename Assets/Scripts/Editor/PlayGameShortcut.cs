using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Crea un gran botón en el menú para que el usuario no tenga que 
/// buscar la escena principal y pueda probar el juego completo de un solo clic.
/// </summary>
public class PlayGameShortcut : Editor
{
    // Ruta exacta donde se encuentra el salón de clases
    private const string MAIN_SCENE_PATH = "Assets/Examples/Scenes/main scene.unity";

    // [MenuItem("POO Game/▶ JUGAR JUEGO COMPLETO", false, 0)]
    public static void PlayFullGame()
    {
        if (EditorApplication.isPlaying) { EditorApplication.isPlaying = false; return; }
        
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        string targetScenePath = "Assets/Scenes/MainMenu.unity";

        if (!System.IO.File.Exists(targetScenePath))
        {
            Debug.LogError("No se encontró el menú principal. Por favor, usa el nuevo botón '🎮 BUILD COMPLETE GAME (ENTREGA FINAL)'.");
            return;
        }

        EditorSceneManager.OpenScene(targetScenePath);
        Debug.Log("✅ Cargando el Menú Principal...");
        EditorApplication.isPlaying = true;
    }
}
