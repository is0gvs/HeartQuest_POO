using UnityEngine;
using UnityEngine.UI;

namespace HeartQuest.UI
{
    /// <summary>
    /// Animación de latido/pulso para el corazón flotante del menú.
    /// Produce un efecto de "respiración" en escala y un brillo neón
    /// oscilante que alterna entre púrpura y cian.
    /// </summary>
    public class HeartPulse : MonoBehaviour
    {
        [Header("── Pulso de Escala ──")]
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float pulseMinScale = 0.9f;
        [SerializeField] private float pulseMaxScale = 1.1f;

        [Header("── Brillo Neón ──")]
        [SerializeField] private Color colorA = new Color(0.616f, 0.302f, 1f, 1f);   // Púrpura #9D4DFF
        [SerializeField] private Color colorB = new Color(0f, 0.898f, 1f, 1f);        // Cian #00E5FF
        [SerializeField] private float colorSpeed = 1.5f;

        [Header("── Flotación ──")]
        [SerializeField] private float floatAmplitude = 8f;
        [SerializeField] private float floatSpeed = 1.2f;

        // Componentes internos
        private RectTransform rectTransform;
        private Image heartImage;
        private Image glowImage;
        private Vector3 originalScale;
        private Vector2 originalPosition;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            heartImage = GetComponent<Image>();
            originalScale = rectTransform.localScale;
            originalPosition = rectTransform.anchoredPosition;

            // Buscar una imagen hija llamada "Glow" para el halo
            Transform glowChild = transform.Find("Glow");
            if (glowChild != null)
            {
                glowImage = glowChild.GetComponent<Image>();
            }
        }

        private void Update()
        {
            AnimatePulse();
            AnimateGlow();
            AnimateFloat();
        }

        /// <summary>
        /// Escala el corazón como un latido suave.
        /// </summary>
        private void AnimatePulse()
        {
            float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            float scale = Mathf.Lerp(pulseMinScale, pulseMaxScale, t);
            rectTransform.localScale = originalScale * scale;
        }

        /// <summary>
        /// Oscila el color del corazón entre púrpura y cian neón.
        /// </summary>
        private void AnimateGlow()
        {
            float t = (Mathf.Sin(Time.time * colorSpeed) + 1f) * 0.5f;
            Color currentColor = Color.Lerp(colorA, colorB, t);

            if (heartImage != null)
            {
                heartImage.color = currentColor;
            }

            // El halo de glow pulsa en opacidad
            if (glowImage != null)
            {
                Color g = currentColor;
                float glowAlpha = Mathf.Lerp(0.2f, 0.6f, t);
                g.a = glowAlpha;
                glowImage.color = g;
            }
        }

        /// <summary>
        /// Movimiento de flotación vertical suave.
        /// </summary>
        private void AnimateFloat()
        {
            float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            rectTransform.anchoredPosition = new Vector2(
                originalPosition.x,
                originalPosition.y + yOffset
            );
        }
    }
}
