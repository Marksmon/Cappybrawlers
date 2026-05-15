using Capybrawlers.Battle;
using Capybrawlers.Creatures;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Capybrawlers.UI
{
    public class BrawlerInfoPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI   _nameText;
        [SerializeField] private TextMeshProUGUI   _statsText;
        [SerializeField] private Button[]          _equipButtons;
        [SerializeField] private TextMeshProUGUI[] _equipNameLabels;
        [SerializeField] private Button            _closeButton;
        [SerializeField] private CardDetailPopup   _cardDetail;

        private void Awake()
        {
            _closeButton.onClick.AddListener(Hide);
        }

        public void Show(CapybrawlerInstance brawler)
        {
            var b = brawler.Build;
            _nameText.text  = $"{b.Nature.natureType} Capybrawler";
            _statsText.text = $"HP {brawler.CurrentHP}/{brawler.MaxHP}  " +
                              $"ATK {brawler.CurrentATK}  DEF {brawler.CurrentDEF}  SPD {brawler.CurrentSPD}";

            EquipmentData[] equips = { b.Helm, b.Armor, b.Weapon };
            for (int i = 0; i < equips.Length; i++)
            {
                var e = equips[i];
                _equipNameLabels[i].text = $"[{e.slot}] {e.equipmentName}";
                int captured = i;
                _equipButtons[captured].onClick.RemoveAllListeners();
                _equipButtons[captured].onClick.AddListener(() =>
                {
                    if (equips[captured].grantedCard != null)
                        _cardDetail.Show(equips[captured].grantedCard);
                });
            }

            _cardDetail.Hide();
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            _cardDetail.Hide();
            gameObject.SetActive(false);
        }
    }
}
