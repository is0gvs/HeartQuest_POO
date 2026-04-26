using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

namespace HeartQuest.UI
{
    /// <summary>
    /// Efectos visuales para botones del menú cyberpunk.
    /// Añade brillo neón al hacer hover, animación de escala,
    /// y transiciones de color suaves.
    /// Se activa tanto con mouse (Pointer) como con teclado (Select).
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class UIButtonEffects : MonoBehaviour, 
        IPointerEnterHandler, IPointerExitHandler, 
        ISelectHandler, IDeselectHandler,
        IPointerClickHandler
    {
        [Header("── Colores ──")]
        [SerializeField] private Color normalColor = new Color(0.071f, 0.098f, 0.169f, 0.9f);       // #121A2B
        [SerializeField] private Color hoverColor = new Color(0.616f, 0.302f, 1f, 1f);               // #9D4DFF
        [SerializeField] private Color glowColor = new Color(0f, 0.898f, 1f, 1f);                    // #00E5FF

        [Header("── Escala ──")]
        [SerializeField] private float hoverScale = 1.05f;
        [SerializeField] private float clickScale = 0.95f;
        [SerializeField] private float scaleSpeed = 8f;

        [Header("── Brillo Neón ──")]
        [SerializeField] private float glowIntensity = 2f;
        [SerializeField] private float glowSpeed = 6f;

        // Componentes internos
        private Image buttonImage;
        private Image glowBorder;
        private RectTransform rectTransform;
        private Vector3 originalScale;
        private Vector3 targetScale;
        private Color targetColor;
        private bool isHovered = false;
        private Coroutine clickPulseCoroutine;

        private void Awake()
        {
            buttonImage = GetComponent<Image>();
            rectTransform = GetComponent<RectTransform>();
            originalScale = rectTransform.localScale;
            targetScale = originalScale;
            targetColor = normalColor;

            // Buscar el hijo "GlowBorder" si existe
            Transform glowChild = transform.Find("GlowBorder");
            if (glowChild != null)
            {
                glowBorder = glowChild.GetComponent<Image>();
                if (glowBorder != null)
                {
                    Color c = glowBorder.color;
                    c.a = 0f;
                    glowBorder.color = c;
                }
            }
        }

        private void Update()
        {
            // Interpolación suave de escala
            rectTransform.localScale = Vector3.Lerp(
                rectTransform.localScale, 
                targetScale, 
                Time.deltaTime * scaleSpeed
            );

            // Interpolación suave de color del botón
            if (buttonImage != null)
            {
                buttonImage.color = Color.Lerp(
                    buttonImage.color, 
                    targetColor, 
                    Time.deltaTime * glowSpeed
                );
            }

            // Animación del borde de brillo
            if (glowBorder != null && isHovered)
            {
                float pulse = (Mathf.Sin(Time.time * glowSpeed) + 1f) * 0.5f;
                Color c = glowColor;
                c.a = Mathf.Lerp(0.3f, 0.8f, pulse);
                glowBorder.color = c;
            }
        }

        // ═══════════════════════════════════════
        // EVENTOS DE MOUSE
        // ═══════════════════════════════════════

        public void OnPointerEnter(PointerEventData eventData)
        {
            ActivateHover();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            DeactivateHover();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            TriggerClickPulse();
        }

        // ═══════════════════════════════════════
        // EVENTOS DE TECLADO (Navegación)
        // ═══════════════════════════════════════

        public void OnSelect(BaseEventData eventData)
        {
            ActivateHover();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            DeactivateHover();
        }

        // ═══════════════════════════════════════
        // LÓGICA INTERNA
        // ═══════════════════════════════════════

        private void ActivateHover()
        {
            isHovered = true;
            targetScale = originalScale * hoverScale;
            targetColor = hoverColor;
        }

        private void DeactivateHover()
        {
            isHovered = false;
            targetScale = originalScale;
            targetColor = normalColor;

            // Apagar el brillo del borde
            if (glowBorder != null)
            {
                Color c = glowBorder.color;
                c.a = 0f;
                glowBorder.color = c;
            }
        }

        /// <summary>
        /// Pulso rápido de escala al hacer clic.
        /// </summary>
        private void TriggerClickPulse()
        {
            if (clickPulseCoroutine != null)
                StopCoroutine(clickPulseCoroutine);
            clickPulseCoroutine = StartCoroutine(ClickPulseRoutine());
        }

        private IEnumerator ClickPulseRoutine()
        {
            targetScale = originalScale * clickScale;
            yield return new WaitForSeconds(0.08f);
            targetScale = originalScale * hoverScale;
        }
    }
}
