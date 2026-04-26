using UnityEngine;
using TMPro;
using UnityEngine.UI;
using AntiBullyingGame.Core;

namespace HeartQuest.UI
{
    public class PlayerStatsUI : MonoBehaviour, IObserver
    {
        [Header("Salud (HP)")]
        public Slider hpSlider;
        public TextMeshProUGUI hpText;

        [Header("Moral (MP)")]
        public Slider mpSlider;
        public TextMeshProUGUI mpText;

        [Header("Experiencia (XP)")]
        public Slider xpSlider;
        public TextMeshProUGUI xpText;

        private GameManager gm;

        void Start()
        {
            gm = Object.FindAnyObjectByType<GameManager>();
            if (gm != null)
            {
                gm.Attach(this);
                // Trigger initial update
                OnStatsUpdated(gm.CurrentHp, gm.MaxHp, gm.CurrentMorale, gm.MaxMorale, gm.CurrentXp, gm.MaxXp, gm.CurrentLevel);
            }
        }

        void OnDestroy()
        {
            if (gm != null) gm.Detach(this);
        }

        public void OnStatsUpdated(int hp, int maxHp, int morale, int maxMorale, int xp, int maxXp, int level)
        {
            if (hpSlider != null) { hpSlider.maxValue = maxHp; hpSlider.value = hp; }
            if (hpText != null) hpText.text = $"{hp}/{maxHp}";

            if (mpSlider != null) { mpSlider.maxValue = maxMorale; mpSlider.value = morale; }
            if (mpText != null) mpText.text = $"{morale}/{maxMorale}";

            if (xpSlider != null) { xpSlider.maxValue = maxXp; xpSlider.value = xp; }
            if (xpText != null) xpText.text = $"Lv {level}";
        }
    }
}
