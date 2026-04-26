using UnityEngine;
using UnityEngine.UI;

namespace HeartQuest.UI
{
    /// <summary>
    /// Efecto de parpadeo/brillo sutil para paneles cyberpunk.
    /// Simula el efecto de luz neón parpadeante en los bordes de los paneles,
    /// dando la sensación de que la interfaz está "viva".
    /// </summary>
    public class NeonPanelFlicker : MonoBehaviour
    {
        [Header("── Configuración de Flicker ──")]
        [SerializeField] private float flickerSpeed = 3f;
        [SerializeField] private float minAlpha = 0.6f;
        [SerializeField] private float maxAlpha = 1.0f;
        [SerializeField] private bool useRandomFlicker = true;
        [SerializeField] private float randomFlickerChance = 0.02f;

        [Header("── Color Base ──")]
        [SerializeField] private Color baseColor = new Color(0.616f, 0.302f, 1f, 1f); // #9D4DFF

        private Image targetImage;
        private Outline outline;
        private float flickerTimer = 0f;

        private void Awake()
        {
            targetImage = GetComponent<Image>();
            outline = GetComponent<Outline>();
        }

        private void Update()
        {
            if (outline != null)
            {
                AnimateOutlineGlow();
            }
            else if (targetImage != null)
            {
                AnimateImageGlow();
            }
        }

        /// <summary>
        /// Anima el brillo del Outline del panel.
        /// </summary>
        private void AnimateOutlineGlow()
        {
            flickerTimer += Time.deltaTime * flickerSpeed;
            float t = (Mathf.Sin(flickerTimer) + 1f) * 0.5f;
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);

            // Flicker aleatorio para efecto cyberpunk
            if (useRandomFlicker && Random.value < randomFlickerChance)
            {
                alpha *= Random.Range(0.3f, 0.7f);
            }

            Color c = baseColor;
            c.a = alpha;
            outline.effectColor = c;
        }

        /// <summary>
        /// Anima la opacidad de la imagen para un brillo suave.
        /// </summary>
        private void AnimateImageGlow()
        {
            flickerTimer += Time.deltaTime * flickerSpeed;
            float t = (Mathf.Sin(flickerTimer) + 1f) * 0.5f;
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);

            Color c = targetImage.color;
            c.a = alpha;
            targetImage.color = c;
        }
    }
}
