using Capybrawlers.Core;
using Capybrawlers.Creatures;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Capybrawlers.Collection
{
    public class FormationSlotView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _slotLabel;
        [SerializeField] private TextMeshProUGUI _brawlerName;
        [SerializeField] private Image           _natureIcon;
        [SerializeField] private GameObject      _emptyState;
        [SerializeField] private GameObject      _filledState;

        public void ShowEmpty(FormationSlot slot)
        {
            _slotLabel.text = SlotLabel(slot);
            _emptyState.SetActive(true);
            _filledState.SetActive(false);
        }

        public void ShowBrawler(FormationSlot slot, ResolvedBuild build)
        {
            _slotLabel.text   = SlotLabel(slot);
            _brawlerName.text = build.Nature.natureType.ToString();
            if (build.Nature.idleSprite != null)
                _natureIcon.sprite = build.Nature.idleSprite;
            _emptyState.SetActive(false);
            _filledState.SetActive(true);
        }

        private static string SlotLabel(FormationSlot slot) => slot switch
        {
            FormationSlot.Frontline     => "Frontline",
            FormationSlot.BacklineLeft  => "Backline L",
            FormationSlot.BacklineRight => "Backline R",
            _                           => slot.ToString(),
        };
    }
}
