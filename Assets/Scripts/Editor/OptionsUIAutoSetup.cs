using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using AntiBullyingGame.UI;

namespace AntiBullyingGame.Editor
{
    public class OptionsUIAutoSetup : EditorWindow
    {
        [MenuItem("HeartQuest/Fix Options UI (Crear Slider y Toggle)")]
        public static void SetupOptionsUI()
        {
            MainMenuController controller = FindObjectOfType<MainMenuController>();
            if (controller == null)
            {
                EditorUtility.DisplayDialog("Error", "No se encontró MainMenuController en la escena. Asegúrate de estar en la escena del menú principal.", "OK");
                return;
            }

            GameObject optionsPanel = controller.optionsPanel;
            if (optionsPanel == null)
            {
                EditorUtility.DisplayDialog("Error", "El Options Panel no está asignado en el MainMenuController.", "OK");
                return;
            }

            // Recursos por defecto para que no se vean blancos o invisibles
            DefaultControls.Resources uiResources = new DefaultControls.Resources();
            uiResources.standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            uiResources.background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            uiResources.knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            uiResources.checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd");

            bool changed = false;

            // Create Slider
            if (controller.volumeSlider == null)
            {
                GameObject sliderObj = DefaultControls.CreateSlider(uiResources);
                sliderObj.transform.SetParent(optionsPanel.transform, false);
                sliderObj.name = "Volume Slider";
                
                RectTransform rt = sliderObj.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(0, 50); 
                rt.sizeDelta = new Vector2(160, 20);

                // Add a text label to the slider
                GameObject labelObj = new GameObject("Label");
                labelObj.transform.SetParent(sliderObj.transform, false);
                Text label = labelObj.AddComponent<Text>();
                label.text = "Volumen";
                label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                label.color = Color.black;
                label.alignment = TextAnchor.MiddleCenter;
                RectTransform labelRt = labelObj.GetComponent<RectTransform>();
                labelRt.anchoredPosition = new Vector2(0, 20); // Above the slider
                labelRt.sizeDelta = new Vector2(160, 30);

                controller.volumeSlider = sliderObj.GetComponent<Slider>();
                changed = true;
            }

            // Create Toggle
            if (controller.fullscreenToggle == null)
            {
                GameObject toggleObj = DefaultControls.CreateToggle(uiResources);
                toggleObj.transform.SetParent(optionsPanel.transform, false);
                toggleObj.name = "Fullscreen Toggle";
                
                RectTransform rt = toggleObj.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(0, -50);

                // Modify the default label
                Text label = toggleObj.GetComponentInChildren<Text>();
                if (label != null)
                {
                    label.text = "Pantalla Completa";
                }

                controller.fullscreenToggle = toggleObj.GetComponent<Toggle>();
                changed = true;
            }

            if (changed)
            {
                EditorUtility.SetDirty(controller);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
                EditorUtility.DisplayDialog("Éxito", "¡Slider y Toggle creados y asignados correctamente en el panel de opciones!\n\nRevisa tu OptionsPanel en la jerarquía.", "¡Genial!");
            }
            else
            {
                EditorUtility.DisplayDialog("Aviso", "El Slider y el Toggle ya están asignados. No se creó nada nuevo.", "OK");
            }
        }
    }
}
