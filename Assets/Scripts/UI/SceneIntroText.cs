using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using AntiBullyingGame.Managers;

namespace AntiBullyingGame.UI 
{
    public class SceneIntroText : MonoBehaviour
    {
        [Header("Configuración de Texto")]
        [TextArea(3, 5)]
        public string introText = "Texto de introducción";
        public float fontSize = 60f;
        public Color textColor = Color.white;

        [Header("Configuración de Panel")]
        public Color panelColor = new Color(0f, 0f, 0f, 1f);

        [Header("Tiempos")]
        public float fadeDuration = 1.5f;
        public float displayDuration = 3f;

        // Referencias internas
        private Canvas _canvas;
        private Image _panel;
        private TextMeshProUGUI _label;

        private void Start()
        {
            string currentSceneName = SceneManager.GetActiveScene().name;

            // 1. Verificar si el SaveManager existe y si esta escena YA está en la lista de vistas
            if (SaveManager.Instance != null && SaveManager.Instance.escenasConIntroVista.Contains(currentSceneName))
            {
                // Si ya se vio la intro en esta escena, simplemente destruimos este script/objeto 
                // para que no construya la UI ni consuma recursos.
                Destroy(gameObject); 
                return;
            }

            // 2. Si es la PRIMERA VEZ, construimos la UI dinámica y arrancamos la animación
            BuildUI();
            StartCoroutine(IntroRoutine());

            // 3. Añadimos esta escena a la lista del SaveManager para que no se repita
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.escenasConIntroVista.Add(currentSceneName);
                
                // Guardamos el estado inmediatamente
                SaveManager.Instance.SaveCurrentGameState();
            }
        }

        // ── UI builder ────────────────────────────────────────────────────────
        
        private void BuildUI()
        {
            // Canvas — Screen Space Overlay para que quede encima de todo
            GameObject canvasGO = new GameObject("IntroTextCanvas");
            _canvas = canvasGO.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 999;
            canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGO.AddComponent<GraphicRaycaster>();
            DontDestroyOnLoad(canvasGO); // Sobrevive si la escena recarga durante el fade

            // Panel de fondo semitransparente
            GameObject panelGO = new GameObject("IntroPanel");
            panelGO.transform.SetParent(canvasGO.transform, false);
            _panel = panelGO.AddComponent<Image>();
            _panel.color = new Color(panelColor.r, panelColor.g, panelColor.b, 0f); // empieza invisible
            RectTransform panelRect = _panel.rectTransform;
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // Texto centrado
            GameObject textGO = new GameObject("IntroLabel");
            textGO.transform.SetParent(panelGO.transform, false);
            _label = textGO.AddComponent<TextMeshProUGUI>();
            _label.text = introText;
            _label.fontSize = fontSize;
            _label.alignment = TextAlignmentOptions.Center;
            _label.color = new Color(textColor.r, textColor.g, textColor.b, 0f); // empieza invisible
            _label.enableWordWrapping = true;
            _label.fontStyle = FontStyles.Italic;
            RectTransform textRect = _label.rectTransform;
            textRect.anchorMin = new Vector2(0.1f, 0.35f);
            textRect.anchorMax = new Vector2(0.9f, 0.65f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }

        // ── Coroutine ─────────────────────────────────────────────────────────
        
        private IEnumerator IntroRoutine()
        {
            // Fade IN
            yield return Fade(0f, 1f, fadeDuration);

            // Visible
            yield return new WaitForSecondsRealtime(displayDuration);

            // Fade OUT
            yield return Fade(1f, 0f, fadeDuration);

            // Limpiar
            if (_canvas != null)
                Destroy(_canvas.gameObject);
                
            Destroy(gameObject);
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                if (_panel != null)
                    _panel.color = new Color(panelColor.r, panelColor.g, panelColor.b, Mathf.Lerp(from, to, t) * panelColor.a);

                if (_label != null)
                    _label.color = new Color(textColor.r, textColor.g, textColor.b, Mathf.Lerp(from, to, t));

                yield return null;
            }
        }
    }
}