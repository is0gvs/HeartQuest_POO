using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace AntiBullyingGame.EditorTools
{
    public class InventoryUISetup : Editor
    {
        [MenuItem("Tools/HeartQuest/Configurar UI de Inventario")]
        public static void SetupInventoryUI()
        {
            // 1. Buscar el InventoryManager en la escena actual
            InventoryManager inventoryManager = FindObjectOfType<InventoryManager>();
            if (inventoryManager == null)
            {
                // Si no existe, creamos un objeto para él
                GameObject managerObj = new GameObject("InventoryManager");
                inventoryManager = managerObj.AddComponent<InventoryManager>();
                Debug.Log("InventoryManager no encontrado. Se ha creado uno nuevo.");
            }

            // 2. Buscar o crear un Canvas
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasObj.AddComponent<GraphicRaycaster>();
                Debug.Log("Canvas creado.");
            }

            // Asegurar que exista un EventSystem
            EventSystem eventSystem = FindObjectOfType<EventSystem>();
            if (eventSystem == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                esObj.AddComponent<EventSystem>();
                esObj.AddComponent<StandaloneInputModule>();
            }

            // 3. Crear el Panel de Inventario (si no existe ya en el InventoryManager)
            if (inventoryManager.panelInventario == null)
            {
                GameObject panelObj = new GameObject("PanelInventario");
                panelObj.transform.SetParent(canvas.transform, false);

                // Configurar el componente Image para que sea un panel con color
                Image panelImage = panelObj.AddComponent<Image>();
                panelImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f); // Color oscuro semitransparente

                // Configurar RectTransform para que esté centrado
                RectTransform rect = panelObj.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.2f, 0.2f);
                rect.anchorMax = new Vector2(0.8f, 0.8f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;

                // Añadir un texto de título básico
                GameObject titleObj = new GameObject("TituloInventario");
                titleObj.transform.SetParent(panelObj.transform, false);
                Text titleText = titleObj.AddComponent<Text>();
                titleText.text = "INVENTARIO";
                titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                titleText.fontSize = 32;
                titleText.alignment = TextAnchor.UpperCenter;
                titleText.color = Color.white;
                
                RectTransform titleRect = titleObj.GetComponent<RectTransform>();
                titleRect.anchorMin = new Vector2(0, 1);
                titleRect.anchorMax = new Vector2(1, 1);
                titleRect.pivot = new Vector2(0.5f, 1);
                titleRect.offsetMin = new Vector2(0, -50);
                titleRect.offsetMax = new Vector2(0, 0);

                // 4. Asignarlo al manager
                inventoryManager.panelInventario = panelObj;
                Debug.Log("PanelInventario creado y asignado al InventoryManager exitosamente.");

                // Ocultarlo por defecto en el editor
                panelObj.SetActive(false);
            }
            else
            {
                Debug.Log("El InventoryManager ya tenía un Panel de Inventario asignado.");
            }

            // Marcar la escena como modificada para que Unity guarde los cambios
            if (!Application.isPlaying)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                    UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            }

            Debug.Log("¡Configuración de UI de Inventario completada!");
        }
    }
}
