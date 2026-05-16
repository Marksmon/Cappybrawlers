using Capybrawlers.Core;
using Capybrawlers.Creatures;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Capybrawlers.UI
{
    public class BrawlerIntroSlice : MonoBehaviour
    {
        [SerializeField] private Image           _bg;
        [SerializeField] private Image           _creatureImage;
        [SerializeField] private TextMeshProUGUI _natureText;
        [SerializeField] private TextMeshProUGUI _statsText;

        public void Bind(ResolvedBuild build)
        {
            _bg.color        = NatureColor(build.Nature.natureType);
            _natureText.text = build.Nature.natureType.ToString().ToUpper();
            _statsText.text  = $"HP {build.FinalHP}\nATK {build.FinalATK}\nSPD {build.FinalSPD}";

            if (build.Nature.idleSprite != null)
            {
                _creatureImage.sprite  = build.Nature.idleSprite;
                _creatureImage.enabled = true;
            }
            else
            {
                _creatureImage.enabled = false;
            }
        }

        public static Color NatureColor(NatureType t) => t switch
        {
            NatureType.Flame => new Color(0.91f, 0.29f, 0.10f, 0.90f),
            NatureType.Storm => new Color(0.96f, 0.77f, 0.09f, 0.90f),
            NatureType.Plant => new Color(0.22f, 0.66f, 0.20f, 0.90f),
            NatureType.Rock  => new Color(0.44f, 0.44f, 0.44f, 0.90f),
            NatureType.Water => new Color(0.10f, 0.44f, 0.91f, 0.90f),
            NatureType.Moon  => new Color(0.61f, 0.25f, 0.78f, 0.90f),
            _                => new Color(0.30f, 0.30f, 0.30f, 0.90f),
        };
    }
}
