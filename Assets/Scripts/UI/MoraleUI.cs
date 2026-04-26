using UnityEngine;
using TMPro;
using UnityEngine.UI;
using AntiBullyingGame.Core;

namespace HeartQuest.UI
{
    public class MoraleUI : MonoBehaviour, IObserver
    {
        public Slider moraleSlider;
        public TextMeshProUGUI moraleText;
        private GameManager gm;

        void Start()
        {
            gm = Object.FindAnyObjectByType<GameManager>();
            if (gm != null)
            {
                gm.Attach(this);
                OnMoraleUpdated(gm.CurrentMorale);
            }
        }

        void OnDestroy()
        {
            if (gm != null) gm.Detach(this);
        }

        public void OnMoraleUpdated(int currentMorale)
        {
            if (moraleSlider != null) moraleSlider.value = currentMorale;
            if (moraleText != null) moraleText.text = $"MORAL: {currentMorale}/100";
        }
    }
}
