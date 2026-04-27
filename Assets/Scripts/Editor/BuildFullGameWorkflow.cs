using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Orquestador Maestro: Construye el menú principal, la escena del aula y la batalla,
/// los vincula, los añade al Build Settings y deja el juego listo para jugar
/// con un solo clic. ¡Entrega prolija y funcional garantizada!
/// </summary>
public class BuildFullGameWorkflow : Editor
{
    [MenuItem("POO Game/Build_Heartquest", false, -10)]
    public static void BuildEverything()
    {
        if (EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = false;
        }
        
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        
        if (!System.IO.Directory.Exists("Assets/Scenes"))
        {
            System.IO.Directory.CreateDirectory("Assets/Scenes");
        }

        // 1. Generar Menú Principal
        Debug.Log("<color=yellow>--- 1. CONSTRUYENDO MENÚ PRINCIPAL ---</color>");
        string menuPath = "Assets/Scenes/MainMenu.unity";
        var menuScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        MainMenuAutoSetup.SetupMainMenu();
        EditorSceneManager.SaveScene(menuScene, menuPath);

        Debug.Log("<color=yellow>--- 2. CONSTRUYENDO SALÓN DE CLASES ---</color>");
        var classScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        // Generar el salón y la chica de cabello azul con sus diálogos
        SceneAutoSetup.Setup(); 
        // Generar a Mateo (Bully) con su BattleTrigger
        AutomateBullySetup.AutoSetupBully(); 
        string classPath = "Assets/Scenes/ClassroomScene.unity";
        EditorSceneManager.SaveScene(classScene, classPath);

        Debug.Log("<color=yellow>--- 3. CONSTRUYENDO ESCENA DE BATALLA ---</color>");
        // El script de la batalla ya crea y guarda su propia escena
        BattleSceneSetup.Build(); 
        string battlePath = "Assets/Scenes/BattleScene.unity";

        Debug.Log("<color=yellow>--- 4. CONFIGURANDO BUILD SETTINGS ---</color>");
        // Esto es crucial para que los botones de "LoadScene" funcionen en el juego
        EditorBuildSettingsScene[] buildScenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene(menuPath, true),
            new EditorBuildSettingsScene(classPath, true),
            new EditorBuildSettingsScene(battlePath, true)
        };
        EditorBuildSettings.scenes = buildScenes;

        Debug.Log("<color=green>✅ ENTREGA FINAL GENERADA CON ÉXITO</color>");
        EditorUtility.DisplayDialog("Entrega Final Lista", "Se han generado el Menú Principal, el Salón de Clases (con todas las colisiones y NPCs) y la Escena de Batalla.\n\nTodo ha sido enlazado. ¡El juego está listo para jugar!", "Jugar Ahora");

        // Cargar menú principal y arrancar el juego
        EditorSceneManager.OpenScene(menuPath);
        EditorApplication.isPlaying = true;
    }
}
