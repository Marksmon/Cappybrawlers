using Capybrawlers.Cards;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Capybrawlers.UI
{
    public class CardDetailPopup : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _costText;
        [SerializeField] private TextMeshProUGUI _descText;
        [SerializeField] private Button          _closeButton;

        private void Awake()
        {
            _closeButton.onClick.AddListener(Hide);
        }

        public void Show(CardData card)
        {
            _nameText.text = card.cardName;
            _costText.text = $"{card.staminaCost} Stamina";
            _descText.text = card.effectDescription;
            gameObject.SetActive(true);
        }

        public void Hide() => gameObject.SetActive(false);
    }
}
