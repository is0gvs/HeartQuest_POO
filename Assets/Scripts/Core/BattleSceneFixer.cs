using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleSceneFixer : MonoBehaviour
{
    void Awake()
    {
        // Forzar fondo negro en la cámara para evitar la pantalla blanca
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.clearFlags = CameraClearFlags.SolidColor;
            mainCam.backgroundColor = Color.black;
        }

        // Asegurarse de que el tiempo corra (por si acaso el menú anterior lo pausó)
        Time.timeScale = 1f;

        Debug.Log("Escena de batalla reparada: Cámara en negro y tiempo reanudado.");
    }
}
